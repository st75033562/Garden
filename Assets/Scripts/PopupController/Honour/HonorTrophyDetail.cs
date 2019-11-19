using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HonorTrophyDetail : MonoBehaviour {
    public Text textName;
    public Text textContent;
    public Text textTime;
    public AssetBundleSprite assetBundleSprite;

    public void SetData(UserTrophy trophy) {
        textName.text = UserManager.Instance.Nickname;
        

        TrophyResultData data = TrophyResultData.GeTrophyData(trophy.trophyPb.trophyResultId);
        assetBundleSprite.SetAsset(data.assetBundleName, data.assetName);

        
        string medal = null, ranking = null;
        if(trophy.awardTrophy == Trophy_Type.TtGold) {
            medal = "ui_trophy_result_gold".Localize();
            ranking = "ui_trophy_result_1".Localize();
        } else if(trophy.awardTrophy == Trophy_Type.TtSilver) {
            medal = "ui_trophy_result_silver".Localize();
            ranking = "ui_trophy_result_2".Localize();
        } else {
            medal = "ui_trophy_result_copper".Localize();
            ranking = "ui_trophy_result_3".Localize();
        }

        string strCnontent = null;
        if(trophy.courseRaceType == Course_Race_Type.CrtRankedMatch) {
            strCnontent = string.Format("ui_trophy_result_rank".Localize(), trophy.trophyPb.courseName, ranking);
        } else {
            strCnontent = string.Format("ui_trophy_result_routine".Localize(), trophy.trophyPb.courseName, trophy.courseScore.ToString(), medal);
        }
        textContent.text = strCnontent;

        textTime.text = TimeUtils.GetLocalizedTime((long)trophy.awardTime);
    }

    public void OnClickClose() {
        gameObject.SetActive(false);
    }
}
