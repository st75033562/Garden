using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PublishPeriodItem : PopupController {
    public class PayLoad {
        public Period_Info periodInfo;
        public uint CourseId;
    }
    public ScrollLoopController scroll;
    public Text periodNama;
    public Text periodDescription;
    public Text unlockNum;
    public Text submitCount;
    public PeriodAnswer periodAnser;
    public Text textThreeStar;
    public Text textTwoStar;
    public Text textOneStar;

    private List<GBAnswer> treeStarAnswer = new List<GBAnswer>();
    private List<GBAnswer> twoStarAnswer = new List<GBAnswer>();
    private List<GBAnswer> oneStarAnswer = new List<GBAnswer>();

    public Period_Info periodInfo { get; set; }
    private Period_Item_Info gbPeriodItem;
    private uint courseId;
    protected override void Start() {
        base.Start();
        var data = (PayLoad)payload;
        courseId = data.CourseId;
       SetData(data.periodInfo);
    }
    public void SetData(Period_Info periodInfo) {
        this.periodInfo = periodInfo;
        periodNama.text = periodInfo.PeriodName;
        periodDescription.text = periodInfo.PeriodDescription;

        treeStarAnswer.Clear();
        twoStarAnswer.Clear();
        oneStarAnswer.Clear();

        unlockNum.text = periodInfo.UnlockedUserCount.ToString();

        List<Period_Item_Info> periodItems = new List<Period_Item_Info>();

        foreach(uint i in periodInfo.PeriodItemDisplayList) {
            if((Period_Item_Type)periodInfo.PeriodItems[i].ItemType == Period_Item_Type.ItemGb) {
                gbPeriodItem = periodInfo.PeriodItems[i];
                foreach(GBAnswer gbAnswer in periodInfo.PeriodItems[i].GbInfo.GbAnswerList.Values) {
                    if(gbAnswer.GbScore >= periodInfo.PeriodFinsishCon.ThreestarScore) {
                        treeStarAnswer.Add(gbAnswer);
                    } else if(gbAnswer.GbScore >= periodInfo.PeriodFinsishCon.DoublestarScore) {
                        twoStarAnswer.Add(gbAnswer);
                    } else {
                        oneStarAnswer.Add(gbAnswer);
                    }
                }
                submitCount.text = periodInfo.PeriodItems[i].GbInfo.GbAnswerList.Count.ToString();

            }
            periodItems.Add(periodInfo.PeriodItems[i]);
        }

        textThreeStar.text = treeStarAnswer.Count.ToString();
        textTwoStar.text = twoStarAnswer.Count.ToString();
        textOneStar.text = oneStarAnswer.Count.ToString();

        scroll.initWithData(periodItems);
    }

    public void OnClickTreeStar() {
        if(periodInfo.PeriodType == (uint)PopupILPeriod.PassModeType.Submit) {
            periodAnser.gameObject.SetActive(true);
            periodAnser.SetData(3, gbPeriodItem, treeStarAnswer);
        }
    }

    public void OnClickTwoStar() {
        if(periodInfo.PeriodType == (uint)PopupILPeriod.PassModeType.Submit) {
            periodAnser.gameObject.SetActive(true);
            periodAnser.SetData(2, gbPeriodItem, twoStarAnswer);
        }
    }

    public void OnClickOneStar() {
        if(periodInfo.PeriodType == (uint)PopupILPeriod.PassModeType.Submit) {
            periodAnser.gameObject.SetActive(true);
            periodAnser.SetData(1, gbPeriodItem, oneStarAnswer);
        }
    }
    public void OnClickClose() {
        gameObject.SetActive(false);
    }

    public void OnClickRank() {
        var data = new PopupPeriodRank.PayLoad();
        data.courseId = courseId;
        data.periodId = periodInfo.PeriodId;
        data.startScore = new uint[3];
        data.startScore[0] = periodInfo.PeriodFinsishCon.PassScore;
        data.startScore[1] = periodInfo.PeriodFinsishCon.DoublestarScore;
        data.startScore[2] = periodInfo.PeriodFinsishCon.ThreestarScore;
        PopupManager.PeirodRank(data);
    }
}
