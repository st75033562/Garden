using DataAccess;
using Google.Protobuf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using g_WebRequestManager = Singleton<WebRequestManager>;

public class UILogin : MonoBehaviour
{
	public LobbyManager m_Manager;
	public Text m_Title;
	public Text m_AccountText;
	public Text m_PassWordText;
    public InputField inputName;
    public InputField inputPassword;
	public Toggle m_Remmber;
	public Text m_RemmberDec;
	public Text m_LoginBtnText;
	public Text m_RegistBtnText;

	const string c_Save_Remmber_key = "Remmber";
	const string c_Save_Account_key = "LoginAccount";
	const string c_Save_Password_key = "LoginPassword";

	int m_RemmberFlag;

	void Start () {
		m_RemmberFlag = 0;
		m_RemmberFlag = PlayerPrefs.GetInt(c_Save_Remmber_key);
        if (0 != m_RemmberFlag)
		{
			m_Remmber.isOn = true;
			string tSaveAccount = PlayerPrefs.GetString(c_Save_Account_key);
			string tSavePassword = PlayerPrefs.GetString(c_Save_Password_key);
			inputName.text = tSaveAccount;
			inputPassword.text = tSavePassword;
		}
		else
		{
			m_Remmber.isOn = false;
		}
	}

    public void OnClickLogin()
    {
        OnLogin();
    }

    public void OnClickRegister()
    {
        m_Manager.ShowRegist();
        SetActive(false);
    }

    public void OnClickForgotPassword()
    {
        m_Manager.ShowForgotPassword();
        SetActive(false);
    }

    void OnLogin()
	{
        if (inputName.text.Trim() == string.Empty)
        {
            m_Manager.ShowMaskTips("empty_account".Localize());
            return;    
        }

        if (inputPassword.text.Trim() == string.Empty)
        {
            m_Manager.ShowMaskTips("empty_password".Localize());
            return;
        }

        if (!SocketManager.instance.connected)
        {
            m_Manager.ShowMask();
            SocketManager.instance.connect((success) =>
            {
                m_Manager.CloseMask();
                if (!success)
                {
                    m_Manager.ShowMaskTips("error_connection".Localize());
                }
                else
                {
                    DoLogin();
                }
            });
        }
        else
        {
            DoLogin();
        }
    }

    void DoLogin()
    {
        m_Manager.ShowMask();
        
        SocketManager.instance.username = inputName.text.Trim();
        SocketManager.instance.password = inputPassword.text.Trim();
        SocketManager.instance.login(LoginCallBack, OnLoginTimeout);
    }

	public void SetActive(bool show)
	{
		if(show)
		{
			if(!string.IsNullOrEmpty(UserManager.Instance.AccountName))
			{
				inputName.text = UserManager.Instance.AccountName;
			}
			if (!string.IsNullOrEmpty(UserManager.Instance.Password))
			{
				inputPassword.text = UserManager.Instance.Password;
			}
			gameObject.SetActive(true);
		}
		else
		{
			gameObject.SetActive(false);
		}
	}

	public void RemmberCheckBoxChanged()
	{
		if(m_Remmber.isOn)
		{
			m_RemmberFlag = 1;
		}
		else
		{
			m_RemmberFlag = 0;
		}
	}

    private void OnLoginTimeout()
    {
        m_Manager.CloseMask();
    }

