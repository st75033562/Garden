using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PublishItemCell : ScrollCell {
    public PublishPeriodItem publishPeriodItem;
    public Sprite[] iconSprites;
    public Image icon;
    public Text itemNameText;
    public Text finishRate;

    public override void configureCellData() {
        Period_Item_Info periodItem = (Period_Item_Info)DataObject;
        icon.sprite = iconSprites[periodItem.ItemType];
        itemNameText.text = periodItem.ItemName;
        if(publishPeriodItem.periodInfo.UnlockedUserCount == 0)
            finishRate.text = "0%";
        else
            finishRate.text = Math.Round(((float)periodItem.ItemCompletedUserCount / publishPeriodItem.periodInfo.UnlockedUserCount) * 100, 2) + "%";
    }
    public virtual void SetData(Period_Info periodInfo , Period_Item_Info periodItem) {
        
    }
}
