using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UITeacherJoinRequest : MonoBehaviour {
	public Image m_AvatarIcon;
	public Text m_Nick;
	public UITeacherStudentManager m_Manager;

	ulong m_RequestID;
	uint m_UserID;
	
	public void SetActive(bool show)
	{
		gameObject.SetActive(show);
	}

	public void SetValue(ClassRequestInfo request)
	{
		m_RequestID = request.m_MailID;
		m_UserID = request.m_UserID;
        m_Nick.text = request.m_NickName;
		m_AvatarIcon.sprite = UserIconResource.GetUserIcon((int)request.m_IconID);
    }

	public ulong RequestID
	{
		get { return m_RequestID; }
	}

	public void ClickAgree()
	{
		m_Manager.AgreeJoin(m_RequestID);
    }

	public void ClickRefuse()
	{
		m_Manager.RefuseJoin(m_RequestID);
	}

	public void ClickNode()
	{
		m_Manager.ShowDetailInfo(m_UserID);
    }
}
