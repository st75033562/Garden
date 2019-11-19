using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HonorTrophyCell : ScrollCell {
    public Text[] textCourseName;
    public AssetBundleSprite[] imgBody;
    public AssetBundleSprite[] imgBase;
    public AssetBundleSprite[] imgHandle;
    public AssetBundleSprite[] imgPattern;
    public GameObject[] trophyCells;
    public HonorTrophyDetail honorTrophyDetail;
    private List<UserTrophy> userTrophies;

    private const int TrophiesPerRow = 4;

    public override void configureCellData() {
        userTrophies = (List<UserTrophy>)DataObject;

        for (int i=0; i< TrophiesPerRow; i++)
        {
            if(i >= userTrophies.Count) {
                trophyCells[i].gameObject.SetActive(false);
                continue;
            }

            trophyCells[i].gameObject.SetActive(true);
            textCourseName[i].text = userTrophies[i].trophyPb.courseName;

            UpdateImage(imgBody[i], userTrophies[i].awardTrophy, userTrophies[i].trophyPb.bodyId);
            UpdateImage(imgBase[i], userTrophies[i].awardTrophy, userTrophies[i].trophyPb.baseId);
            UpdateImage(imgHandle[i], userTrophies[i].awardTrophy, userTrophies[i].trophyPb.handleId);
            UpdateImage(imgPattern[i], userTrophies[i].awardTrophy, userTrophies[i].trophyPb.patternId);
        }
    }

    private void UpdateImage(AssetBundleSprite sprite, Trophy_Type type, int partId)
    {
        if(partId != 0) {
            sprite.gameObject.SetActive(true);
            var data = TrophyData.GetTrophyData(partId);
            if(type == Trophy_Type.TtGold) {
                sprite.SetAsset(data.assetBundleName, data.assetNameGold);
            } else if(type == Trophy_Type.TtSilver) {
                sprite.SetAsset(data.assetBundleName, data.assetNameSilver);
            } else {
                sprite.SetAsset(data.assetBundleName, data.assetNameBronze);
            }
        } else {
            sprite.gameObject.SetActive(false);
        }
    }

    public void OnClickCell(int index) {
        honorTrophyDetail.gameObject.SetActive(true);
        honorTrophyDetail.SetData(userTrophies[index]);
    }
}
