using Google.Protobuf;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopupModifyPassWord : PopupController {
    public InputField inputOldPassWord;
    public InputField inputNewPassWord_1;
    public InputField inputNewPassWord_2;
    public Button btnConfirm;
    public Text textHint;

    private bool oldValidatePass;
    private bool newValidataPass;

    protected override void Start() {
        base.Start();
        textHint.gameObject.SetActive(false);
    }

    public void EndOldPassWord() {
        if(inputOldPassWord.text != UserManager.Instance.Password) {
            textHint.gameObject.SetActive(true);
            textHint.text = "ui_old_password_hint".Localize();
            oldValidatePass = false;
        } else {
            textHint.gameObject.SetActive(false);
            oldValidatePass = true;
            SwitchBtnState();
        }
    }

    public void EndNewPassWord() {
        if(string.IsNullOrEmpty(inputNewPassWord_1.text) || string.IsNullOrEmpty(inputNewPassWord_2.text)) {
            newValidataPass = false;
            SwitchBtnState();
            return;
        }
        if(inputNewPassWord_1.text != inputNewPassWord_2.text) {
            textHint.gameObject.SetActive(true);
            textHint.text = "ui_new_password_hint".Localize();
            newValidataPass = false;
            SwitchBtnState();
        } else {
            textHint.gameObject.SetActive(false);
            newValidataPass = true;
            SwitchBtnState();
        }
    }

    void SwitchBtnState() {
        btnConfirm.interactable = oldValidatePass && newValidataPass;
    }

    public void OnClickSendPassWord() {
        int maskId = PopupManager.ShowMask();
        CMD_Modify_Password_r_Parameters modifyPassword = new CMD_Modify_Password_r_Parameters();
        modifyPassword.NewPassword = inputNewPassWord_2.text;
        modifyPassword.OldPassword = UserManager.Instance.Password;
        SocketManager.instance.send(Command_ID.CmdModifyPasswordR, modifyPassword.ToByteString(), (result, content) => {
            PopupManager.Close(maskId);
            if(result == Command_Result.CmdNoError) {
                UserManager.Instance.Password = inputNewPassWord_2.text;
                Close();
            } else {
                PopupManager.Notice(result.Localize());
            }
        });
    }
}
