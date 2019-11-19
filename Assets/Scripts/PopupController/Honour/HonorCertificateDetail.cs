using LitJson;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HonorCertificateDetail : MonoBehaviour {
    public Text textName;
    public Text textContent;
    public Text textTime;
    public AssetBundleSprite assetBundleSprite;

    public void OnClickSetData(HonorCerificateCell cell) {
        gameObject.SetActive(true);
        textName.text = UserManager.Instance.Nickname;
        textContent.text = string.Format("ui_certificate_content".Localize(), cell.userCertificateInfo.courseName);
        textTime.text = TimeUtils.GetLocalizedTime((long)cell.userCertificateInfo.awardTime);

        CertificateData data = CertificateData.GetCerticateData(cell.userCertificateInfo.certificateId);
        assetBundleSprite.SetAsset(data.resultBundle, data.resultName);
    }

    public void OnClickClose() {
        gameObject.SetActive(false);
    }
}
