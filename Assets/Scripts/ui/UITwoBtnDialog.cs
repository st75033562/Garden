using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using System.Collections.Generic;

public class UITwoBtnDialog : MonoBehaviour
{
	public class DialogEvent
	{
		public string m_Content;
		public Action<object> m_SuccessCallBack;
		public object m_SuccessParam;
		public Action<object> m_FailCallBack;
		public object m_FailParam;
	}
	public Text m_Notice;
	Queue<DialogEvent> m_Data = new Queue<DialogEvent>();
	
	public void ShowDialog(string content, Action<object> success, object successparam, Action<object> fail = null, object failparam = null)
	{
		if (0 == m_Data.Count)
		{
			m_Notice.text = content;
            gameObject.SetActive(true);
		}
		DialogEvent tNewEvent = new DialogEvent();
		tNewEvent.m_Content = content;
		tNewEvent.m_SuccessCallBack = success;
		tNewEvent.m_SuccessParam = successparam;
		tNewEvent.m_FailCallBack = fail;
		tNewEvent.m_FailParam = failparam;
		m_Data.Enqueue(tNewEvent);

	}

	public void ClickCancel()
	{
		CloseDialog(false);
	}

	public void ClickConfirm()
	{
		CloseDialog(true);
    }

	void CloseDialog(bool confirm)
	{
		DialogEvent tCurEvent = m_Data.Dequeue();
		if(confirm && null != tCurEvent.m_SuccessCallBack)
		{
			tCurEvent.m_SuccessCallBack(tCurEvent.m_SuccessParam);
		}
		else if(!confirm && null != tCurEvent.m_FailCallBack)
		{
			tCurEvent.m_FailCallBack(tCurEvent.m_FailParam);
		}
		if (0 == m_Data.Count)
		{
			gameObject.SetActive(false);
		}
		else
		{
			tCurEvent = m_Data.Peek();
			m_Notice.text = tCurEvent.m_Content;
		}
	}
}
