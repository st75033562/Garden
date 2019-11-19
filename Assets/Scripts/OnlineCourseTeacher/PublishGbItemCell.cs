using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PublishGbItemCell : PublishItemCell {
    public Text unlockNum;
    public Text submitCount;
    public Text threeStarCount;
    public Text twoStarCount;
    public Text oneStarCount;
    public Slider threeStarSlider;
    public Slider twoStarSlider;
    public Slider oneStarSlider;
    public PeriodAnswer periodAnswer;


    private List<GBAnswer> treeStarAnswer = new List<GBAnswer>();
    private List<GBAnswer> twoStarAnswer = new List<GBAnswer>();
    private List<GBAnswer> oneStarAnswer = new List<GBAnswer>();

    private Period_Item_Info periodItem;
    // Use this for initialization
    void Start () {
		
	}
    public override void SetData(Period_Info periodInfo , Period_Item_Info periodItem) {
        this.periodItem = periodItem;
        base.SetData(periodInfo , periodItem);

        unlockNum.text = "ui_text_unlocked".Localize() + ":" + periodInfo.UnlockedUserCount.ToString();
        submitCount.text = "ui_text_Submissions".Localize() + ":" + periodItem.ItemCompletedUserCount.ToString();

        treeStarAnswer.Clear();
        twoStarAnswer.Clear();
        oneStarAnswer.Clear();

        foreach (GBAnswer gbAnswer in periodItem.GbInfo.GbAnswerList.Values)
        {
            if(gbAnswer.GbScore >= periodInfo.PeriodFinsishCon.ThreestarScore) {
                treeStarAnswer.Add(gbAnswer);
            } else if(gbAnswer.GbScore >= periodInfo.PeriodFinsishCon.DoublestarScore) {
                twoStarAnswer.Add(gbAnswer);
            } else {
                oneStarAnswer.Add(gbAnswer);
            }
        }

        threeStarCount.text = treeStarAnswer.Count.ToString();
        twoStarCount.text = twoStarAnswer.Count.ToString();
        oneStarCount.text = oneStarAnswer.Count.ToString();
        threeStarSlider.value = (float)treeStarAnswer.Count / periodItem.ItemCompletedUserCount;
        twoStarSlider.value = (float)twoStarAnswer.Count / periodItem.ItemCompletedUserCount;
        oneStarSlider.value = (float)oneStarAnswer.Count / periodItem.ItemCompletedUserCount;

    }

    public void OnClickStar3() {
        periodAnswer.SetData(3 , periodItem, treeStarAnswer);
    }

    public void OnClickStar2() {
        periodAnswer.SetData(2, periodItem, twoStarAnswer);
    }

    public void OnClickStar1() {
        periodAnswer.SetData(1, periodItem, oneStarAnswer);
        
    }
}
