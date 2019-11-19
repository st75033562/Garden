using LitJson;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HonorCerificateCell : ScrollCell {
    public Text textName;
    public AssetBundleSprite assetBundleSprite;

    public UserCertificate userCertificateInfo { get; set; }

    public override void configureCellData() {
        userCertificateInfo = (UserCertificate)DataObject;
        textName.text = userCertificateInfo.courseName;
        CertificateData data = CertificateData.GetCerticateData(userCertificateInfo.certificateId);
        assetBundleSprite.SetAsset(data.assetBundleName, data.assetName);
    }
}
