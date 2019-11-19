using Google.Protobuf;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopupModifyAccount : PopupController {
    public enum PanelType {
        Mian,
        Modify,
        Add,
        Notice
    }
    enum OPerationType {
        Phone,
        Mail
    }
    public GameObject[] panels;

    public GameObject phoneModifyPanel;
    public GameObject phoneAddPanel;
    public GameObject mailModifyPanel;
    public GameObject mailAddPanel;
    public Text mainPhoneText;
    public Text mainMailText;
    public Text oldNum;
    public InputField oldPhoneVC;
    public Text modifyHint;
    public InputField newNum;
    public InputField newVC;
    public Text addHint;
    public ModifyAccountNotice modifyNotice;
    public Text textTitle;
    public Text oldAccount;
    public Button oldVerificatebtn;
    public Text newAccount;
    public Text newAccountNotice;
    public UISendCodeButton bindSendCode;

    private OPerationType operationType;
    protected override void Start() {
        base.Start();
        ShowPanel(PanelType.Mian);
    }

    void ShowPanel(PanelType type) {
        foreach (GameObject go in panels)
        {
            go.SetActive(false);
        }
        panels[(int)type].SetActive(true);
        if(type == PanelType.Mian) {
            textTitle.text = "ui_modify_account_bind".Localize();
            if(!string.IsNullOrEmpty(UserManager.Instance.PhoneNum)) {
                phoneModifyPanel.SetActive(true);
                string subStr = UserManager.Instance.PhoneNum.Substring(3, 4);
                mainPhoneText.text = UserManager.Instance.PhoneNum.Replace(subStr, "****");
            } else {
                phoneModifyPanel.SetActive(false);
            }
            phoneAddPanel.SetActive(!phoneModifyPanel.activeSelf);

            if(!string.IsNullOrEmpty(UserManager.Instance.mailAddr)) {
                mailModifyPanel.SetActive(true);
                mainMailText.text = UserManager.Instance.mailAddr;
            } else {
                mailModifyPanel.SetActive(false);
            }
            mailAddPanel.SetActive(!mailModifyPanel.activeSelf);
        } else if(type == PanelType.Add) {
            if(operationType == OPerationType.Mail) {
                textTitle.text = "ui_bind_mail".Localize();
                newAccount.text = "ui_ma_new_mail".Localize();
                newAccountNotice.text = "ui_ma_intput_mail".Localize();
            } else {
                textTitle.text = "ui_bind_phone".Localize();
                newAccount.text = "ui_ma_new_phone".Localize();
                newAccountNotice.text = "ui_ma_intput_phone".Localize();
            }
            newNum.text = "";
            newVC.text = "";
            addHint.gameObject.SetActive(false);
        } else if(type == PanelType.Modify) {
            modifyHint.gameObject.SetActive(false);
            oldVerificatebtn.interactable = false;
            oldPhoneVC.text = "";
            if(operationType == OPerationType.Mail) {
                textTitle.text = "ui_ma_modify_mailbox".Localize();
                oldAccount.text = "ui_ma_old_mailbox".Localize();
            } else {
                textTitle.text = "ui_ma_modify_phone".Localize();
                oldAccount.text = "ui_ma_old_phone".Localize();
            }
        } else if(type == PanelType.Notice) {
            textTitle.text = "ui_dialog_notice".Localize();
        }
    }

    public void OnClickAddPhone() {
        operationType = OPerationType.Phone;
        ShowPanel(PanelType.Add);
    }

    public void OnClickAddMail() {
        operationType = OPerationType.Mail;
        ShowPanel(PanelType.Add);
    }

    public void OnClickModifyPhone() {
        ShowPanel(PanelType.Notice);
        modifyNotice.SetData("ui_ma_phone_notice".Localize(), () => {
            operationType = OPerationType.Phone;
            ShowPanel(PanelType.Modify);
            oldNum.text = UserManager.Instance.PhoneNum;
        });
    }

    public void OnClickModifyMail() {
        ShowPanel(PanelType.Notice);
        modifyNotice.SetData("ui_ma_mail_notice".Localize(), () => {
            operationType = OPerationType.Mail;
            ShowPanel(PanelType.Modify);
            oldNum.text = UserManager.Instance.mailAddr;
        });
    }

    public override void OnCloseButton()
    {
        if(panels[(int)PanelType.Mian].activeSelf) {
            base.OnCloseButton();
        } else {
            ShowPanel(PanelType.Mian);
        }
    }

    public void OnClickGetUnBindCode() {
        int maskId = PopupManager.ShowMask();
        CMD_Get_Verification_Code_r_Parameters getVerification = new CMD_Get_Verification_Code_r_Parameters();
        getVerification.CellphoneNum = oldNum.text;
        getVerification.VcType = 3; //账号验证码
        SocketManager.instance.send(Command_ID.CmdGetVerificationCodeR, getVerification.ToByteString(), (result, content) => {
            PopupManager.Close(maskId);
            if(result != Command_Result.CmdNoError) {
                modifyHint.gameObject.SetActive(true);
                modifyHint.text = result.Localize();
            }
        });
    }

    public void InputOldVerificate() {
        oldVerificatebtn.interactable = !string.IsNullOrEmpty(oldPhoneVC.text);
    }

    public void OnClickUnBindVC() {
        int maskId = PopupManager.ShowMask();
        CMD_Unbind_r_Parameters unbind = new CMD_Unbind_r_Parameters();
        if(operationType == OPerationType.Phone) {
            unbind.PhoneNum = oldNum.text;
        } else {
            unbind.MailAddr = oldNum.text;
        }

        unbind.VerifyCode = oldPhoneVC.text;
        SocketManager.instance.send(Command_ID.CmdUnbindR, unbind.ToByteString(), (result, content) => {
            PopupManager.Close(maskId);
            if(result == Command_Result.CmdNoError) {
                if(operationType == OPerationType.Phone) {
                    UserManager.Instance.PhoneNum = "";
                } else {
                    UserManager.Instance.mailAddr = "";
                }
                ShowPanel(PanelType.Add);
            } else {
                modifyHint.gameObject.SetActive(true);
                modifyHint.text = result.Localize();
            }
        });
    }

    public void OnClickGetBindCode() {
        if (!ValidateNewAccount())
        {
            return;
        }

        bindSendCode.StartCooldown();
        int maskId = PopupManager.ShowMask();
        CMD_Get_Verification_Code_r_Parameters getVerification = new CMD_Get_Verification_Code_r_Parameters();
        getVerification.CellphoneNum = newNum.text;
        getVerification.VcType = 2; //绑定账号验证码
        SocketManager.instance.send(Command_ID.CmdGetVerificationCodeR, getVerification.ToByteString(), (result, content) => {
            PopupManager.Close(maskId);
            if(result != Command_Result.CmdNoError) {
                addHint.gameObject.SetActive(true);
                addHint.text = result.Localize();
            }
        });
    }

    bool ValidateNewAccount()
    {
        if (operationType == OPerationType.Phone && !AccountUtils.IsPhoneNumber(newNum.text))
        {
            PopupManager.Notice("ui_phone_number_invalid".Localize());
            return false;
        }

        if (operationType == OPerationType.Mail && !AccountUtils.IsEmail(newNum.text))
        {
            PopupManager.Notice("ui_email_invalid".Localize());
            return false;
        }

        return true;
    }

    public void OnClickSaveBind() {
        if (!ValidateNewAccount())
        {
            return;
        }

        int maskId = PopupManager.ShowMask();
        CMD_Rbind_r_Parameters bind = new CMD_Rbind_r_Parameters();
        if(operationType == OPerationType.Phone) {
            bind.PhoneNum = newNum.text;
        } else {
            bind.MailAddr = newNum.text;
        }
        
        bind.VerifyCode = newVC.text; 
        SocketManager.instance.send(Command_ID.CmdRbindR, bind.ToByteString(), (result, content) => {
            PopupManager.Close(maskId);
            if(result == Command_Result.CmdNoError) {
                if(operationType == OPerationType.Phone) {
                    UserManager.Instance.PhoneNum = newNum.text;
                } else {
                    UserManager.Instance.mailAddr = newNum.text;
                }

                OnCloseButton();
            } else {
                addHint.gameObject.SetActive(true);
                addHint.text = result.Localize();
            }
        });
    }
}
