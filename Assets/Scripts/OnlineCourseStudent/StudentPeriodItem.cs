using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StudentPeriodItem : ScrollCell {
    public Text textName;
    public Sprite[] bgType;
    public Image bg;
    public GameObject passGo;
    public GameObject btnCellGo;
    public Image[] stars;
    public Sprite passStar;
    public Sprite failStar;


    private St_Period_Info stPeriodInfo;
    public override void configureCellData() {
        stPeriodInfo = (St_Period_Info)DataObject;
        textName.text = stPeriodInfo.periodInfo.PeriodName;

        int periodLevel = (Context as StudentPeriodUI).CurPeriodLevel;

        if(DataIndex < periodLevel) {
            FinfishPeriodState();
        } else if(DataIndex == periodLevel) {
            IngPeriodState();
        } else {
            UnLockPeriodState();
        }

        int gbScore = stPeriodInfo.GetAllGbScore();
        int getStar = 1;
        if(gbScore == 0) {
            getStar = 0;
        } else if(gbScore >= stPeriodInfo.periodInfo.PeriodFinsishCon.ThreestarScore) { // 三星
            getStar = 3;
        } else if(gbScore >= stPeriodInfo.periodInfo.PeriodFinsishCon.DoublestarScore) {
            getStar = 2;
        }


        for(int i = 0; i < stars.Length; i++) {
            if(i < getStar) {
                stars[i].sprite = passStar;
            } else {
                stars[i].sprite = failStar;
            }
        }

    }

    public uint GetPeriodId() {
        return stPeriodInfo.periodInfo.PeriodId;
    }

    void FinfishPeriodState() {
        btnCellGo.SetActive(true);
        passGo.SetActive(true);
        bg.sprite = bgType[0];
    }

    void IngPeriodState() {
        btnCellGo.SetActive(true);
        passGo.SetActive(false);
        bg.sprite = bgType[2];

       // float progress = (float)stPeriodInfo.userPeriodList.UserAttendPeriodList.Count / stPeriodInfo.periodInfo.PeriodItems.Count;
    }

    void UnLockPeriodState() {
        btnCellGo.SetActive(false);
        passGo.SetActive(false);
        bg.sprite = bgType[1];
    }

    public void OnClickCell() {
        (Context as StudentPeriodUI).ClickCell(stPeriodInfo);
    }
}
