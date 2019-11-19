using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Google.Protobuf;
using System;

using g_WebRequestManager = Singleton<WebRequestManager>;

public class UITeacherSelectClassIcon : MonoBehaviour
{
	public TeacherManager m_Manager;
	public Text m_Title;
	public UIIconNode[] m_Icon;
	
	Action<int> m_CallBack;
	UIIconNode m_CurSelect;

	void Start()
	{
		m_Title.text = "select_avatar".Localize();
		for (int i = 0; i < m_Icon.Length; ++i)
		{
			m_Icon[i].Icon.sprite = ClassIconResource.GetIcon(i + 1);
		}
	}

	public void OnSelectAvatar(UIIconNode node)
	{
		if(null != m_CurSelect)
		{
			m_CurSelect.SelectState(false);
        }
		m_CurSelect = node;
		m_CurSelect.SelectState(true);
		if (null != m_CallBack)
		{
			m_CallBack(m_CurSelect.ID);
		}
		SetActive(false);
	}

	public void SetActive(bool show, Action<int> callback = null)
	{
		gameObject.SetActive(show);
		if(null != m_CurSelect)
		{
			m_CurSelect.SelectState(false);
			m_CurSelect = null;
        }
		m_CallBack = callback;
	}

	public void Close()
	{
		SetActive(false);
	}
}
