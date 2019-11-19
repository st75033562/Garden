using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections;

public class DragEventToScrollViewOrScrollBar : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
	public ScrollRect m_Scroll;
	public Scrollbar m_ScrollBar;
	public Action m_Callback;
	public bool m_X_To_Scroll;
	public float m_Deviation = 20.0f;

	Vector2 m_DownPos;

	public void OnDrag(PointerEventData eventData)
	{
		Vector2 tDir = eventData.position - m_DownPos;
		if(Vector2.Distance(eventData.position, m_DownPos) < m_Deviation)
		{
			return;
		}
		if (m_ScrollBar && m_Scroll)
		{
			if (Math.Abs(tDir.x) < Math.Abs(tDir.y))
			{
				DispatchYDrag(eventData);
			}
			else
			{
				DispatchXDrag(eventData);
			}
		}
		else if(m_Scroll)
		{
			EventToRect(eventData);
		}
		else if(m_ScrollBar)
		{
			EventToBar(eventData);
		}
		if (null != m_Callback)
		{
			m_Callback();
		}
	}

	public void OnPointerUp(PointerEventData eventData)
	{
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		m_DownPos = eventData.position;
    }

	void DispatchYDrag(PointerEventData eventData)
	{
		if(m_X_To_Scroll)
		{
			EventToBar(eventData);
        }
		else
		{
			EventToRect(eventData);
        }
	}

	void DispatchXDrag(PointerEventData eventData)
	{
		if(m_X_To_Scroll)
		{
			EventToRect(eventData);
		}
		else
		{
			EventToBar(eventData);
		}
	}

	void EventToRect(PointerEventData eventData)
	{
		eventData.pointerEnter = m_Scroll.gameObject;
		eventData.pointerPress = m_Scroll.gameObject;
		eventData.rawPointerPress = m_Scroll.gameObject;
		eventData.pointerDrag = m_Scroll.gameObject;
		m_Scroll.OnBeginDrag(eventData);
	}

	void EventToBar(PointerEventData eventData)
	{
		eventData.pointerEnter = m_ScrollBar.gameObject;
		eventData.pointerPress = m_ScrollBar.gameObject;
		eventData.rawPointerPress = m_ScrollBar.gameObject;
		eventData.pointerDrag = m_ScrollBar.gameObject;
		m_ScrollBar.OnBeginDrag(eventData);
	}
}
