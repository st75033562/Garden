using Google.Protobuf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.Assertions;
using System.Linq;
using LitJson;

public delegate void CommandCallback(Command_Result res, ByteString content);

public enum DisconnectReason
{
    None,
    NetworkError,
    SessionExpired,
    Timeout,
    ReconnectLoginFailed,
}

public enum ReconnectState
{
    Reconnecting,
    Reconnected,
    ReconnectFailed
}

/// <summary>
/// A simple tcp client
/// 
/// There're two kinds of callbacks that you can register to listen for responses.
///     1. listener, which is global and is only removed by hand
///     2. request callback, which is provided when sending a request, specific to the request only
/// </summary>
public class SocketManager : Singleton<SocketManager>
{
    private static readonly A8.Logger s_logger = A8.Logger.GetLogger<SocketManager>();

    private struct Request
    {
        public readonly Command_ID cmdId;
        public readonly ByteString content;
        public readonly bool shouldResend;
        public readonly CommandCallback cb;

        public Request(Command_ID cmdId, ByteString content, bool shouldResend, CommandCallback cb)
        {
            this.cmdId = cmdId;
            this.content = content;
            this.shouldResend = shouldResend;
            this.cb = cb;
        }
    }

    public event Action onLoggedIn;
    public event Action<ReconnectState> onReconnectStateChanged;
    public event Action<DisconnectReason> onDisconnected;

    private ISocket socket;
    private Func<ISocket> socketFactory;

    private Action<bool> connectCallback;
    private Action onLoginTimeout;

    private readonly Dictionary<Command_ID, CommandCallback> commandListeners 
        = new Dictionary<Command_ID, CommandCallback>();

    private readonly List<SocketResponse> pendingResponses = new List<SocketResponse>();
    private readonly List<Request> pendingRequests = new List<Request>();

    private const int MaxReconnectCount = 3;
    private const int CheckKeepAliveTimeOut = 15;
    private const int LoginTimeout = 15;

    private int reconnectCount;
    private bool keepAliveTimeOut;
    private bool isReconnectLogin;

    private Coroutine coKeepAlive;
    private Coroutine coLoginTimer;

    private static readonly Dictionary<Command_ID, Command_ID> commandMapping = new Dictionary<Command_ID, Command_ID>();

    public enum State
    {
        Disconnected,
        Connecting,
        Connected,
        LoggingIn,
        LoggedIn,
        Reconnecting
    }

    private State currentState = State.Disconnected;

    static SocketManager()
    {
        RegisterCommandMapping();
    }

    private static void RegisterCommandMapping()
    {
        var names = Enum.GetNames(typeof(Command_ID));
        var values = (Command_ID[])Enum.GetValues(typeof(Command_ID));
        for (int i = 0; i < names.Length; ++i)
        {
            if (names[i].Last() == 'A')
            {
                string requestName = names[i].Substring(0, names[i].Length - 1) + 'R';
                for (int j = 0; j < names.Length; ++j)
                {
                    if (names[j] == requestName)
                    {
                        var ackCmdId = values[i];
                        var reqCmdId = values[j];
                        commandMapping.Add(ackCmdId, reqCmdId);
                    }
                }
            }
        }

        // special
        commandMapping.Add(Command_ID.CmdEmpty, Command_ID.CmdEmpty);
    }

    public SocketManager()
    {
        processingRequests = true;
    }

    public void initialize(Func<ISocket> socketFactory = null)
    {
        this.socketFactory = socketFactory ?? delegate { return new AsyncSocket(); };
    }

    public bool connected
    {
        get { return state >= State.Connected && state < State.Reconnecting; }
    }

    public State state
    {
        get { return currentState; }
        private set
        {
            if (currentState != value)
            {
                currentState = value;
                s_logger.Log(value);
            }
        }
    }


    /// <summary>
    /// serverAddress and serverPort must be set before calling connect
    /// </summary>
    public string serverAddress { get; set; }

    public int serverPort { get; set; }

    /// <summary>
    /// username and password for login, should be set before login
    /// </summary>
    public string username { get; set; }

    public string password { get; set; }

    public uint uid { get; private set; }

    public string token { get; private set; }

    public bool processingRequests { get; set; }

