using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;

public enum SocketResponseType
{
    Data,
    Connected,
    ConnectException,
    Exception = 100,
    SendException,
    NotConnected,
    ReceiveException,
    Closed
}

public struct SocketResponse
{
    public readonly byte[] data;
    public readonly SocketResponseType type;

    public SocketResponse(SocketResponseType type)
        : this(0, type)
    {
    }

    public SocketResponse(int length, SocketResponseType type = SocketResponseType.Data)
    {
        if (length > 0)
        {
            this.data = new byte[length];
        }
        else
        {
            this.data = null;
        }
        this.type = type;
    }
}

public interface ISocket
{
    int connectionTimeout { get; set; }
    void connect(string host, int port);
    bool connected { get; }
    void send(byte[] data);
    void getResponses(List<SocketResponse> packets);
    void clearAndClose();
}

public class AsyncSocket : ISocket
{
    private static readonly A8.Logger s_logger = A8.Logger.GetLogger<AsyncSocket>();

    private const int HeaderLength = 4;
    private const int DefaultConnectTimeout = 15000;

    private Socket socket;
    private readonly object m_gate = new object();
    private readonly Queue<byte[]> requests = new Queue<byte[]>();
    private readonly Queue<SocketResponse> responses = new Queue<SocketResponse>();
    private bool isSending;

    private enum State
    {
        NotConnected,
        Connecting,
        Connected,
        Closed,
    }

    private State m_state = State.NotConnected;
    private System.Timers.Timer m_connectionTimer;

    public AsyncSocket()
    {
        connectionTimeout = DefaultConnectTimeout;
    }

    public AsyncSocket(Socket socket)
        : this()
    {
        if (socket == null)
        {
            throw new ArgumentNullException();
        }
        m_state = State.Connected;
        this.socket = socket;
        asyncReceive(true, 0, HeaderLength);
    }

    public int connectionTimeout
    {
        get;
        set;
    }

    public void connect(string host, int port)
    {
        lock (m_gate)
        {
            if (m_state != State.NotConnected)
            {
                throw new InvalidOperationException();
            }

            m_state = State.Connecting;
        }

        Dns.BeginGetHostAddresses(host, result => {
            try
            {
                IPAddress[] ips = Dns.EndGetHostAddresses(result);
                if (ips.Length > 0)
                {
                    connectTo(ips, 0, port,
                        () => {
                            addResponse(SocketResponseType.Connected);
                            asyncReceive(true, 0, HeaderLength);
                        },
                        () => {
                            s_logger.LogError("Failed to connect to {0} after trying all dns records", host);
                            close(SocketResponseType.ConnectException);
                        });
                }
                else
                {
                    s_logger.LogError("no dns record for host: " + host);
                    close(SocketResponseType.ConnectException);
                }
            }
            catch (Exception e)
            {
                s_logger.LogException(e);
                close(SocketResponseType.ConnectException);
            }
        }, null);
    }

    private void connectTo(IPAddress[] addresses, int index, int port, Action onSuccess, Action onFailure)
    {
        var ip = addresses[index];
        s_logger.Log("connecting to ip: {0}, family: {1}", ip, ip.AddressFamily);

        lock (m_gate)
        {
            if (m_state != State.Connecting)
            {
                // already closed
                return;
            }

            this.socket = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            this.socket.BeginConnect(ip, port, asyncResult => {
                var socket = (Socket)asyncResult.AsyncState;
                try
                {
                    socket.EndConnect(asyncResult);
                    lock (m_gate)
                    {
                        if (m_state == State.Connecting)
                        {
                            m_state = State.Connected;
                        }
                        cancelConnectionTimer();
                    }
                    onSuccess();
                }
                catch (ObjectDisposedException)
                {
                    s_logger.Log("connection canceled");
                    onFailure();
                }
                catch (Exception e)
                {
                    s_logger.LogException(e);

                    if (index + 1 < addresses.Length)
                    {
                        socket.Close();
                        connectTo(addresses, index + 1, port, onSuccess, onFailure);
                    }
                    else
                    {
                        onFailure();
                    }
                }
            }, this.socket);

            if (m_connectionTimer == null)
            {
                var timeoutTime = connectionTimeout > 0 ? connectionTimeout : DefaultConnectTimeout;
                m_connectionTimer = new System.Timers.Timer(timeoutTime);
                m_connectionTimer.AutoReset = false;
                m_connectionTimer.Elapsed += (sender, e) => {
                    s_logger.Log("connection timeout");
                    close(SocketResponseType.ConnectException, State.NotConnected);
                };
                m_connectionTimer.Enabled = true;
            }
        }
    }

