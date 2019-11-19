using Google.Protobuf.Collections;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Message
{
    public static readonly Message defaultMessage = new Message("message1", NameScope.Local);

    public Message(string name, NameScope scope)
    {
        this.name = name;
        this.scope = scope;
        this.targetRobotIndices = new List<int>();
    }

    public string name { get; private set; }

    public NameScope scope { get; private set; }

    /// <summary>
    /// target robot indices, empty means the message is for all robots
    /// </summary>
    public List<int> targetRobotIndices { get; private set; }
}

public interface IMessageListener
{
    void onMessage(Message msg);
}

public class MessageManager : IEnumerable<Message>
{
    public event Action<Message> onMessageDeleted;
    public event Action<Message> onMessageAdded;

    private readonly List<Message> m_messages = new List<Message>();
    private readonly List<IMessageListener> m_listeners = new List<IMessageListener>();

    public int count
    {
        get { return m_messages.Count; }
    }

    public void add(Message msg)
    {
        if (has(msg.name))
        {
            throw new ArgumentException("duplicate");
        }

        m_messages.Add(msg);

        if (onMessageAdded != null)
        {
            onMessageAdded(msg);
        }
    }

    public Message get(string name)
    {
        return m_messages.Find(x => x.name == name);
    }

    public bool has(string name)
    {
        return get(name) != null;
    }

    public void delete(string name)
    {
        int index = m_messages.FindIndex(x => x.name == name);
        if (index != -1)
        {
            var msg = m_messages[index];
            m_messages.RemoveAt(index);
            if (onMessageDeleted != null)
            {
                onMessageDeleted(msg);
            }
        }
    }

    public void clear()
    {
        m_messages.Clear();
    }

    public void reset()
    {
        m_messages.Clear();
        m_listeners.Clear();
    }

    public void addListener(IMessageListener listener)
    {
        m_listeners.Add(listener);
    }

    public void removeListener(IMessageListener listener)
    {
        m_listeners.Remove(listener);
    }

    public void broadcast(string name)
    {
        var msg = get(name);
        if (msg != null)
        {
            foreach (var listener in m_listeners)
            {
                listener.onMessage(msg);
            }
        }
    }

    public IEnumerable<Message> localMessages
    {
        get { return m_messages.Where(x => x.scope == NameScope.Local); }
    }

    public IEnumerable<Message> globalMessages
    {
        get { return m_messages.Where(x => x.scope == NameScope.Global); }
    }

    public IEnumerator<Message> GetEnumerator()
    {
        return m_messages.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void load(Save_ProjectData saveData)
    {
        m_messages.Clear();
        loadMessages(saveData.LocalMessages, saveData.LocalMessageExtraData, NameScope.Local);
        loadMessages(saveData.GlobalMessages, saveData.GlobalMessageExtraData, NameScope.Global);
    }

    private void loadMessages(RepeatedField<string> messages,
                              RepeatedField<Save_MessageExtraData> extraData,
                              NameScope scope)
    {
        for (int i = 0; i < messages.Count; ++i)
        {
            var msg = new Message(messages[i], scope);
            if (extraData.Count > 0)
            {
                msg.targetRobotIndices.AddRange(extraData[i].RobotIndices);
            }
            m_messages.Add(msg);
        }
    }

    public void save(Save_ProjectData saveData)
    {
        foreach (var msg in m_messages)
        {
            var extraData = new Save_MessageExtraData();
            extraData.RobotIndices.Add(msg.targetRobotIndices);

            if (msg.scope == NameScope.Global)
            {
                saveData.GlobalMessages.Add(msg.name);
                saveData.GlobalMessageExtraData.Add(extraData);
            }
            else
            {
                saveData.LocalMessages.Add(msg.name);
                saveData.LocalMessageExtraData.Add(extraData);
            }
        }
    }
}
