using System;
using System.Collections.Generic;

public class MessageHandler : IMessageListener
{
    private HashSet<string> m_queuedMsgs = new HashSet<string>();
    private HashSet<string> m_firedMsgs = new HashSet<string>();

    public MessageHandler(MessageManager manager)
    {
        if (manager == null)
        {
            throw new ArgumentNullException("manager");
        }
        manager.addListener(this);
    }

    public void Broadcast(string msg)
    {
        if (msg == null)
        {
            throw new ArgumentNullException("msg");
        }
        m_queuedMsgs.Add(msg);
    }

    public bool IsBroadcasted(string msg)
    {
        if (msg == null)
        {
            throw new ArgumentNullException("msg");
        }
        return m_firedMsgs.Contains(msg);
    }

    public void Update()
    {
        Utils.Swap(ref m_queuedMsgs, ref m_firedMsgs);
        m_queuedMsgs.Clear();
    }

    public void Reset()
    {
        m_queuedMsgs.Clear();
        m_firedMsgs.Clear();
    }

    public void onMessage(Message msg)
    {
        Broadcast(msg.name);
    }
}
