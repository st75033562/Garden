using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Linq;

public class UITeacherSearchNode : ScrollCell
{
	public Image m_AvatarID;
	public Text m_Nick;
	public GameObject m_Add;
	public UITeacherSearch m_Leader;
    public Button btnCell;
	
    public override void configureCellData()
    {
        var userInfo = UserInfo;

		m_AvatarID.sprite = UserIconResource.GetUserIcon((int)userInfo.UserInconId);
		m_Nick.text = userInfo.UserNickname;

        bool isEnrolled = m_Leader.CurClass.studentsInfos.Any(x => x.userId == userInfo.UserId);
		m_Add.SetActive(!isEnrolled);
        btnCell.interactable = !isEnrolled;
    }

    public A8_User_Info UserInfo
    {
        get { return (A8_User_Info)DataObject; }
    }

	public void ClickAddBtn()
	{
		m_Leader.InvateMember(UserInfo.UserId);
	}

	public void ChangeInviteStatu()
	{
		m_Add.SetActive(false);
	}

	public void ClickNode()
	{
	//	m_Leader.AskPlayInfo(UserInfo.UserId);
    }
}
