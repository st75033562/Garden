using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public class UITipsBase : MonoBehaviour
{
	public Text m_Notice;

	public class DataNode
	{
		public string m_Notice;
		public Action<object> m_CallBack;
		public object m_Param;
	}
	Queue<DataNode> m_Data = new Queue<DataNode>();

	public void ShowTips(string content, Action<object> callback = null, object param = null)
	{
		if (0 == m_Data.Count)
		{
			m_Notice.text = content;
			gameObject.SetActive(true);
		}
		DataNode tNewNode = new DataNode();
		tNewNode.m_Notice = content;
		tNewNode.m_CallBack = callback;
		tNewNode.m_Param = param;
        m_Data.Enqueue(tNewNode);
	}

	public void CloseTips()
	{
		DataNode tCur = m_Data.Dequeue();
		if(null != tCur.m_CallBack)
		{
			tCur.m_CallBack(tCur.m_Param);
        }
		if (0  == m_Data.Count)
		{
			gameObject.SetActive(false);
		}
		else
		{
			m_Notice.text = m_Data.Peek().m_Notice;
		}
	}
}
