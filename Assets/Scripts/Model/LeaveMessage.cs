using System;
using System.Collections.Generic;
using System.Linq;

public enum LeaveMessageType
{
	Text,
	Voice,
}

public class LeaveMessage
{
	public uint m_UserID;
	public string m_NickName;
	public LeaveMessageType m_Type;
	public string m_TextMsg;
	public uint m_VoiceID;

	public LeaveMessage() {}

    public LeaveMessage(LeaveMessage rhs)
    {
        m_UserID = rhs.m_UserID;
        m_NickName = rhs.m_NickName;
        m_Type = rhs.m_Type;
        m_TextMsg = rhs.m_TextMsg;
        m_VoiceID = rhs.m_VoiceID;
    }

	public void LoadData(Save_LeaveMessageNode save)
	{
		m_UserID = save.UserId;
		m_NickName = save.UserName;
		m_Type = (LeaveMessageType)save.MsgType;
		switch (m_Type)
		{
			case LeaveMessageType.Text:
				{
					m_TextMsg = save.TextTypeData;
				}
				break;
			case LeaveMessageType.Voice:
				{
					m_TextMsg = save.TextTypeData;
				}
				break;
		}
	}

	public Save_LeaveMessageNode SaveData()
	{
		Save_LeaveMessageNode tSave = new Save_LeaveMessageNode();
		tSave.UserId = m_UserID;
		tSave.UserName = m_NickName;
		tSave.MsgType = (int)m_Type;
		switch (m_Type)
		{
			case LeaveMessageType.Text:
				{
					tSave.TextTypeData = m_TextMsg;
				}
				break;
			case LeaveMessageType.Voice:
				{
					tSave.TextTypeData = m_TextMsg;
				}
				break;
		}
		return tSave;
	}

    // #TODO add a new property for accessing voice id
	public string TextLeaveMessage
	{
		get
		{
			return m_TextMsg;
		}
		set
		{
			m_TextMsg = value;
		}
	}
}

public class LeaveMessageList : IEnumerable<LeaveMessage>
{
	private string m_StrKey = string.Empty;
	private readonly List<int> m_NodeIndexList = new List<int>();
	private readonly List<LeaveMessage> m_LeaveMessage = new List<LeaveMessage>();

    public LeaveMessageList()
    {}

    public LeaveMessageList(string key)
    {
        this.Key = key;
    }

    public LeaveMessageList(IEnumerable<int> nodeIds)
    {
        m_StrKey = GenerateKey(nodeIds);
        m_NodeIndexList.AddRange(nodeIds);
    }

    public LeaveMessageList(LeaveMessageList rhs)
    {
        m_StrKey = rhs.m_StrKey;
        m_NodeIndexList.AddRange(rhs.m_NodeIndexList);
        m_LeaveMessage.AddRange(rhs.m_LeaveMessage);
    }

    public string Key
    {
        get { return m_StrKey; }
        set
        {
            if (value == null)
            {
                throw new ArgumentNullException();
            }
            m_StrKey = value;
            m_NodeIndexList.Clear();
            m_NodeIndexList.AddRange(InternalParseKey(value));
        }
    }

    public int NodeCount
    {
        get { return m_NodeIndexList.Count; }
    }

    public int GetNodeAt(int index)
    {
        return m_NodeIndexList[index];
    }

    public IEnumerable<int> NodeIndices
    {
        get { return m_NodeIndexList; }
        set
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            var key = GenerateKey(value);
            if (key == "")
            {
                throw new ArgumentException("value");
            }

            m_StrKey = key;
            m_NodeIndexList.Clear();
            m_NodeIndexList.AddRange(value);
        }
    }

    public bool ContainsNode(int nodeId)
    {
        return m_NodeIndexList.Contains(nodeId);
    }

    public void RemoveNode(int nodeId)
    {
        if (m_NodeIndexList.Remove(nodeId))
        {
            m_StrKey = GenerateKey(m_NodeIndexList);
        }
    }

    public List<LeaveMessage> LeaveMessages
    {
        get { return m_LeaveMessage; }
    }

	public void Clear()
	{
        m_StrKey = string.Empty;
		m_NodeIndexList.Clear();
		m_LeaveMessage.Clear();
	}

    public void RemoveMessage(LeaveMessage msg)
    {
        m_LeaveMessage.Remove(msg);
    }

    public static string GenerateKey(IEnumerable<int> nodeIds)
    {
        return string.Join("-", nodeIds.Select(x => x.ToString()).ToArray());
    }

    private static IEnumerable<int> InternalParseKey(string key)
    {
        return key.Split('-').Select(x => int.Parse(x));
    }

    public static List<int> ParseKey(string key)
    {
        return InternalParseKey(key).ToList();
    }

    public IEnumerator<LeaveMessage> GetEnumerator()
    {
        return m_LeaveMessage.GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