	private void LoginCallBack(Command_Result res, CMD_Account_Login_a_Parameters login_a)
	{
		m_Manager.CloseMask();
		if (res == Command_Result.CmdNoError && res != Command_Result.CmdAccountExpired)
		{
			UserManager.Instance.AccountName = inputName.text;
			UserManager.Instance.Password = inputPassword.text;
			UserManager.Instance.AccountId = login_a.AccountId;
			UserManager.Instance.Token = login_a.AccountToken;
			UserManager.Instance.AvatarID = (int)login_a.UserInfo.UserInconId;
			UserManager.Instance.Nickname = login_a.UserInfo.UserNickname;
			UserManager.Instance.UserId = login_a.UserInfo.UserId;
            if(login_a.UserInfo.UserPurchaseInfo != null) {
                UserManager.Instance.Coin = (int)login_a.UserInfo.UserPurchaseInfo.Coins;
            } else {
                UserManager.Instance.Coin = 0;
            }
            
            if (login_a.UserInfo != null)
            {
                UserManager.Instance.UserCourseInfo = login_a.UserInfo.UserCourseInfo;
            }
            ServerTime.Init(TimeUtils.FromEpochSeconds((long)login_a.SrvTime));

            UserManager.Instance.Authority = (User_Type)login_a.UserInfo.UserType;
            if (login_a.UserInfo.UserExpiredTime != ulong.MaxValue)
            {
                UserManager.Instance.AccountExpireTimeUTC = TimeUtils.FromEpochSeconds((long)login_a.UserInfo.UserExpiredTime);
            }
            else
            {
                UserManager.Instance.AccountExpireTimeUTC = null;
            }
            if(login_a.UserInfo.UserExtInfo != null) {
                UserManager.Instance.PhoneNum = login_a.UserInfo.UserExtInfo.CellphoneNum;
                UserManager.Instance.mailAddr = login_a.UserInfo.UserExtInfo.MailAddr;
            }

            if(null != login_a.UserInfo.UserClassInfo) {
                foreach(var item in login_a.UserInfo.UserClassInfo.UserAttendClassMapNew) {
                    ClassInfo tCurClass = UserManager.Instance.GetClass(item.Key);
                    if(null == tCurClass) {
                        tCurClass = new ClassInfo();
                        tCurClass.m_ID = item.Key;
                        UserManager.Instance.ClassList.Add(tCurClass);
                    }
                    tCurClass.m_ClassStatus = ClassInfo.Status.Attend_Status;
                }
                foreach(var item in login_a.UserInfo.UserClassInfo.UserCreateClassMapNew) {
                    ClassInfo tCurClass = UserManager.Instance.GetClass(item.Key);
                    if(null == tCurClass) {
                        tCurClass = new ClassInfo();
                        tCurClass.m_ID = item.Key;
                        UserManager.Instance.ClassList.Add(tCurClass);
                    }
                    tCurClass.m_ClassStatus = ClassInfo.Status.Create_Status;
                }
                foreach(var item in login_a.UserInfo.UserClassInfo.UserAppliedClassesNew) {
                    ClassInfo tCurClass = UserManager.Instance.GetClass(item.Key);
                    if(null == tCurClass) {
                        tCurClass = new ClassInfo();
                        tCurClass.m_ID = item.Key;
                        UserManager.Instance.ClassList.Add(tCurClass);
                    }
                    tCurClass.m_ClassStatus = ClassInfo.Status.Applied_Status;
                }
            }
            UserManager.Instance.ClassSort();

            var arObjs = ARObjectDataSource.freeObjects.Select(x => x.id);
            if (login_a.UserInfo.UserBuyArObjs != null)
            {
                arObjs = arObjs.Concat(login_a.UserInfo.UserBuyArObjs.BuyArObjs.Keys.Select(x => (int)x));
            }
            UserManager.Instance.arObjects.Initialize(arObjs);

            LoadHonorWall();

            UserManager.Instance.CreateTopics = new Dictionary<uint, User_Topic_Unit_Info>();
            UserManager.Instance.AttendTopics = new Dictionary<uint, User_Topic_Unit_Info>();
            if(login_a.UserInfo.UserTopicInfo != null) {
                foreach (uint key in login_a.UserInfo.UserTopicInfo.UserCreateTopicMap.Keys) {
                    UserManager.Instance.CreateTopics.Add(key, login_a.UserInfo.UserTopicInfo.UserCreateTopicMap[key]);
                }
                foreach(uint key in login_a.UserInfo.UserTopicInfo.UserAttendTopicMap.Keys) {
                    UserManager.Instance.AttendTopics.Add(key, login_a.UserInfo.UserTopicInfo.UserAttendTopicMap[key]);
                }
  
            }
        }
		else
		{
			m_Manager.RequestErrorCode(res);
		}
	}

    void LoadHonorWall() {
        int maskId = PopupManager.ShowMask();
        CMD_Get_Honorwall_r_Parameters getHonourWall = new CMD_Get_Honorwall_r_Parameters();
        SocketManager.instance.send(Command_ID.CmdGetHonorwallR, getHonourWall.ToByteString(), (res, content) => {
            PopupManager.Close(maskId);
            if(res == Command_Result.CmdNoError) {
                HonorWallData honorWallData = HonorWallData.instance;
                CMD_Get_Honorwall_a_Parameters getHonourWallA = CMD_Get_Honorwall_a_Parameters.Parser.ParseFrom(content);
                Honor_Wall_Info honorWallInfo = getHonourWallA.HonorwallInfo;
                foreach(uint key in honorWallInfo.UserCertificates.Keys) {
                    honorWallData.AddCertificate(UserCertificate.Parse(key, honorWallInfo.UserCertificates[key]));
                }
                foreach(uint key in honorWallInfo.UserTrophies.Keys) {
                    honorWallData.AddTrophy(UserTrophy.Parse(key, honorWallInfo.UserTrophies[key]));
                }

                m_Manager.LoginSuccess();
                if(0 != m_RemmberFlag) {
                    PlayerPrefs.SetString(c_Save_Account_key, UserManager.Instance.AccountName);
                    PlayerPrefs.SetString(c_Save_Password_key, UserManager.Instance.Password);
                }
                PlayerPrefs.SetInt(c_Save_Remmber_key, m_RemmberFlag);
                PlayerPrefs.Save();

                SetActive(false);
            } else {
                m_Manager.RequestErrorCode(res);
            }
        });
    }
}
