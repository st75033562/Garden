using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StudentPeriodManager : PopupController {
    public Transform itemParentGo;
    public Text textDescription;
    public ScrollLoopController scroll;
    public Text textScore;
    public Text titleText;

    public St_Period_Info stPeriodInfo {
        set;
        get;
    }

    public int GbScore {
        set { textScore.text = value.ToString(); }
    }

    protected override void Start() {
        base.Start();
        SetData((St_Period_Info)payload);
    }

    public void SetData(St_Period_Info stPeriodInfo) {
        textDescription.text = stPeriodInfo.periodInfo.PeriodDescription;
        this.stPeriodInfo = stPeriodInfo;

        List<Period_Item_Info> lists = new List<Period_Item_Info>();
        foreach(uint i in stPeriodInfo.periodInfo.PeriodItemDisplayList) {
            lists.Add(stPeriodInfo.periodInfo.PeriodItems[i]);
        }
        scroll.initWithData(lists);
        titleText.text = stPeriodInfo.periodInfo.PeriodName;
    }
}
