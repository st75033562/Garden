using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectCertificateCell : ScrollCell {
    public Button btnSelf;
    public AssetBundleSprite assetBundleSprite;

    public CertificateData certificateData { set; get; }
    public override void configureCellData() {
        SwitchBtn(true);
        certificateData = (CertificateData)DataObject;
        assetBundleSprite.SetAsset(certificateData.assetBundleName, certificateData.assetName);
    }

    public void SwitchBtn(bool state) {
        btnSelf.interactable = state;
    }
}
