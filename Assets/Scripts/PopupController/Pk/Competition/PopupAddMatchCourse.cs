using Google.Protobuf;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopupAddMatchCourse : PopupController
{

    public ScrollLoopController scroll;
    public InputField inputCourseName;
    public Text textCoin;
    public CompetitionListModel currentModel;
    protected override void Start()
    {
        base.Start();
        currentModel = (CompetitionListModel)payload;
        scroll.context = this;
        scroll.initWithData(new List<int>());
    }
    protected override void OnDisable()
    {
        base.OnDisable();
        UserManager.Instance.onCoinChange -= CoinValueChange;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        UserManager.Instance.onCoinChange += CoinValueChange;
    }

    void CoinValueChange(int coins)
    {
        textCoin.text = coins.ToString();
    }

    public void OnClickBack()
    {
        gameObject.SetActive(false);
    }

    public void OnClickSearch()
    {
        if (string.IsNullOrEmpty(inputCourseName.text))
            return;
        int maskId = PopupManager.ShowMask();
        CMD_Get_Course_List_r_Parameters courseListR = new CMD_Get_Course_List_r_Parameters();
        courseListR.ReqType = GetCourseListType.GetCourseTest;
        courseListR.ReqName = inputCourseName.text;
        courseListR.ReqCourseType = Course_type.Race;

        SocketManager.instance.send(Command_ID.CmdGetCourseListR, courseListR.ToByteString(), (res, content) => {
            PopupManager.Close(maskId);
            if (res == Command_Result.CmdNoError)
            {
                CMD_Get_Course_List_a_Parameters courseA = CMD_Get_Course_List_a_Parameters.Parser.ParseFrom(content);
                scroll.initWithData(courseA.CouseList, false);
            }
            else
            {
                Debug.LogError("CmdCreateCourseR:" + res);
            }
        });
    }

}
