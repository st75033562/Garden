using System;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class UIChangePassword : MonoBehaviour
{
    public enum Mode
    {
        ForgotPassword,
        ChangePassword
    }

    public InputField inputPhoneNumber;
    public InputField inputNewPassword;

    public InputField inputCode;
    public Button sendButton;
    public UISendCodeButton sendButtonController;

    public LobbyManager lobbyManager;

    private Mode m_mode = Mode.ChangePassword;

    void Awake()
    {
#if !REGION_CHINA
        inputCode.transform.parent.gameObject.SetActive(false);
#endif
    }

    void OnEnable()
    {
        inputNewPassword.text = string.Empty;
        inputCode.text = string.Empty;
        inputPhoneNumber.text = string.Empty;
    }

    public void Show(Mode mode)
    {
        m_mode = mode;

        inputPhoneNumber.transform.parent.gameObject.SetActive(mode == Mode.ForgotPassword);
        gameObject.SetActive(true);
        sendButtonController.Reset();
    }

    public string PhoneNumber
    {
        set { inputPhoneNumber.text = value; }

        get { return inputPhoneNumber.text; }
    }


    public void OnClickChange()
    {
        if (m_mode == Mode.ForgotPassword)
        {
            if (!AccountUtils.IsPhoneNumber(inputPhoneNumber.text))
            {
                lobbyManager.ShowMaskTips("ui_phone_number_invalid".Localize());
                return;
            }
        }

        if (inputNewPassword.text.Length == 0)
        {
            lobbyManager.ShowMaskTips("ui_change_password_new_pwd_error".Localize());
            return;
        }

        if (inputCode.text.Length != UIRegister.VerificationCodeLength)
        {
            lobbyManager.ShowMaskTips("ui_verification_code_length_invalid".Localize(UIRegister.VerificationCodeLength));
            return;
        }

#if REGION_CHINA
        lobbyManager.ShowMask();

        var request = Users.ResetPassword(inputNewPassword.text, PhoneNumber, inputCode.text);
        request.defaultErrorHandling = false;
        request.Success(OnChangePassword)
        .Error(() => {
            lobbyManager.ShowMaskTips("network_error".Localize());
        })
        .Execute();
#else
        throw new NotImplementedException();
#endif
    }

    void OnChangePassword(Command_Result res, CMD_Reset_Password_a_Parameters data)
    {
        lobbyManager.CloseMask();

        if (res == Command_Result.CmdNoError)
        {
            lobbyManager.ShowMaskTips("ui_change_password_succeeds".Localize(), delegate {
                lobbyManager.Logout();
                lobbyManager.ShowLogin();
                gameObject.SetActive(false);
            });
        }
        else
        {
            lobbyManager.ShowMaskTips(res.Localize());
        }
    }

    public void OnClickGotoLogin()
    {
        gameObject.SetActive(false);
        lobbyManager.ShowLogin();
    }

    public void OnClickSendCode()
    {
        if (!AccountUtils.IsPhoneNumber(PhoneNumber))
        {
            lobbyManager.ShowMaskTips("ui_phone_number_invalid".Localize());
            return;
        }

        var request = Users.GetVerificationCode(PhoneNumber, null, false);
        request.defaultErrorHandling = false;
        request.Success((code, response) => {
            if (code == Command_Result.CmdNoError)
            {
                Debug.Log("request vcode success: " + response.VerificationCode);
                if (!string.IsNullOrEmpty(response.VerificationCode))
                {
                    inputCode.text = response.VerificationCode;
                }
                sendButtonController.StartCooldown();
            }
            else
            {
                lobbyManager.ShowMaskTips(code.Localize());
            }
        })
        .Error(() => {
            lobbyManager.ShowMaskTips("network_error".Localize());
        })
        .Execute();
    }
}
