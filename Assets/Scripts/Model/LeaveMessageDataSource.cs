using Google.Protobuf;
using System.Collections.Generic;
using System.Linq;

public class LeaveMessageDataSource
{
    private readonly Dictionary<string, LeaveMessageList> m_messages = new Dictionary<string, LeaveMessageList>();

	public void addMessage(string key, LeaveMessage data)
	{
        LeaveMessageList msgList;
		if (!m_messages.TryGetValue(key, out msgList))
		{
			msgList = new LeaveMessageList(key);
            m_messages.Add(key, msgList);
		}
        msgList.LeaveMessages.Add(data);
	}

	public bool addMessage(LeaveMessageList data)
	{
		LeaveMessageList msgList = null;
		if (m_messages.TryGetValue(data.Key, out msgList))
		{
			msgList.LeaveMessages.AddRange(data.LeaveMessages);
            return false;
		}
		else
		{
			m_messages.Add(data.Key, data);
            return true;
		}
	}

	public LeaveMessageList getMessage(string key)
	{
		LeaveMessageList msgList;
		m_messages.TryGetValue(key, out msgList);
        return msgList;
	}

    public bool hasMessage(string key)
    {
        return m_messages.ContainsKey(key);
    }

    public void deleteMessage(string key)
    {
        m_messages.Remove(key);
    }

    public int messageCount
    {
        get { return m_messages.Count; }
    }

    public IEnumerable<LeaveMessageList> messages
    {
        get { return m_messages.Values; }
    }

    public bool hasUserMessages(uint userId)
    {
        return m_messages.Values.SelectMany(x => x).Any(x => x.m_UserID == userId);
    }

    public void loadMessages(byte[] data)
    {
        m_messages.Clear();
        if(data == null) {
            return;
        }
		var messages = Save_LeaveMessageData_All.Parser.ParseFrom(data);
        for (int i = 0; i < messages.MsgList.Count; ++i)
        {
            Save_LeaveMessageData messageData = messages.MsgList[i];
            var msgList = new LeaveMessageList();
            msgList.Key = messageData.MsgKey;

            for (int j = 0; j < messageData.MsgNodeList.Count; ++j)
            {
                LeaveMessage msg = new LeaveMessage();
                msg.LoadData(messageData.MsgNodeList[j]);
                msgList.LeaveMessages.Add(msg);
            }
            m_messages.Add(msgList.Key, msgList);
        }
    }

    public byte[] serializeMessages()
    {
		var tAllMsg = new Save_LeaveMessageData_All();
		foreach (var item in m_messages)
		{
			Save_LeaveMessageData tMsg = new Save_LeaveMessageData();
			tMsg.MsgKey = item.Value.Key;
			for (int i = 0; i < item.Value.LeaveMessages.Count; ++i)
			{
				tMsg.MsgNodeList.Add(item.Value.LeaveMessages[i].SaveData());
			}
			tAllMsg.MsgList.Add(tMsg);
		}

		return tAllMsg.ToByteArray();
    }
}