    public void connect(Action<bool> action)
    {
        if (state != State.Disconnected)
        {
            throw new InvalidOperationException();
        }

        pendingRequests.Clear();
        state = State.Connecting;
        internalConnect(action);
    }

    private void internalConnect(Action<bool> action)
    {
        connectCallback = action;
        socket = socketFactory();
        socket.connect(serverAddress, serverPort);
    }

    public void login(Action<Command_Result, CMD_Account_Login_a_Parameters> cb,
                      Action onLoginTimeout)
    {
        if (string.IsNullOrEmpty(username))
        {
            throw new ArgumentException("username");
        }

        if (string.IsNullOrEmpty(password))
        {
            throw new ArgumentException("password");
        }

        if (state != State.Connected)
        {
            throw new InvalidOperationException();
        }

        var login_r = new CMD_Account_Login_r_Parameters();
        login_r.AccountName = username;
        login_r.AccountPass = password;
        JsonData device = new JsonData();
        device["deviceModel"] = UnityEngine.SystemInfo.deviceModel;
        device["deviceName"] = UnityEngine.SystemInfo.deviceName;
        device["deviceType"] = UnityEngine.SystemInfo.deviceType.ToString();
        device["deviceUniqueIdentifier"] = UnityEngine.SystemInfo.deviceUniqueIdentifier;
        device["graphicsMemorySize"] = UnityEngine.SystemInfo.graphicsMemorySize;
        device["graphicsDeviceVendor"] = UnityEngine.SystemInfo.graphicsDeviceVendor;
        device["operatingSystem"] = UnityEngine.SystemInfo.operatingSystem;
        device["systemMemorySize"] = UnityEngine.SystemInfo.systemMemorySize;
        device["processorType"] = UnityEngine.SystemInfo.processorType;
        device["version"] = Application.version;

        login_r.DeviceInfo = device.ToJson();
        this.onLoginTimeout = onLoginTimeout;
        coLoginTimer = StartCoroutine(loginTimer());
        state = State.LoggingIn;
        send(Command_ID.CmdAccountLoginR, login_r.ToByteString(), false, (res, data) => {
                this.onLoginTimeout = null;
                if (coLoginTimer != null)
                {
                    StopCoroutine(coLoginTimer);
                    coLoginTimer = null;
                }

                var response = CMD_Account_Login_a_Parameters.Parser.ParseFrom(data);
                if (res == Command_Result.CmdAccountExpired || res == Command_Result.CmdNoError)
                {
                    state = State.LoggedIn;
                    uid = response.UserInfo.UserId;
                    token = response.AccountToken;

                    if (onLoggedIn != null)
                    {
                        onLoggedIn();
                    }
                }
                else
                {
                    state = State.Connected;
                }

                if (cb != null)
                {
                    cb(res, response);
                }
            });
    }

    private IEnumerator loginTimer()
    {
        yield return new WaitForSecondsRealtime(LoginTimeout);

        s_logger.Log("login timed out");
        if (onLoginTimeout != null)
        {
            onLoginTimeout();
            onLoginTimeout = null;
        }
        internalDisconnect(DisconnectReason.Timeout, !isReconnectLogin);
        coLoginTimer = null;
    }

    #region command listeners
    public void setListener(Command_ID cmdId, CommandCallback action)
    {
        if (action == null)
        {
            throw new ArgumentNullException("action");
        }

        commandListeners[cmdId] = action;
    }

    public void removeListener(Command_ID comId, CommandCallback action)
    {
        removeListener(comId, (Delegate)action);
    }

    private void removeListener(Command_ID cmdId, Delegate action)
    {
        if (commandListeners.ContainsKey(cmdId) && commandListeners[cmdId].Equals(action))
        {
            commandListeners.Remove(cmdId);
        }
    }

    public void removeAllListeners()
    {
        commandListeners.Clear();
    }
    #endregion

    public void send(Command_ID cmdId, ByteString content)
    {
        send(cmdId, content, (CommandCallback)null);
    }

    public void send(Command_ID cmdId, ByteString content, CommandCallback cb)
    {
        send(cmdId, content, true, cb);
    }

    public void send(Command_ID cmdId, IMessage message, CommandCallback cb)
    {
        send(cmdId, message.ToByteString(), cb);
    }

    private void send(Command_ID cmdId, ByteString content, bool shouldResend, CommandCallback cb)
    {
        pendingRequests.Add(new Request(cmdId, content, shouldResend, cb));
        internalSend(cmdId, content);
    }

