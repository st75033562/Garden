using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProphyPartCell : ScrollCell {
    public AssetBundleSprite assetBundleSprite;

    public TrophyData trophyData { get; set; }

    public override void configureCellData() {
        trophyData = (TrophyData)DataObject;
        assetBundleSprite.SetAsset(trophyData.assetBundleName, trophyData.assetNameGold);
    }
}
