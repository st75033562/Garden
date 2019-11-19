using UnityEngine;
using UnityEngine.UI;
using Google.Protobuf;
using System.Text.RegularExpressions;
using System.Collections;

public class UIRegister : MonoBehaviour
{
	public LobbyManager m_Manager;

    public Button sendCodeButton;
    public UISendCodeButton sendCodeButtonController;
    public GameObject accountInput;
	public InputField inputAccount;

    public GameObject emailInput;
	public InputField inputEmail;

    public InputField inputPassword;
	public InputField m_NickNameInput;
	public Image m_AvatarSelectBtn;
	public Sprite m_AvatarDefaultSelectImage;

    public InputField inputVerificationCode;
    public UIActivationCodeInput inputInvitationCode;

    public const int VerificationCodeLength = 6;

	int m_AvatartID;

	// Use this for initialization
	void Start () {
#if REGION_CHINA
        accountInput.SetActive(true);
        emailInput.SetActive(false);
        inputVerificationCode.transform.parent.gameObject.SetActive(true);
#else
        accountInput.SetActive(false);
        emailInput.SetActive(true);
        inputVerificationCode.transform.parent.gameObject.SetActive(false);
#endif
    }
	
    void OnEnable()
    {
        inputAccount.text = string.Empty;
        inputEmail.text = string.Empty;
        inputPassword.text = string.Empty;
        m_NickNameInput.text = string.Empty;
        inputVerificationCode.text = string.Empty;
        inputInvitationCode.code = string.Empty;
    }

    public void OnRegister()
    {
#if REGION_CHINA
        string account = inputAccount.text;
        if (!AccountUtils.IsPhoneNumber(account))
        {
            m_Manager.ShowMaskTips("ui_phone_number_invalid".Localize());
            return;
        }
#else
        string account = inputEmail.text;
        if (!AccountUtils.IsEmail(account))
        {
            m_Manager.ShowMaskTips("ui_email_invalid".Localize());
            return;
        }
#endif

        if (string.IsNullOrEmpty(inputPassword.text))
        {
            m_Manager.ShowMaskTips("empty_password".Localize());
            return;
        }

        if (string.IsNullOrEmpty(m_NickNameInput.text.Trim()))
        {
            m_Manager.ShowMaskTips("empty_nickname".Localize());
            return;
        }

        if(FileUtils.fileNameContainsInvalidChars(m_NickNameInput.text.Trim())) {
            m_Manager.ShowMaskTips("file_name_invalid_char".Localize());
            return;
        }

        if (!inputInvitationCode.valid)
        {
            m_Manager.ShowMaskTips("ui_invalid_invitation_code_length".Localize(UIActivationCodeInput.CodeLength));
            return;
        }

        string verificationCode = inputVerificationCode.text;
        if (verificationCode.Length != VerificationCodeLength)
        {
            m_Manager.ShowMaskTips("ui_verification_code_length_invalid".Localize(VerificationCodeLength));
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
                    DoRegister(account, verificationCode, inputInvitationCode.code);
                }
            });
        }
        else
        {
            DoRegister(account, verificationCode, inputInvitationCode.code);
        }
    }

    void DoRegister(string account, string verificationCode, string invitationCode)
    {
        CMD_Reg_r_Parameters register_r = new CMD_Reg_r_Parameters();
        register_r.AccountName = account;
        register_r.AccountPass = inputPassword.text;
        register_r.UserNickname = m_NickNameInput.text.Trim();
        register_r.UserIconId = (uint)m_AvatartID;
        register_r.CellphoneNum = account;
        register_r.VerificationCode = verificationCode;
        register_r.InviteCode = invitationCode;

        SocketManager.instance.send(Command_ID.CmdRegR, register_r.ToByteString(), RegisterCallBack);

        m_Manager.ShowMask();
    }


    void RegisterCallBack(Command_Result res, ByteString content)
	{
		m_Manager.CloseMask();
        if (res == Command_Result.CmdNoError)
		{
			UserManager.Instance.AccountName = inputAccount.text;
			UserManager.Instance.Password = inputPassword.text;
			gameObject.SetActive(false);
            m_Manager.Logout();
			m_Manager.ShowLogin();
		}
		else
		{
			m_Manager.RequestErrorCode(res);
		}
	}

	public void SetActive(bool show)
	{
		gameObject.SetActive(show);

		m_AvatartID = 1;
		m_AvatarSelectBtn.sprite = m_AvatarDefaultSelectImage;
        sendCodeButtonController.Reset();
	}

	public void SwitchToLogin()
	{
		SetActive(false);
		m_Manager.ShowLogin();
    }

	public void ShowAvatarSelect()
	{
		m_Manager.ShowAvatar(UIAvatarWorkMode.Regist_Enum, AvatarSelectCallBack);
    }

	public void AvatarSelectCallBack(int id)
	{
		m_AvatarSelectBtn.sprite = UserIconResource.GetUserIcon(id);
		m_AvatartID = id;
    }

    public void OnClickSendCode()
    {
        if (!AccountUtils.IsPhoneNumber(inputAccount.text))
        {
            m_Manager.ShowMaskTips("ui_phone_number_invalid".Localize());
            return;
        }

        if (!inputInvitationCode.valid)
        {
            m_Manager.ShowMaskTips("ui_invalid_invitation_code_length".Localize(UIActivationCodeInput.CodeLength));
            return;
        }

        var request = Users.GetVerificationCode(inputAccount.text, inputInvitationCode.code, true);
        request.defaultErrorHandling = false;
        request.Success((code, response) => {
            if (code == Command_Result.CmdNoError)
            {
                Debug.Log("request vcode success: " + response.VerificationCode);
                if (!string.IsNullOrEmpty(response.VerificationCode))
                {
                    inputVerificationCode.text = response.VerificationCode;
                }
                sendCodeButtonController.StartCooldown();
            }
            else
            {
                m_Manager.ShowMaskTips(code.Localize());
            }
        })
        .Error(() => {
            m_Manager.ShowMaskTips("network_error".Localize());
        })
        .Execute();
    }

    public void OnClickPrivacyService() {
        Application.OpenURL("http://www.pocketurtle.com/agreements/service.html");
    }

    public void OnClickPrivacy() {
        Application.OpenURL("http://www.pocketurtle.com/agreements/privacy.html");
    }
}
