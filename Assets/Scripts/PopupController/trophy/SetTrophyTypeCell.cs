using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SetTrophyTypeCell : ScrollCell {
    public Button btnCell;
    public AssetBundleSprite assetBundleSprite;
    public TrophyResultData trophyResult { get; set; }
    public override void configureCellData() {
        trophyResult = (TrophyResultData)DataObject;
        btnCell.interactable = !IsSelected;

        if (((PopupSetTrophy)Context).setTrophyData.courseRaceType == Course_Race_Type.CrtRankedMatch) {
            assetBundleSprite.SetAsset(trophyResult.previewBundleName, trophyResult.rankAssetName);
        }
        else {
            assetBundleSprite.SetAsset(trophyResult.previewBundleName, trophyResult.previewAssetName);
        }
        
    }

    public void OnClickCell() {
        IsSelected = true;
    }
}
