using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TeacherManager : ManagerBase
{
	public UITeacherMainView m_View;
	public UITeacherClassMain m_ClassMain;
	public UIViewUser m_UserInfoDialog;

	// Use this for initialization
	void Start()
	{
	//	StackUIBase.Push(m_View);
		if (null != UserManager.Instance.CurClass)
		{
		//	StackUIBase.Push(m_ClassMain);
            if (null != UserManager.Instance.CurTask)
            {
          //      m_ClassMain.SetActiveTab((int)TeacherClassTab.Grade);
            }
		}
	}

	public void Return()
	{
		SceneDirector.Pop();
	}

	public void ShowUserInfo(uint ID)
	{
		m_UserInfoDialog.ShowUserDetailInfo(ID);
	}
}
