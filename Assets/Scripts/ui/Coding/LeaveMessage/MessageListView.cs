using System;
using UnityEngine;

public class MessageListView : MonoBehaviour
{
	public GameObject m_TextLableTemplate;
	public GameObject m_VoiceTemplate;
	public RectTransform m_ContentTrans;

    private bool m_isReadOnly;

    public bool IsReadOnly
    {
        get { return m_isReadOnly; }
        set
        {
            m_isReadOnly = value;
            foreach (Transform child in m_ContentTrans)
            {
                child.GetComponent<MessageElementBase>().Deletable = !m_isReadOnly;
            }
        }
    }

    public void SetMessages(LeaveMessageList messages)
    {
        if (messages == null)
        {
            throw new ArgumentNullException("messages");
        }

        for (int i = m_ContentTrans.childCount - 1; i >= 0; --i)
        {
            var child = m_ContentTrans.GetChild(i);
            child.SetParent(null);
            Destroy(child.gameObject);
        }

        for (int i = messages.LeaveMessages.Count - 1; i >= 0; --i)
        {
            LeaveMessage msg = messages.LeaveMessages[i];
            GameObject messageObj = null;
            switch (msg.m_Type)
            {
            case LeaveMessageType.Text:
                messageObj = Instantiate(m_TextLableTemplate, m_ContentTrans);
                break;
            case LeaveMessageType.Voice:
                messageObj = Instantiate(m_VoiceTemplate, m_ContentTrans);
                break;
            default:
                throw new ArgumentException("invalid message type: " + msg.m_Type);
            }
            messageObj.SetActive(true);

            var element = messageObj.GetComponent<MessageElementBase>();
            element.Deletable = !m_isReadOnly;
            element.AvatarService = AvatarService;
            element.Message = msg;
            element.MessageIndex = i;
            element.MessageKey = messages.Key;
        }
    }

    public IAvatarService AvatarService
    {
        get;
        set;
    }
}
