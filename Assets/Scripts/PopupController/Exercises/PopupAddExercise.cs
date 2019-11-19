using Google.Protobuf;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopupAddExercise : PopupController {
    public enum Type {
        Publish,
        Test
    }

    public class Payload {
        public PopupAddExercise.Type type;
        public Action<Topic_Info> addCallback;
    }
    public ScrollLoopController scroll;
    public InputField inputCourseName;
    public Text textCoin;
    public GameObject coinGo;

    public Type type;
    private Action<Topic_Info> addCallback;
    protected override void Start() {
        base.Start();
        var data = (Payload)payload;
        type = data.type;
        addCallback = data.addCallback;
        textCoin.text = UserManager.Instance.Coin.ToString();
        if(type == Type.Publish) {
            ShowExercises(GetTopicListType.GetTopicPublished);
        } else {
            scroll.context = this;
            scroll.initWithData(new List<int>());
        }
        coinGo.SetActive(type != Type.Test);
    }

    protected override void OnDisable() {
        base.OnDisable();
        UserManager.Instance.onCoinChange -= CoinValueChange;
    }

    protected override void OnEnable() {
        base.OnEnable();
        UserManager.Instance.onCoinChange += CoinValueChange;
    }

    void CoinValueChange(int coins) {
        textCoin.text = coins.ToString();
    }

    public void ShowExercises(GetTopicListType type, string reqName = null) {
        int maskId = PopupManager.ShowMask();
        CMD_Get_Topic_List_r_Parameters exerciseR = new CMD_Get_Topic_List_r_Parameters();
        if(Preference.scriptLanguage == ScriptLanguage.Visual) {
            exerciseR.ReqProjectLanguageType = Project_Language_Type.ProjectLanguageGraphy;
        } else {
            exerciseR.ReqProjectLanguageType = Project_Language_Type.ProjectLanguagePython;
        }
        exerciseR.ReqType = type;
        if(reqName != null) {
            exerciseR.ReqName = reqName;
        }

        SocketManager.instance.send(Command_ID.CmdGetTopicListR, exerciseR.ToByteString(), (res, content) => {
            PopupManager.Close(maskId);
            if(res == Command_Result.CmdNoError) {
                var courseA = CMD_Get_Topic_List_a_Parameters.Parser.ParseFrom(content);
                scroll.context = this;
                scroll.initWithData(courseA.TopicList);
            } else {
                Debug.LogError("CmdCreateCourseR:" + res);
            }
        });
    }

    public void OnClickSearch() {
        if(string.IsNullOrEmpty(inputCourseName.text))
            return;

        if(type == Type.Publish) {
            ShowExercises(GetTopicListType.GetTopicPublished, inputCourseName.text);     
        } else if(type == Type.Test) {
            ShowExercises(GetTopicListType.GetTopicTest, inputCourseName.text);
        }
    }

    public void OnClickBuy(AddExerciseCell info) {
        if(type == Type.Publish) {
            if(info.topicInfo.TopicPrice > UserManager.Instance.Coin) {
                PopupManager.Notice("ui_cmd_result_17".Localize());
                return;
            }
            Buy(info);
        } else if(type == Type.Test) {
            PopupManager.SetPassword("ui_minicourse_view_password".Localize(), "", new SetPasswordData((str) => {
                Buy(info, str);
            }, null));
        }
    }

    void Buy(AddExerciseCell info, string password = null) {
        var buy = new CMD_Buy_Topic_r_Parameters();
        buy.TopicId = info.topicInfo.TopicId;
        if(password != null) {
            buy.TopicPassword = password;
        }

        int maskId = PopupManager.ShowMask();
        SocketManager.instance.send(Command_ID.CmdBuyTopicR, buy.ToByteString(), (res, content) => {
            PopupManager.Close(maskId);
            if(res == Command_Result.CmdNoError) {
                info.SetBuyState(false);
                addCallback(info.topicInfo);
            } else {
                PopupManager.Notice(res.Localize());
                Debug.LogError("CmdCreateCourseR:" + res);
            }
        });

    }
}
