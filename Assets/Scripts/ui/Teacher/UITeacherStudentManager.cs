using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Google.Protobuf;

public enum StuManagerOperation {
    NONE,
    DELETE,
    SEARCH
}

public class UITeacherStudentManager : MonoBehaviour
{
    public ScrollLoopController scrollStudentInfos;
    public GameObject btnCancle;
    public Button[] disableBtns;
    public Toggle[] disableToggles;
    public ButtonColorEffect btnBack;
    public GameObject searchPanel;
    public GameObject btnDel;

    private List<StudentInfoCellData> studentInfos = new List<StudentInfoCellData>();


    public void OnClickOperation(int type) {
        bool noneMode = (StuManagerOperation)type == StuManagerOperation.NONE;
        btnCancle.SetActive(!noneMode);

        foreach (Button btn in disableBtns)
        {
            btn.interactable = noneMode;
        }
        foreach (Toggle tog in disableToggles)
        {
            tog.interactable = noneMode;
        }
        btnBack.interactable = noneMode;
        if(!noneMode) {
            disableBtns[type].interactable = true;
        } else {
            searchPanel.SetActive(false);
        }

        if((StuManagerOperation)type == StuManagerOperation.SEARCH) {
            return;
        }
        foreach (StudentInfoCellData cell in studentInfos)
        {
            cell.operationType = (StuManagerOperation)type;
        }
        foreach(ScrollCell cell in scrollStudentInfos.GetCellsInUse()) {
            cell.gameObject.GetComponent<StudentInfoCell>().UpdateOpertion();
        }
    }


    public void AgreeJoin(ulong mailID) {
        CMD_Read_mail_r_Parameters tAgree = new CMD_Read_mail_r_Parameters();
        tAgree.MailId = mailID;

        int popupId = PopupManager.ShowMask();
        SocketManager.instance.send(Command_ID.CmdReadMailR, tAgree.ToByteString(), (res, content)=> {
            PopupManager.Close(popupId);
            if(res == Command_Result.CmdNoError) {
                CMD_Read_Mail_a_Parameters tRt = CMD_Read_Mail_a_Parameters.Parser.ParseFrom(content);
                ClassRequestInfo tCurRequest = UserManager.Instance.GetClassRequestInfo(tRt.MailId);
                //这里可以不刷新，留到NewStudentJoin里面一起刷新
                NewStudentJoin(tCurRequest.m_UserID);
                UserManager.Instance.DeleteClassRequest(tRt.MailId);
            } else {
                PopupManager.Notice(res.Localize());
            }
        });
    }

    public void NewStudentJoin(uint id) {
        CMD_Get_Userinfo_r_Parameters tNewStudentInfo = new CMD_Get_Userinfo_r_Parameters();
        tNewStudentInfo.ReqId = id;
        int popupId = PopupManager.ShowMask();
        SocketManager.instance.send(Command_ID.CmdGetUserinfoR, tNewStudentInfo.ToByteString(), (res, content) => {
            PopupManager.Close(popupId);
            if(res == Command_Result.CmdNoError) {
                CMD_Get_Userinfo_a_Parameters tRt = CMD_Get_Userinfo_a_Parameters.Parser.ParseFrom(content);
                ClassInfo tCurClass = UserManager.Instance.CurClass;
                for(int i = 0; i < tRt.UserList.Count; ++i) {
                    tCurClass.AddMemebr(tRt.UserList[i]);
                }
                RefreshList();
                OnClickOperation(0);
            } else {
                PopupManager.Notice(res.Localize());
            }
        });
    }

    public void OnActivate(bool show)
    {
        if (show)
        {
            GetClassRequests();
        }
    }