    private void cancelConnectionTimer()
    {
        if (m_connectionTimer != null)
        {
            m_connectionTimer.Close();
            m_connectionTimer = null;
        }
    }

    public bool connected
    {
        get
        {
            lock (m_gate) { return m_state == State.Connected; }
        }
    }

    public void send(byte[] data)
    {
        lock (m_gate)
        {
            if (m_state != State.Connected)
            {
                s_logger.LogError("socket not connected");
                return;
            }

            if (!isSending)
            {
                if (requests.Count > 0)
                {
                    send(requests.Dequeue(), 0);
                }
                else
                {
                    send(data, 0);
                }
                isSending = true;
            }
            else
            {
                requests.Enqueue(data);
            }
        }
    }

    private void send(byte[] buf, int offset)
    {
        try
        {
            var curSocket = socket;
            // already closed
            if (curSocket == null)
            {
                return;
            }

            curSocket.BeginSend(buf, offset, buf.Length - offset, SocketFlags.None, asyncResult => {
                try
                {
                    int sentBytes = curSocket.EndSend(asyncResult);
                    if (sentBytes < buf.Length - offset)
                    {
                        send(buf, offset + sentBytes);
                        return;
                    }

                    byte[] pendingBuf = null;
                    lock (m_gate)
                    {
                        if (requests.Count > 0)
                        {
                            pendingBuf = requests.Dequeue();
                        }
                        else
                        {
                            isSending = false;
                        }
                    }
                    if (pendingBuf != null)
                    {
                        send(pendingBuf, 0);
                    }
                }
                catch (ObjectDisposedException)
                {
                    s_logger.Log("already closed");
                }
                catch (SocketException ex)
                {
                    s_logger.LogException(ex);
                    close(SocketResponseType.SendException);
                }
            }, null);
        }
        catch (ObjectDisposedException)
        {
            // ignore
            s_logger.Log("already closed");
        }
    }

    void asyncReceive(bool isHead, int offset, int size, SocketResponse? response = null)
    {
        if (response == null)
        {
            response = new SocketResponse(size);
        }

        var curSocket = socket;
        if (curSocket == null)
        {
            // already closed
            return;
        }
        try
        {
            curSocket.BeginReceive(response.Value.data, offset, size, SocketFlags.None, asyncResult => {
                try
                {
                    int length = curSocket.EndReceive(asyncResult); //return real read length 
                    if (length == 0)
                    {
                        close(SocketResponseType.Closed);
                        return;
                    }
                    if (length < size)
                    {
                        asyncReceive(isHead, offset + length, size - length, response);
                        return;
                    }
                }
                catch (ObjectDisposedException)
                {
                    s_logger.Log("already closed");
                }
                catch (SocketException ex)
                {
                    s_logger.LogException(ex);
                    close(SocketResponseType.ReceiveException);
                    return;
                }

                if (isHead)
                {
                    //得到的网络字节序长度转换成主机字节序的长度，服务端返回的长度包含头的长度，所以减去头的大小
                    size = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(response.Value.data, 0)) - HeaderLength;
                }
                else
                {
                    addResponse(response.Value);
                    size = HeaderLength;
                }
                asyncReceive(!isHead, 0, size);
            }, null);
        }
        catch (ObjectDisposedException)
        {
            // ignore
            s_logger.Log("already closed");
        }
    }

    void addResponse(SocketResponse buffer)
    {
        lock (m_gate)
        {
            responses.Enqueue(buffer);
        }
    }

    void addResponse(SocketResponseType state)
    {
        addResponse(new SocketResponse(state));
    }

    public void getResponses(List<SocketResponse> packets)
    {
        if (packets == null)
        {
            throw new ArgumentNullException();
        }

        lock (m_gate)
        {
            packets.AddRange(responses);
            responses.Clear();
        }
    }

    private void close(SocketResponseType? resp, State state = State.Closed)
    {
        lock (m_gate)
        {
            if (m_state == State.NotConnected || m_state == State.Closed)
            {
                return;
            }

            cancelConnectionTimer();
            m_state = state;

            if (socket != null)
            {
                try
                {
                    socket.Shutdown(SocketShutdown.Both);
                }
                catch (Exception e)
                {
                    s_logger.LogException(e);
                }
                socket.Close();
                socket = null;
            }

            if (resp != null)
            {
                responses.Enqueue(new SocketResponse(resp.Value));
            }
        }
    }

    // #TODO properly shutdown the socket
    //       1. close the send queue and shutdown the writer
    //       2. read until no more data
    public void clearAndClose()
    {
        close(null);

        lock (m_gate)
        {
            requests.Clear();
            isSending = false;

            responses.Clear();
        }
    }
}
