using Google.Protobuf;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActiveConfigure {
    public PopupActivation.Type type;
    public Action<CMD_Use_Activation_Code_a_Parameters> activeSucess;
}
public class PopupActivation : PopupController {
    public UIActivationCodeInput inputCode;
    public Button activateButton;
    public Text titleText;
    public Text hintText;

    private Action<CMD_Use_Activation_Code_a_Parameters> activeSucess;
    private int popupId;
    public enum Type {
        Account,
        Python,
        GameBoard,
        AR
    }

    protected override void Start () {
        base.Start();
        var configure = (ActiveConfigure)payload;
        activeSucess = configure.activeSucess;
        inputCode.onValidChanged.AddListener(OnCodeValidChanged);
        SetType(configure.type);
    }

    void SetType(Type type) {
        switch(type) {
            case Type.Account:
                titleText.text = "ui_activation_title".Localize();
                hintText.text = "ui_activation_hint".Localize();
                break;
            case Type.Python:
                titleText.text = "ui_text_activate_py".Localize();
                hintText.text = "ui_text_activate_py_hint".Localize();
                break;
            case Type.GameBoard:
                titleText.text = "ui_text_activate_py".Localize();
                hintText.text = "ui_text_activate_gb_hint".Localize();
                break;
            case Type.AR:
                titleText.text = "ui_text_activate_py".Localize();
                hintText.text = "ui_text_activate_ar_hint".Localize();
                break;
        }
    }

    void OnCodeValidChanged(bool valid) {
        activateButton.interactable = valid;
    }

    protected override void OnEnable() {
        base.OnEnable();
        inputCode.code = string.Empty;
        activateButton.interactable = false;
    }

    void OnActivateAccount(Command_Result res, ByteString data) {
        PopupManager.Close(popupId);
        if(res == Command_Result.CmdNoError) {
            var activationA = CMD_Use_Activation_Code_a_Parameters.Parser.ParseFrom(data);
            activeSucess(activationA);
            Close();
        } else {
            PopupManager.Notice(res.Localize());
        }
    }

    public void OnClickActivate() {
#if !TEST
        popupId = PopupManager.ShowMask();
        CMD_Use_Activation_Code_r_Parameters param = new CMD_Use_Activation_Code_r_Parameters();
        param.ActivationCode = inputCode.code;
        SocketManager.instance.send(Command_ID.CmdUseActivationCodeR, param.ToByteString(), OnActivateAccount);
#else
        OnActivateAccount(null, Command_Result.CmdNoError);
#endif
    }
}
