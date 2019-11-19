using Google.Protobuf;
using UnityEngine;
using System;

public class SocketRequest : CustomYieldInstruction
{
    private bool m_sent;
    private bool m_done;
    private readonly SocketManager m_socketManager;
    private readonly Command_ID m_cmdId;
    private readonly IMessage m_msg;
    protected CommandCallback m_callback;
    private bool m_aborted;
    private object m_typedResponse;

    public SocketRequest(Command_ID id, IMessage msg)
        : this(SocketManager.instance, id, msg)
    {
    }

    public SocketRequest(SocketManager manager, Command_ID id, IMessage msg)
    {
        if (manager == null)
        {
            throw new ArgumentNullException("manager");
        }
        if (msg == null)
        {
            throw new ArgumentNullException("msg");
        }

        m_socketManager = manager;
        m_cmdId = id;
        m_msg = msg;
    }

    public void Send()
    {
        if (m_sent)
        {
            throw new InvalidOperationException();
        }
        m_sent = true;
        m_socketManager.send(m_cmdId, m_msg, OnCommandCallback);
    }

    public void Abort()
    {
        if (!m_sent)
        {
            return;
        }
        m_aborted = true;
    }

    public SocketRequest On(CommandCallback callback)
    {
        if (callback == null)
        {
            throw new ArgumentNullException("callback");
        }

        m_callback = callback;
        return this;
    }

    public SocketRequest On<T>(Action<Command_Result, T> callback) where T : IMessage<T>, new()
    {
        if (callback == null)
        {
            throw new ArgumentNullException("callback");
        }

        m_callback = (result, data) => {
            callback(result, Response<T>());
        };

        return this;
    }

    public T Response<T>() where T : IMessage<T>, new()
    {
        if (response == null)
        {
            return default(T);
        }

        if (m_typedResponse == null)
        {
            m_typedResponse = new MessageParser<T>(() => new T()).ParseFrom(response);
        }
        return (T)m_typedResponse;
    }

    private void OnCommandCallback(Command_Result res, ByteString data)
    {
        if (m_aborted) { return; }

        result = res;
        response = data;
        m_done = true;
        if (m_callback != null)
        {
            m_callback(res, data);
        }
    }

    public Command_Result result { get; private set; }

    public ByteString response { get; private set; }

    public override bool keepWaiting
    {
        get { return !m_done && !m_aborted; }
    }
}

public static class SocketManagerExtensions
{
    public static SocketRequest sendAsync<T>(this SocketManager manager, Command_ID id, T msg) 
        where T : IMessage<T>, new()
    {
        var request = new SocketRequest(manager, id, msg);
        request.Send();
        return request;
    }
}
