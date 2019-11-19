using Google.Protobuf;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class popupCertificateNotify : PopupController {
    public Text textCourseName;
    public AssetBundlePrefab assetbundlePrefab;

    private UserCertificate userCertificate;
    protected override void Start () {
        base.Start();
        assetbundlePrefab.loadFinished += LoadFinish;
        userCertificate = (UserCertificate)payload;

        var certificateData = CertificateData.GetCerticateData(userCertificate.certificateId);
        assetbundlePrefab.SetAsset(certificateData.notifyBundle, certificateData.notifyName);
        textCourseName.text = userCertificate.courseName;
    }

    public void OnClickConfirm() {
        int popupId = PopupManager.ShowMask();
        var setHonorState = new CMD_Update_Honorwall_State_r_Parameters();
        setHonorState.CourseId = userCertificate.courseId;
        setHonorState.Trophy = 0;
        SocketManager.instance.send(Command_ID.CmdUpdateHonorwallStateR, setHonorState.ToByteString(), (res, content)=> {
            PopupManager.Close(popupId);
            if(res != Command_Result.CmdNoError) {
                PopupManager.Notice(res.Localize());
            }
            OnCloseButton();
        });
    }


    void LoadFinish() {
        string name = UserManager.Instance.Nickname;
        string content = string.Format("ui_certificate_content".Localize(), userCertificate.courseName);
        string time = TimeUtils.GetLocalizedTime((long)userCertificate.awardTime);
        GameObject go = assetbundlePrefab.GetInstance();
        go.GetComponent<CertificateNotify>().SetData(name, content, time);
    }

    protected override void OnDestroy() {
        base.OnDestroy();
        assetbundlePrefab.loadFinished -= LoadFinish;
    }
}
