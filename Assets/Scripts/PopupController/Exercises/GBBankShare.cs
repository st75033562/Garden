using Google.Protobuf;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GBBankShare : GBBankShareBase
{

    
    // Use this for initialization
    protected override void OnEnable() {
        SharedStatus = Shared_Status.Public;
        base.OnEnable();
    }

    public override void ShowMask(bool showMask)
    {
        foreach (var data in bankCellData)
        {
            data.showMask = showMask;
        }
        base.ShowMask(showMask);
    }

    public void OnClickCreateFloder()
    {
        if (gameObject.activeSelf)
        {
            base.OnClickAddFloder(null);
        }
    }


    public void OnClick(GBBankCell cell)
    {
        if (gameObject.activeSelf) {
            OnClick(cell, null);
        }
    }
}