    private void internalSend(Command_ID cmdId, ByteString content)
    {
        if (socket == null)
        {
            s_logger.Log("socket not connected, do not send " + cmdId);
            return;
        }

        CMD cmd = new CMD();
        cmd.CmdId = (uint)cmdId;
        if(content != null)
        {
            cmd.CmdParameters = content;
        }
        cmd.UserId = uid;
        if (!string.IsNullOrEmpty(token))
        {
            // #TODO cache token
            cmd.UserToken = ByteString.CopyFrom(token, System.Text.Encoding.UTF8);
        }

        byte[] buf = cmd.ToByteArray();
        int length = IPAddress.HostToNetworkOrder(buf.Length + 4);  //网络字节序
        byte[] bufLength = BitConverter.GetBytes(length);
        byte[] packetBuffer = new byte[buf.Length + 4];

        Array.Copy(bufLength, 0, packetBuffer, 0, 4);
        Array.Copy(buf, 0, packetBuffer, 4, buf.Length);

        socket.send(packetBuffer);
    }

    private void removeRequest(Command_ID cmdId)
    {
        var index = pendingRequests.FindIndex(x => x.cmdId == cmdId);
        if (index != -1)
        {
            pendingRequests.RemoveAt(index);
        }
    }

    // Update is called once per frame
    void Update() {
        if (!processingRequests || socket == null)
        {
            return;
        }

        socket.getResponses(pendingResponses);

        for (int i = 0; i < pendingResponses.Count; i++)
        {
            if (pendingResponses[i].type == SocketResponseType.Data)
            {
                CMD cmd = CMD.Parser.ParseFrom(pendingResponses[i].data);
                var requestCallback = acknowledgeRequest(cmd.Id);

                // token expired for non-login request, need logout
                if (cmd.Result == Command_Result.CmdAuthError && cmd.Id != Command_ID.CmdAccountLoginA)
                {
                    s_logger.Log("session expired");
                    internalDisconnect(DisconnectReason.SessionExpired, true);
                    break;
                }

                if (requestCallback != null)
                {
                    invokeCallback(requestCallback, cmd);
                }

                CommandCallback listener;
                if (commandListeners.TryGetValue(cmd.Id, out listener))
                {
                    invokeCallback(listener, cmd);
                }
            }
            else if (pendingResponses[i].type == SocketResponseType.Connected)
            {
                state = State.Connected;
                connectCallback(true);
                connectCallback = null;
                Assert.IsNull(coKeepAlive);
                coKeepAlive = StartCoroutine(keepAlive());
            }
            else if (pendingResponses[i].type == SocketResponseType.ConnectException)
            {
                if (state != State.Reconnecting)
                {
                    state = State.Disconnected;
                }
                closeSocket();
                connectCallback(false);
                connectCallback = null;
            }
            else if (pendingResponses[i].type >= SocketResponseType.Exception)
            {
                if (pendingRequests.Count > 0 && !pendingRequests[0].shouldResend)
                {
                    pendingRequests.RemoveAt(0);
                }

                s_logger.Log("error: " + pendingResponses[i].type);
                if (state != State.Reconnecting)
                {
                    closeSocket();
                    StartCoroutine(reconnect(DisconnectReason.NetworkError));
                }
            }
        }

        pendingResponses.Clear();
    }

    private void invokeCallback(CommandCallback cb, CMD cmd)
    {
        try
        {
            if (cb.IsSafeToInvoke())
            {
                cb.Invoke(cmd.Result, cmd.CmdParameters);
            }
        }
        catch (Exception e)
        {
            s_logger.LogException(e);
        }
    }

    private CommandCallback acknowledgeRequest(Command_ID responseCmdId)
    {
        CommandCallback cb = null;
        Command_ID requestCmdId;
        if (commandMapping.TryGetValue(responseCmdId, out requestCmdId))
        {
            for (int i = 0; i < pendingRequests.Count; i++)
            {
                if (pendingRequests[i].cmdId == requestCmdId)
                {
                    cb = pendingRequests[i].cb;
                    pendingRequests.RemoveAt(i);
                }
            }
        }
        return cb;
    }

