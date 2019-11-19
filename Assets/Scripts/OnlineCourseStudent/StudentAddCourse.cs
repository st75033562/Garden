using Google.Protobuf;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StudentAddCourse : PopupController {
    public class Payload {
        public OnlineCourseStudentController.ShowType showType;
        public Action<Course_Info> addCallback;
    }
    public ScrollLoopController scroll;
    public InputField inputCourseName;
    public Text textCoin;

    public OnlineCourseStudentController.ShowType showType;
    private Action<Course_Info> addCallback;
    protected override void Start() {
        base.Start();
        var data = (Payload)payload;
        showType = data.showType;
        addCallback = data.addCallback;
        ShowAll();
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

    public void ShowAll() {
        textCoin.text = UserManager.Instance.Coin.ToString();
        if(showType == OnlineCourseStudentController.ShowType.Formal) {
            int maskId = PopupManager.ShowMask();
            CMD_Get_Course_List_r_Parameters courseListR = new CMD_Get_Course_List_r_Parameters();
            courseListR.ReqType = GetCourseListType.GetCoursePublishedNew;
            if(Preference.scriptLanguage == ScriptLanguage.Visual) {
                courseListR.ReqProjectLanguageType = Project_Language_Type.ProjectLanguageGraphy;
            } else {
                courseListR.ReqProjectLanguageType = Project_Language_Type.ProjectLanguagePython;
            }

            SocketManager.instance.send(Command_ID.CmdGetCourseListR, courseListR.ToByteString(), (res, content) => {
                PopupManager.Close(maskId);
                if(res == Command_Result.CmdNoError) {
                    CMD_Get_Course_List_a_Parameters courseA = CMD_Get_Course_List_a_Parameters.Parser.ParseFrom(content);
                    scroll.context = this;
                    scroll.initWithData(courseA.CouseList);
                } else {
                    Debug.LogError("CmdCreateCourseR:" + res);
                }
            });
        } else {
            scroll.context = this;
            scroll.initWithData(new List<int>());
        }
    }

    public void OnClickBack() {
        gameObject.SetActive(false);
    }

    public void AddMyCourse(Course_Info course) {
        addCallback(course);
    }

    public void OnClickSearch() {
        if(string.IsNullOrEmpty(inputCourseName.text))
            return;
        int maskId = PopupManager.ShowMask();
        CMD_Get_Course_List_r_Parameters courseListR = new CMD_Get_Course_List_r_Parameters();
        if(showType == OnlineCourseStudentController.ShowType.Formal) {
            courseListR.ReqType = GetCourseListType.GetCoursePublishedNew;
        } else if(showType == OnlineCourseStudentController.ShowType.Test) {
            courseListR.ReqType = GetCourseListType.GetCourseTest;
        }
        else if (showType == OnlineCourseStudentController.ShowType.Test)
        {
            courseListR.ReqType = GetCourseListType.GetCourseTest;
        }
        courseListR.ReqName = inputCourseName.text;
        if(Preference.scriptLanguage == ScriptLanguage.Visual) {
            courseListR.ReqProjectLanguageType = Project_Language_Type.ProjectLanguageGraphy;
        } else {
            courseListR.ReqProjectLanguageType = Project_Language_Type.ProjectLanguagePython;
        }
        SocketManager.instance.send(Command_ID.CmdGetCourseListR, courseListR.ToByteString(), (res, content) => {
            PopupManager.Close(maskId);
            if(res == Command_Result.CmdNoError) {
                CMD_Get_Course_List_a_Parameters courseA = CMD_Get_Course_List_a_Parameters.Parser.ParseFrom(content);
                scroll.initWithData(courseA.CouseList, false);
            } else {
                Debug.LogError("CmdCreateCourseR:" + res);
            }
        });
    }
}
