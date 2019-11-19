using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Google.Protobuf;
using System.Collections.Generic;

public class UITeacherSearch : MonoBehaviour {
    public ScrollLoopController scroll;
    public InputField m_SearchInput;
    public UITeacherStudentManager studentManager;

    private readonly List<A8_User_Info> m_SearchResults = new List<A8_User_Info>();

	public void ClickSearch()
	{
        gameObject.SetActive(true);
		CMD_Get_Userinfo_r_Parameters tNewStudentInfo = new CMD_Get_Userinfo_r_Parameters();
		tNewStudentInfo.ReqNickname = m_SearchInput.text;

        int popupId = PopupManager.ShowMask();
        SocketManager.instance.send(Command_ID.CmdGetUserinfoR, tNewStudentInfo.ToByteString(), (res, content) => {
            PopupManager.Close(popupId);
            if(res == Command_Result.CmdNoError) {
                CMD_Get_Userinfo_a_Parameters tRt = CMD_Get_Userinfo_a_Parameters.Parser.ParseFrom(content);
                m_SearchResults.Clear();
                m_SearchResults.AddRange(tRt.UserList);
                scroll.initWithData(m_SearchResults);
            } else {
                PopupManager.Notice(res.Localize());
            }
        });

    }

	public void SetActive(bool show)
	{
        gameObject.SetActive(show);
        m_SearchResults.Clear();
        scroll.initWithData(m_SearchResults);
    }

	public void InvateMember(uint userID)
	{
		ClassInfo tCurClass = UserManager.Instance.CurClass;
		if (null == tCurClass)
		{
			return;
		}
		for (int i = 0; i < tCurClass.studentsInfos.Count; ++i)
		{
			if (userID == tCurClass.studentsInfos[i].userId)
			{
				return;
			}
		}
        int popupId = PopupManager.ShowMask();
        CMD_Add_Class_Member_r_Parameters tInvite = new CMD_Add_Class_Member_r_Parameters();
		tInvite.AddId = userID;
		tInvite.ClassId = tCurClass.m_ID;

        SocketManager.instance.send(Command_ID.CmdAddClassMemberR, tInvite.ToByteString(), (res, content) => {
            PopupManager.Close(popupId);
            if(res == Command_Result.CmdNoError) {
                CMD_Add_Class_Member_a_Parameters tRt = CMD_Add_Class_Member_a_Parameters.Parser.ParseFrom(content);
                NewStudentInvite(tRt.AddedId);
            } else {
                PopupManager.Notice(res.Localize());
            }
        });
    }

	public void NewStudentInvite(uint id)
	{
        int popupId = PopupManager.ShowMask();
		CMD_Get_Userinfo_r_Parameters tNewStudentInfo = new CMD_Get_Userinfo_r_Parameters();
		tNewStudentInfo.ReqId = id;

        SocketManager.instance.send(Command_ID.CmdGetUserinfoR, tNewStudentInfo.ToByteString(), (res, content) => {
            PopupManager.Close(popupId);
            if(res == Command_Result.CmdNoError) {
                CMD_Get_Userinfo_a_Parameters tRt = CMD_Get_Userinfo_a_Parameters.Parser.ParseFrom(content);
                ClassInfo tCurClass = UserManager.Instance.CurClass;
                for(int i = 0; i < tRt.UserList.Count; ++i) {
                    tCurClass.AddMemebr(tRt.UserList[i]);
                }
                scroll.initWithData(m_SearchResults);
                studentManager.RefreshList();
            } else {
                PopupManager.Notice(res.Localize());
            }
        });
    }

	public void AskPlayInfo(uint ID)
	{
	//	m_Manager.ShowUserInfo(ID);
	}

    public ClassInfo CurClass
    {
        get { return UserManager.Instance.CurClass; }
    }
}
