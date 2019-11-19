//#define TEST

using Google.Protobuf;
using UnityEngine;
using UnityEngine.UI;

public class UIActivation : MonoBehaviour
{
    public LobbyManager uiManager;
    public UIActivationCodeInput inputCode;
    public Button activateButton;
    public Text titleText;
    public Text hintText;

    private Type type;
    public enum Type {
        Account,
        Python,
        GameBoard
    }

    public void SetType(Type type) {
        this.type = type;
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
        }
    }
    void Start()
    {
        inputCode.onValidChanged.AddListener(OnCodeValidChanged);
    }

    void OnCodeValidChanged(bool valid)
    {
        activateButton.interactable = valid;
    }

    void OnEnable()
    {
        inputCode.code = string.Empty;
        activateButton.interactable = false;
    }

    void OnActivateAccount(Command_Result res, ByteString data)
    {
        uiManager.CloseMask();

        if (res == Command_Result.CmdNoError)
        {
            var activationA = CMD_Use_Activation_Code_a_Parameters.Parser.ParseFrom(data);
            if(type == Type.Account) {
                uiManager.ShowMaskTips("ui_account_activated".Localize(), delegate {
                    gameObject.SetActive(false);
                    uiManager.Logout();
                    uiManager.ShowLogin();
                });
            } else if(type == Type.Python || type == Type.GameBoard) {
                UserManager.Instance.Authority = (User_Type)activationA.UserType;
                uiManager.ShowMaskTips("ui_text_activate_sucess".Localize(), delegate {
                    gameObject.SetActive(false);
                });
            }

        }
        else
        {
            uiManager.ShowMaskTips(res.Localize());
        }
    }

    public void OnClickActivate()
    {
#if !TEST

        uiManager.ShowMask();

        CMD_Use_Activation_Code_r_Parameters param = new CMD_Use_Activation_Code_r_Parameters();
        param.ActivationCode = inputCode.code;
        SocketManager.instance.send(Command_ID.CmdUseActivationCodeR, param.ToByteString(), OnActivateAccount);
#else
        OnActivateAccount(null, Command_Result.CmdNoError);
#endif
    }
}