    public void RefreshList()
    {
        studentInfos.Clear();

        List<ClassRequestInfo> tCurClassRequest = UserManager.Instance.GetClassRequestList(UserManager.Instance.CurClass.m_ID);
        if(tCurClassRequest != null) {
            for(int i = 0; i < tCurClassRequest.Count; ++i) {
                studentInfos.Add(new StudentInfoCellData { classRequestInfo = tCurClassRequest[i], memberInfo = null });
            }
        }


        ClassInfo tCurClass = UserManager.Instance.CurClass;
        if(tCurClass != null && tCurClass.studentsInfos != null) {
            for(int i = 0; i < tCurClass.studentsInfos.Count; ++i) {
                studentInfos.Add(new StudentInfoCellData { classRequestInfo = null, memberInfo = tCurClass.studentsInfos[i] });
            }
        }

        scrollStudentInfos.initWithData(studentInfos);
        btnDel.SetActive(studentInfos.Count != 0);
    }

    void GetClassRequests()
    {
        CMD_Get_Maillist_r_Parameters tMailRequest = new CMD_Get_Maillist_r_Parameters();
        tMailRequest.RequestType = (uint)Mail_Type.MailClassReq;
        int popId = PopupManager.ShowMask();
        SocketManager.instance.send(Command_ID.CmdGetMaillistR, tMailRequest.ToByteString(), (res, content) => {
            PopupManager.Close(popId);
            if(res == Command_Result.CmdNoError) {
                UserManager.Instance.ClearClassRequests();
                CMD_Get_Maillist_a_Parameters tRt = CMD_Get_Maillist_a_Parameters.Parser.ParseFrom(content);
                for(int i = 0; i < tRt.MailList.Count; ++i) {
                    UserManager.Instance.AddClassRequestInfo(tRt.MailList[i].ToClassRequestInfo());
                }
                RefreshList();
            } else {
                PopupManager.Notice(res.Localize());
            }
        });
    }

    public void RefuseJoin(ulong mailID) {
        CMD_Del_Mail_r_Parameters tRefuse = new CMD_Del_Mail_r_Parameters();
        tRefuse.MailIds.Add(mailID);
        int popId = PopupManager.ShowMask();
        SocketManager.instance.send(Command_ID.CmdDelMailR, tRefuse.ToByteString(), (res, content) => {
            PopupManager.Close(popId);
            if(res == Command_Result.CmdNoError) {
                CMD_Del_Mail_a_Parameters tRt = CMD_Del_Mail_a_Parameters.Parser.ParseFrom(content);
                for(int i = 0; i < tRt.MailIds.Count; ++i) {
                    UserManager.Instance.DeleteClassRequest(tRt.MailIds[i]);
                }
                RefreshList();
                OnClickOperation(0);
            } else {
                PopupManager.Notice(res.Localize());
            }
        });
    }

    public void KickStudent(uint id)
	{
		ClassInfo tCurClass = UserManager.Instance.CurClass;
		MemberInfo tMember = tCurClass.GetMember(id);
		if(null != tMember)
		{
            PopupManager.YesNo("kick_prompt".Localize(tMember.nickName), ()=> {
                ConfirmToKick(id);
            });
		}
		else
		{
            PopupManager.Notice("kick_error".Localize());
		}
	}

	public void ConfirmToKick(object id)
	{
		CMD_Del_Class_Member_r_Parameters tKick = new CMD_Del_Class_Member_r_Parameters();
		tKick.ReqDelId = (uint)id;
		tKick.ClassId = UserManager.Instance.CurClass.m_ID;
        int popupId = PopupManager.ShowMask();
        SocketManager.instance.send(Command_ID.CmdDelClassMemberR, tKick.ToByteString(), (res, content) => {
            PopupManager.Close(popupId);
            if(res == Command_Result.CmdNoError) {
                CMD_Del_Class_Member_a_Parameters tRt = CMD_Del_Class_Member_a_Parameters.Parser.ParseFrom(content);
                ClassInfo tCurClass = UserManager.Instance.GetClass(tRt.ClassId);
                tCurClass.DeleteMember(tRt.DeledId);
                RefreshList();
                OnClickOperation(0);
            } else {
                PopupManager.Notice(res.Localize());
            }
        });
    }

	public void ShowDetailInfo(uint id)
	{
		//m_Manager.ShowUserInfo(id);
	}

}