    IEnumerator keepAlive()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(CheckKeepAliveTimeOut);
            if (keepAliveTimeOut)
            {
                s_logger.Log("keep alive timed out");

                removeRequest(Command_ID.CmdEmpty);
                socket.clearAndClose();
                StartCoroutine(reconnect(DisconnectReason.Timeout));
                yield break;
            }

            send(Command_ID.CmdEmpty, null, false, delegate {
                keepAliveTimeOut = false;
            });
            keepAliveTimeOut = true;
        }
    }

    private void stopKeepAlive()
    {
        if (coKeepAlive != null)
        {
            StopCoroutine(coKeepAlive);
            coKeepAlive = null;
            keepAliveTimeOut = false;
        }
    }

    public void reconnect()
    {
        if (state != State.Disconnected)
        {
            throw new InvalidOperationException();
        }

        StartCoroutine(reconnect(DisconnectReason.None));
    }

    IEnumerator reconnect(DisconnectReason reason)
    {
        s_logger.Log("reconnect ==>" + reconnectCount);
        if (reconnectCount++ < MaxReconnectCount)
        {
            if (state != State.Reconnecting)
            {
                stopKeepAlive();

                state = State.Reconnecting;
                if (onReconnectStateChanged != null)
                {
                    onReconnectStateChanged(ReconnectState.Reconnecting);
                }
            }

            yield return new WaitForSecondsRealtime(2);

            internalConnect((success) => {
                s_logger.Log("Reconnect: " + success);
                if (success)
                {
                    reconnectCount = 0;
                    reconnectLogin();
                }
                else
                {
                    StartCoroutine(reconnect(reason));
                }
            });
        }
        else
        {
            s_logger.Log("reconnection reached max try");
            internalDisconnect(reason, false);
        }
    }

    private void reconnectLogin()
    {
        if (uid != 0)
        {
            s_logger.Log("user was logged in, try to login again");
            isReconnectLogin = true;
            login((res, response) =>
            {
                isReconnectLogin = false;
                if (res == Command_Result.CmdNoError || res == Command_Result.CmdAuthError)
                {
                    token = response.AccountToken;
                    s_logger.Log("login succeeded, token: " + token);

                    // #TODO it would be better if we could know the last successfully executed command id
                    // do we need to resend the requests when auth error ?
                    pendingRequests.RemoveAll(x => !x.shouldResend);
                    s_logger.Log("pending requests: " + pendingRequests.Count);

                    for (int i = 0; i < pendingRequests.Count; i++)
                    {
                        s_logger.Log("resend: " + pendingRequests[i].cmdId);
                        internalSend(pendingRequests[i].cmdId, pendingRequests[i].content);
                    }

                    state = State.LoggedIn;
                    if (onReconnectStateChanged != null)
                    {
                        onReconnectStateChanged(ReconnectState.Reconnected);
                    }
                }
                else
                {
                    s_logger.Log("login failed: " + res);
                    internalDisconnect(DisconnectReason.ReconnectLoginFailed, false);
                }
            }, null);
        }
        else
        {
            s_logger.Log("user was not logged in, reconnection done");
            state = State.Connected;
            if (onReconnectStateChanged != null)
            {
                onReconnectStateChanged(ReconnectState.Reconnected);
            }
        }
    }

    public void disconnect()
    {
        internalDisconnect(DisconnectReason.None, true);
    }

    private void internalDisconnect(DisconnectReason reason, bool clearPendingRequest)
    {
        if (state != State.Disconnected)
        {
            var oldState = state;
            state = State.Disconnected;

            closeSocket();
            pendingResponses.Clear();
            if (clearPendingRequest)
            {
                pendingRequests.Clear();
            }
            stopKeepAlive();
            StopAllCoroutines();
            reconnectCount = 0;
            coLoginTimer = null;

            var wasReconnectLogin = isReconnectLogin;
            isReconnectLogin = false;

            if ((oldState == State.Reconnecting || wasReconnectLogin) && onReconnectStateChanged != null)
            {
                onReconnectStateChanged(ReconnectState.ReconnectFailed);
            }

            if (onDisconnected != null)
            {
                onDisconnected(reason);
            }
        }
    }

    private void closeSocket()
    {
        if (socket != null)
        {
            socket.clearAndClose();
            socket = null;
        }
    }

    public bool hasOutgoingRequests
    {
        get { return pendingRequests.Count > 0; }
    }
}
