using Google.Protobuf;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopupModifyName : PopupController {
    public InputField inputFieldName;
    public Button btnSave;

    private Action<string> callBack;
	// Use this for initialization
	protected override void Start () {
        base.Start();
        callBack = (Action<string>) payload;
    }

    public void InputFieldEditor() {
        btnSave.interactable = !string.IsNullOrEmpty(inputFieldName.text.Trim());
    }

    public void OnClickSave() {
        if(FileUtils.fileNameContainsInvalidChars(inputFieldName.text.Trim())) {
            PopupManager.Notice("file_name_invalid_char".Localize());
            return;
        }
        int maskId = PopupManager.ShowMask();
        CMD_Change_Nickname_r_Parameters changeNickName = new CMD_Change_Nickname_r_Parameters();
        changeNickName.NewNickname = inputFieldName.text.Trim();
        SocketManager.instance.send(Command_ID.CmdChangeNicknameR, changeNickName.ToByteString(), (result, content) => {
            PopupManager.Close(maskId);
            if(result == Command_Result.CmdNoError) {
                callBack(inputFieldName.text.Trim());
                Close();
            } else {
                PopupManager.Notice(result.Localize());
            }
        });
    }
}
