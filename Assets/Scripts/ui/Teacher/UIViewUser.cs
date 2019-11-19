using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Google.Protobuf;
using System.Collections.Generic;


public class UIViewUser : MonoBehaviour
{
	public Image m_Avatar;
	public Text m_NickName;
	public Text m_Class;
	public TeacherManager m_Manager;

    // Use this for initialization
    void Start()
    {
    }

    public void SetActive(bool show)
	{
		gameObject.SetActive(show);
	}

	public void ShowUserDetailInfo(uint ID)
	{
		CMD_Get_Userinfo_r_Parameters tNewStudentInfo = new CMD_Get_Userinfo_r_Parameters();
		tNewStudentInfo.ReqId = ID;

        SocketManager.instance.send(Command_ID.CmdGetUserinfoR, tNewStudentInfo.ToByteString(), SearchCallBack);

        m_Manager.ShowMask();
	}

	private void SearchCallBack(Command_Result res, ByteString content)
	{
        if (res == Command_Result.CmdNoError)
		{
			CMD_Get_Userinfo_a_Parameters tRt = CMD_Get_Userinfo_a_Parameters.Parser.ParseFrom(content);
			A8_User_Info tCurInfo = tRt.UserList[0];
			SetBaseInfo(tCurInfo);

			List<uint> tAllClass = new List<uint>();

			if (null != tCurInfo.UserClassInfo)
			{
				foreach (var item in tCurInfo.UserClassInfo.UserCreateClassMapNew)
				{
					tAllClass.Add(item.Key);
				}
				foreach (var item in tCurInfo.UserClassInfo.UserAttendClassMapNew)
				{
					tAllClass.Add(item.Key);
                }
			}

			if (tAllClass.Count > 3)
			{
				tAllClass.RemoveRange(3, tAllClass.Count - 3);
			}

			if (0 != tAllClass.Count)
			{
				CMD_Get_Classinfo_r_Parameters tClassRequest = new CMD_Get_Classinfo_r_Parameters();
				for (int i = 0; i < tAllClass.Count; ++i)
				{
					tClassRequest.ReqClassList.Add(tAllClass[i]);
				}
                SocketManager.instance.send(Command_ID.CmdGetClassinfoR, tClassRequest.ToByteString(), ClassCallBack);
            }
			else
			{
				SetClassInfo(null);
			}
		}
		else
		{
            m_Manager.CloseMask();
			m_Manager.RequestErrorCode(res);
		}
	}

	public void SetBaseInfo(A8_User_Info info)
	{
        m_Avatar.sprite = UserIconResource.GetUserIcon((int)info.UserInconId);
		m_NickName.text = info.UserNickname;
	}

	private void ClassCallBack(Command_Result res, ByteString content)
	{
        if (res == Command_Result.CmdNoError)
		{
			CMD_Get_Classinfo_a_Parameters tRt = CMD_Get_Classinfo_a_Parameters.Parser.ParseFrom(content);
			SetClassInfo(tRt);
		}
		else
		{
			m_Manager.RequestErrorCode(res);
		}
	}

	public void SetClassInfo(CMD_Get_Classinfo_a_Parameters classInfo)
	{
		m_Manager.CloseMask();

		m_Class.text = "";
		if (null != classInfo)
		{
			for (int i = 0; i < classInfo.ClassInfoList.Count; ++i)
			{
				A8_Class_Info tCurNetData = classInfo.ClassInfoList[i];
				if (0 != i)
				{
					m_Class.text += "\n";
				}
				m_Class.text += tCurNetData.ClassName;
			}
		}
		SetActive(true);
	}

	public void ClickClose()
	{
		SetActive(false);
	}
}
