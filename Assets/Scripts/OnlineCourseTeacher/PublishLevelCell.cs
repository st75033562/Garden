using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PublishLevelCell : ScrollCell {
    public Text textName;
    public Text unLockNum;
    public Text finishRate;
    public Text averageScore;
    public GameObject[] stars;

    private Period_Info periodInfo;

    public override void configureCellData() {
        periodInfo = (Period_Info)DataObject;
        textName.text = periodInfo.PeriodName;
        unLockNum.text = periodInfo.UnlockedUserCount.ToString();
        if(periodInfo.PeriodCompletedUserCount == 0) {
            finishRate.text = "0%";
            averageScore.text = "0";
        } else {
            finishRate.text = Math.Round((float)periodInfo.PeriodCompletedUserCount / periodInfo.UnlockedUserCount * 100, 2).ToString() + "%";
            averageScore.text = (periodInfo.PeriodTotalScore / periodInfo.PeriodCompletedUserCount).ToString();
        }

        uint averageStar = 1;
        if(periodInfo.PeriodCompletedUserCount != 0 && periodInfo.PeriodTotalStar > 1) {
            averageStar = periodInfo.PeriodTotalStar / periodInfo.PeriodCompletedUserCount;
        }
        for(int i=0; i < stars.Length; i++) {
            if(i < averageStar)
                stars[i].SetActive(true);
            else
                stars[i].SetActive(false);
        }

    }

    public void OnClick() {
        (Context as PublishPeriod).ClickCell(periodInfo);
    }
}
