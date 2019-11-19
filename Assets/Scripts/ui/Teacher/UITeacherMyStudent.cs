using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UITeacherMyStudent : MonoBehaviour {
	public Image m_AvatarIcon;
	public Text m_StudentName;
	public UITeacherStudentManager m_Manager;

	uint m_StudentID;
	
	public void SetActive(bool show)
	{
		gameObject.SetActive(show);
	}

	public void SetValue(MemberInfo info)
	{
		m_StudentID = info.userId;
		m_StudentName.text = info.nickName;
		m_AvatarIcon.sprite = UserIconResource.GetUserIcon((int)info.iconId);
    }

	public void ClickKick()
	{
		m_Manager.KickStudent(m_StudentID);
    }

	public uint StudentID
	{
		get { return m_StudentID; }
	}

	public void ClickNode()
	{
		m_Manager.ShowDetailInfo(m_StudentID);
	}
}
