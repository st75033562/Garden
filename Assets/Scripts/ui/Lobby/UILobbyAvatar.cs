using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Google.Protobuf;
using System;

using g_WebRequestManager = Singleton<WebRequestManager>;

public enum UIAvatarWorkMode
{
	Regist_Enum,
	Change_Enum,
}

public class UILobbyAvatar : MonoBehaviour
{
	public LobbyManager m_Manager;
	public Text m_Title;
	public UIIconNode[] m_Icon;

	UIAvatarWorkMode m_Mode;
	Action<int> m_CallBack;
	UIIconNode m_CurSelect;

	void Awake()
	{
		m_Mode = UIAvatarWorkMode.Regist_Enum;
	}
	// Use this for initialization
	void Start()
	{
	}

	public void OnSelectAvatar(UIIconNode node)
	{
		if(null != m_CurSelect)
		{
			m_CurSelect.SelectState(false);
		}
		m_CurSelect = node;
		m_CurSelect.SelectState(true);
        switch (m_Mode)
		{
			case UIAvatarWorkMode.Regist_Enum:
				{
					if (null != m_CallBack)
					{
						m_CallBack(node.ID);
					}
					SetActive(false);
				}
				break;
			case UIAvatarWorkMode.Change_Enum:
				{
					CMD_Update_Userinfo_r_Parameters tChangeAvatar = new CMD_Update_Userinfo_r_Parameters();
					tChangeAvatar.UpdateUserinfo = new A8_User_Info();
					tChangeAvatar.UpdateUserinfo.UserInconId = (uint)node.ID;

					SocketManager.instance.send(Command_ID.CmdUpdateUserinfoR, tChangeAvatar.ToByteString(), ChangeAvatarCallBack);
				}
				break;
		}
	}

	public void SetActive(bool show, UIAvatarWorkMode mode = UIAvatarWorkMode.Change_Enum, Action<int> callback = null)
	{
		gameObject.SetActive(show);
		if(m_CurSelect)
		{
			m_CurSelect.SelectState(false);
			m_CurSelect = null;
		}
		m_Mode = mode;
		m_CallBack = callback;
		for (int i = 0; i < m_Icon.Length; ++i)
		{
			m_Icon[i].Icon.sprite = UserIconResource.GetUserIcon(i + 1);
		}
	}

	public void Close()
	{
		SetActive(false);
	}

	void ChangeAvatarCallBack(Command_Result res, ByteString content)
	{
		if (res == Command_Result.CmdNoError)
		{
			CMD_Update_Userinfo_a_Parameters tRt = CMD_Update_Userinfo_a_Parameters.Parser.ParseFrom(content);
			UserManager.Instance.AvatarID = (int)tRt.UpdatedUserinfo.UserInconId;
			EventBus.Default.AddEvent(EventId.UpdateAvatar);
		}
		else
		{
			m_Manager.RequestErrorCode(res);
		}
	}
}
