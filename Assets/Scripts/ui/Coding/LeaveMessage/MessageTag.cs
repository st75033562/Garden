using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class MessageTag : MonoBehaviour
{
	public LeaveMessagePanel m_MessagePanel;
    public EventBus m_EventManager;
	public GameObject m_CloseImage;
	public GameObject m_OpenImage;
	public GameObject m_BtnList;
	public RectTransform m_ViewParent;
	public GameObject m_MessageBtnTemplate;
	public GameObject[] m_ShowList;
    public MessageListView m_MessageListView;

    // all message buttons, including merged messages
	List<MessageTagBtn> m_MsgBtns = new List<MessageTagBtn>();
	MessageTagBtn m_CurActiveMsg;
	FunctionNode m_HeadNode;
	List<MessageTagBtn> m_MergedMsgBtns = new List<MessageTagBtn>();

	bool m_bOpen = false;

	void Awake()
	{
		m_EventManager.AddListener(EventId.UpdateLeaveMessage, UpdateLeaveMessageCallback);
		m_EventManager.AddListener(EventId.ChangeLeaveMessageKey, OnMessageKeyChanged);
	}

    private void OnDestroy()
    {
		m_EventManager.RemoveListener(EventId.UpdateLeaveMessage, UpdateLeaveMessageCallback);
		m_EventManager.RemoveListener(EventId.ChangeLeaveMessageKey, OnMessageKeyChanged);
    }

    void LateUpdate()
	{
		if (m_HeadNode)
		{
			UpdateTagPos();
		}
	}

    public bool IsOpen
    {
        get { return m_bOpen; }
    }

	public void ClickTag()
	{
		if (!m_MessagePanel.IsEditMode())
		{
			m_MessagePanel.SetActive(true);
		}
        // toggle the open state
		m_bOpen = !m_bOpen;
		if (m_bOpen)
		{
            if (m_MessagePanel.ActiveTag && m_MessagePanel.ActiveTag != this)
            {
                m_MessagePanel.ReleaseActiveTag();
            }
            m_MessagePanel.SetActiveTag(this);
		}
		else
		{
            Assert.IsTrue(m_MessagePanel.ActiveTag == this);
            m_MessagePanel.SetActiveTag(null);
		}
        m_MessageListView.gameObject.SetActive(m_bOpen);

		if (m_MsgBtns.Count != 1)
		{
            if (m_bOpen != m_CurActiveMsg.IsOpen)
            {
                Refresh();
                if (!m_bOpen)
                {
                    // reset to the first message
                    m_CurActiveMsg = m_MsgBtns[0];
                }
            }
		}
        else
        {
            Refresh();
        }
	}

	public void CloseDetail()
	{
		m_bOpen = false;
        Refresh();
	}

	void RefreshFlag()
	{
		m_CloseImage.SetActive(!m_bOpen);
		m_OpenImage.SetActive(m_bOpen);
        m_BtnList.SetActive(m_bOpen && m_MsgBtns.Count > 1);
	}

	public void UpdateLeaveMessageCallback(object param)
	{
		string msgKey = (string)param;
		List<int> tNodeIndexList = LeaveMessageList.ParseKey(msgKey);
		if (m_HeadNode.NodeIndex != tNodeIndexList[0])
		{
            // ignore the updated message which is either not our or merged message, 
            bool isMergedMsg = m_MergedMsgBtns.Any(x => x.Key == msgKey);
			if (!isMergedMsg)
			{
				return;
			}
		}

        var tTagBtn = m_MsgBtns.Find(x => x.Key == msgKey);
		if (tTagBtn == null)
		{
            // a new message
			AddMessageTagBtn(msgKey);
            Refresh();
		}
		else
		{
			var messageList = m_MessagePanel.MessageDataSource.getMessage(tTagBtn.Key);
            // if the message is deleted
			if (messageList == null || 0 == messageList.LeaveMessages.Count)
			{
                RemoveMsgBtn(tTagBtn);
			}
			else
			{
                Refresh();
			}
		}
	}

    private void RemoveMsgBtn(MessageTagBtn btn)
    {
        m_MessagePanel.SetMessageOpened(btn.Key, false);
        m_MsgBtns.Remove(btn);
        m_MergedMsgBtns.Remove(btn);
        Destroy(btn.gameObject);

        // if the only left messages are all from other blocks
        if (m_MsgBtns.Count == m_MergedMsgBtns.Count)
        {
            if (0 != m_MergedMsgBtns.Count)
            {
                m_EventManager.AddEvent(EventId.PickUpNode, null);
            }
            if (btn == m_CurActiveMsg && m_CurActiveMsg.IsOpen)
            {
                m_MessageListView.gameObject.SetActive(false);
            }
            m_MessagePanel.RemoveEmptyMessageTag(this);
        }
        else
        {
            if (btn == m_CurActiveMsg)
            {
                m_CurActiveMsg = m_MsgBtns[0];
            }
            Refresh();
        }
    }

	void RefreshMsgListView()
	{
		if (m_CurActiveMsg.IsOpen)
		{
            var messages = m_MessagePanel.MessageDataSource.getMessage(m_CurActiveMsg.Key);
            m_MessageListView.SetMessages(messages);
        }
	}

	void OnMessageKeyChanged(object param)
	{
		bool bChanged = false;
		List<MessageKeyChangedEvent> tChange = (List<MessageKeyChangedEvent>)param;
		for (int changeIndex = 0; changeIndex < tChange.Count; ++changeIndex)
		{
			for (int btnIndex = 0; btnIndex < m_MsgBtns.Count; ++btnIndex)
			{
				if (m_MsgBtns[btnIndex].Key == tChange[changeIndex].m_OldKey)
				{
					m_MsgBtns[btnIndex].Key = tChange[changeIndex].m_NewKey;
					bChanged = true;
				}
			}
		}
		if (bChanged)
		{
            // check if there're any message buttons with duplicate keys
            // delete duplicate message buttons
            var existingMsgKeys = new HashSet<string>();
			for (int i = 0; i < m_MsgBtns.Count;)
			{
				if (existingMsgKeys.Contains(m_MsgBtns[i].Key))
				{
					if (m_CurActiveMsg == m_MsgBtns[i])
					{
						m_CurActiveMsg = m_MsgBtns[0];
					}
					Destroy(m_MsgBtns[i].gameObject);
                    m_MergedMsgBtns.Remove(m_MsgBtns[i]);
					m_MsgBtns.RemoveAt(i);
				}
				else
				{
					existingMsgKeys.Add(m_MsgBtns[i].Key);
					++i;
				}
			}
		}
	}

	public MessageTagBtn AddMessageTagBtn(string key, FunctionNode node = null)
	{
		GameObject btnGo = Instantiate(m_MessageBtnTemplate, m_ViewParent) as GameObject;
		btnGo.SetActive(true);

		MessageTagBtn msgBtn = btnGo.GetComponent<MessageTagBtn>();
		msgBtn.Key = key;

		m_MsgBtns.Add(msgBtn);

		if (null != node)
		{
			m_CurActiveMsg = msgBtn;
			m_HeadNode = node;
		}

		return msgBtn;
	}

	public void OnClickTagBtn(MessageTagBtn tagBtn)
	{
        if (tagBtn == null)
        {
            throw new ArgumentNullException();
        }

        if (tagBtn.IsOpen)
        {
            return;
        }

		if (m_CurActiveMsg)
		{
			m_MessagePanel.SetMessageOpened(m_CurActiveMsg.Key, false);
			m_CurActiveMsg.IsOpen = false;
		}

        m_CurActiveMsg = tagBtn;
        Refresh();
	}

    private void Refresh()
    {
        m_CurActiveMsg.IsOpen = m_bOpen;
        m_MessagePanel.SetMessageOpened(m_CurActiveMsg.Key, m_CurActiveMsg.IsOpen);
		RefreshFlag();
        RefreshMsgListView();
    }

	public void ClearMerge()
	{
		for (int otherIndex = 0; otherIndex < m_MergedMsgBtns.Count; ++otherIndex)
		{
			var mergedMsg = m_MergedMsgBtns[otherIndex];
			if (m_CurActiveMsg == mergedMsg)
			{
				m_CurActiveMsg = m_MsgBtns[0];
			}
            Destroy(mergedMsg.gameObject);
            m_MsgBtns.Remove(mergedMsg);
		}
		m_MergedMsgBtns.Clear();
	}

	public void MergeWith(MessageTag target)
	{
		for (int i = 0; i < target.m_MsgBtns.Count; ++i)
		{
			MessageTagBtn tNewOther = AddMessageTagBtn(target.m_MsgBtns[i].Key);
			m_MergedMsgBtns.Add(tNewOther);
		}

        Refresh();
	}

	public void UpdateTagPos()
	{
        var trans = (RectTransform)transform;
        var newPos = trans.position;
        newPos.y = m_HeadNode.RectTransform.localToWorldMatrix.MultiplyPoint3x4(m_HeadNode.RectTransform.rect.center).y;
        trans.position = newPos;
	}

	public void SetActive(bool show)
	{
		for (int i = 0; i < m_ShowList.Length; ++i)
		{
			m_ShowList[i].SetActive(show);
		}
	}

    public bool HasSelfMessages
    {
        get { return m_MsgBtns.Count > m_MergedMsgBtns.Count; }
    }

    public int HeadNodeIndex
    {
        get { return m_HeadNode.NodeIndex; }
    }
}
