using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System;
using System.Collections.Generic;

public class MessageElementBase : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    public EventBus m_EventManager;
    public MessageTag m_MessageTag;
	public Transform m_ParenPanle;
	public GameObject m_DeleteArea;
	public ScrollRect m_View;
    public Image m_AvatarImage;
    public Text m_UserNameText;

	private const float s_kPickUpTime = 0.15f;

	Vector2 m_DownPos;
	bool m_PickUp = false;
	int m_PointID = int.MinValue;
	Vector3 m_PosOffSet;
	RectTransform m_Rect;
    LeaveMessage m_Message;

	void Awake()
	{
		m_Rect = GetComponent<RectTransform>();
        Deletable = true;
    }

    public bool Deletable
    {
        get;
        set;
    }

	public void OnDrag(PointerEventData eventData)
	{
		if (m_PointID != eventData.pointerId)
		{
			return;
		}

		if(m_PickUp)
		{
			m_Rect.position = new Vector3(eventData.position.x, eventData.position.y) + m_PosOffSet;
		}
		else
		{
			eventData.pointerEnter = m_View.gameObject;
			eventData.pointerPress = m_View.gameObject;
			eventData.rawPointerPress = m_View.gameObject;
			eventData.pointerDrag = m_View.gameObject;
			m_View.OnBeginDrag(eventData);
            ReleasePick();
        }
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		if (int.MinValue != m_PointID)
		{
			return;
		}

		m_PointID = eventData.pointerId;
		m_DownPos = eventData.position;
		m_PosOffSet = m_Rect.position - new Vector3(m_DownPos.x, m_DownPos.y);
		if (Deletable)
		{
			StartCoroutine(StarPickUp());
		}
    }

	public void OnPointerUp(PointerEventData eventData)
	{
		if (m_PointID != eventData.pointerId)
		{
			return;
		}
		m_PointID = int.MinValue;
		if(!m_PickUp)
		{
			ReleasePick();
            return;
		}
		if(m_DeleteArea)
		{
            if (RectTransformUtility.RectangleContainsScreenPoint(
                m_DeleteArea.GetComponent<RectTransform>(), eventData.position))
            {
                var cmd = new DeleteLeaveMessageCommand(m_MessageTag.m_MessagePanel, MessageKey, MessageIndex);
                m_MessageTag.m_MessagePanel.m_Workspace.UndoManager.AddUndo(cmd);
            }
            else
            {
                m_EventManager.AddEvent(EventId.UpdateLeaveMessage, MessageKey);
            }

			m_DeleteArea.SetActive(false);
		}
		Destroy(gameObject);
	}

	void ReleasePick()
	{
		StopAllCoroutines();
	}

	IEnumerator StarPickUp()
	{
		yield return new WaitForSeconds(s_kPickUpTime);
		m_PickUp = true;
		m_Rect.SetParent(m_ParenPanle);
		m_DeleteArea.SetActive(true);
		yield return 0;
    }

    public int MessageIndex
    {
        get;
        set;
    }

    public string MessageKey
    {
        get;
        set;
    }

    public virtual LeaveMessage Message
    {
        get { return m_Message; }
        set
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            m_Message = value;
            AvatarService.GetAvatarId(m_Message.m_UserID, OnGetAvatarInfo);
            m_UserNameText.text = m_Message.m_NickName;
        }
    }

    private void OnGetAvatarInfo(UserAvatarInfo avatarInfo)
    {
        m_AvatarImage.sprite = UserIconResource.GetUserIcon(avatarInfo.avatarId);
    }

    public IAvatarService AvatarService
    {
        get;
        set;
    }
}
