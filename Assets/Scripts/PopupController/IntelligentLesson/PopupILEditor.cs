using Google.Protobuf;
using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopupILEditor : PopupController {
    public class PayLoad {
        public Course_Info courseInfo;
        public Action<CourseInfo> callBack;
    }
    public InputField lessonName;
    public InputField LessonDescribe;
    public UIImageMedia coverImage;
    public Button btnConfirm;
    public AssetBundleSprite certificateCover;
    public Button btnAddCertificate;
    public Text titleText;
    public Toggle toggleDownloadable;

    private LocalResData res;
    private Course_Info editorCourseInfo;
    private int certificateId;

    private const int referenceHeight = 1080;
    private Action<CourseInfo> callBack;

    protected override void Start() {
        base.Start();
        var data = (PayLoad)payload;
        callBack = data.callBack;
        SetDataAndShow(data.courseInfo);
    }

    void SetDataAndShow(Course_Info courseInfo) {
        gameObject.SetActive(true);
        editorCourseInfo = courseInfo;
        coverImage.gameObject.SetActive(false);
        certificateCover.gameObject.SetActive(false);
        btnAddCertificate.interactable = true;

        if(courseInfo == null) {
            lessonName.text = "";
            LessonDescribe.text = "";
            titleText.text = "Ui_text_addcourse".Localize();
            toggleDownloadable.isOn = true;
        } else {
            titleText.text = "Ui_text_editcourse".Localize();
            lessonName.text = courseInfo.CourseName;
            LessonDescribe.text = courseInfo.CourseDescription;
            toggleDownloadable.isOn = courseInfo.CourseAllowDownloadGb;

            if(!string.IsNullOrEmpty(courseInfo.CourseCoverImageUrl)) {
                coverImage.gameObject.SetActive(true);
                coverImage.SetImage(courseInfo.CourseCoverImageUrl);
            }
            var courseHonor = courseInfo.CourseHonorSetting;
            if(courseHonor != null && courseHonor.CourseCertificateSetting != null) {
                var certificate = new CertificateSetting();
                certificate.ParseJson(courseHonor.CourseCertificateSetting.CertificateJsonSetting);
                CertificateData data = CertificateData.GetCerticateData(certificate.certificateId);
                certificateCover.gameObject.SetActive(true);
                certificateCover.SetAsset(data.assetBundleName, data.assetName);
                btnAddCertificate.interactable = false;
                certificateId = certificate.certificateId;
            }
        }
    }

    protected override void OnEnable() {
        base.OnEnable();
        OnInputCourseName();
    }
    public void OnClickAddCover() {
        LocalResOperate.instance.OpenResWindow(LocalResType.IMAGE, (data) => {
            Texture2D texture = new Texture2D(0, 0);
            if(!texture.LoadImage(data.imageData)) {
                Destroy(texture);
                Debug.LogError("failed to load image " + data.path);
                return;
            }

            if(texture.height > referenceHeight) {
                TextureScale.Bilinear(texture, referenceHeight);
            }

            res = LocalResData.Image(texture.EncodeToPNG());
            coverImage.gameObject.SetActive(true);
            coverImage.SetImage(texture);
        });
    }

    public void OnClickConfirm() {
        if(res == null) {
            SendCourseInfo();
            return;
        }

        Uploads.UploadMedia(res.textureData, res.name, false)
               .Blocking()
               .Success(() => {
                   SendCourseInfo();
                   res = null;
               })
               .Execute();
    }

    void SendCourseInfo() {
        int maskId = PopupManager.ShowMask();
        if(editorCourseInfo != null) {  //修改
            UpdateCourseInfo(editorCourseInfo);

            var courseR = new CMD_Modify_Course_r_Parameters();
            courseR.ModifyInfo = editorCourseInfo;
            SocketManager.instance.send(Command_ID.CmdModifyCourseR, courseR.ToByteString(), (res, content) => {
                PopupManager.Close(maskId);
                if(res == Command_Result.CmdNoError) {
                    gameObject.SetActive(false);
                    callBack(null);
                    Close();
                } else {
                    Debug.LogError("CmdCreateCourseR:" + res);
                }
            });
        } else {
            var courseInfo = new Course_Info();
            UpdateCourseInfo(courseInfo);

            var courseR = new CMD_Create_Course_r_Parameters();
            courseR.CreateInfo = courseInfo;
            SocketManager.instance.send(Command_ID.CmdCreateCourseR, courseR.ToByteString(), (res, content) => {
                PopupManager.Close(maskId);
                if(res == Command_Result.CmdNoError) {
                    CMD_Create_Course_a_Parameters courseA = CMD_Create_Course_a_Parameters.Parser.ParseFrom(content);
                    callBack(new CourseInfo().ParseProtobuf(courseA.CourseInfo));
                    Close();
                } else {
                    Debug.LogError("CmdCreateCourseR:" + res);
                }
            });
        }
    }

    void UpdateCourseInfo(Course_Info courseInfo) {
        courseInfo.CourseName = lessonName.text;
        courseInfo.CourseDescription = LessonDescribe.text;
        courseInfo.CourseAllowDownloadGb = toggleDownloadable.isOn;
        if(Preference.scriptLanguage == ScriptLanguage.Visual) {
            courseInfo.CourseProjectLanguageType = Project_Language_Type.ProjectLanguageGraphy;
        } else {
            courseInfo.CourseProjectLanguageType = Project_Language_Type.ProjectLanguagePython;
        }
        if(res != null) {
            courseInfo.CourseCoverImageUrl = res.name;
        }
        UpdateCertificateSetting(courseInfo);
    }

    void UpdateCertificateSetting(Course_Info courseInfo) {
        if(certificateId != 0) {
            JsonData jd = new JsonData();
            jd["certificateId"] = certificateId;
            jd["courseName"] = courseInfo.CourseName;

            Certificate_Setting certificateSetting = new Certificate_Setting();
            certificateSetting.CertificateJsonSetting = jd.ToJson();
            if(courseInfo.CourseHonorSetting != null) {
                courseInfo.CourseHonorSetting.CourseCertificateSetting = certificateSetting;
            } else {
                Course_Honor_Setting honorSetting = new Course_Honor_Setting();
                honorSetting.CourseCertificateSetting = certificateSetting;
                courseInfo.CourseHonorSetting = honorSetting;
            }
        }
    }

    public void OnClickDeleteCertificate() {
        if(editorCourseInfo != null && editorCourseInfo.CourseHonorSetting != null &&
            editorCourseInfo.CourseHonorSetting.CourseCertificateSetting != null) {
            CMD_Del_Course_Certificate_Setting_r_Parameters delCertificate = new CMD_Del_Course_Certificate_Setting_r_Parameters();
            delCertificate.CourseId = editorCourseInfo.CourseId;

            int popuoId = PopupManager.ShowMask();
            SocketManager.instance.send(Command_ID.CmdDelCourseCertificateSettingR, delCertificate.ToByteString(), (res, content) => {
                PopupManager.Close(popuoId);
                if(res != Command_Result.CmdNoError) {
                    PopupManager.Notice(res.ToString());
                }
            });
        }
        btnAddCertificate.interactable = true;
        certificateCover.gameObject.SetActive(false);
    }

    public void OnClickSelectCertificate() {
        PopupManager.SelectCertificate(titleText.text, (certificateId) => {
            CertificateData certificateData = CertificateData.GetCerticateData(certificateId);
            certificateCover.gameObject.SetActive(true);
            certificateCover.SetAsset(certificateData.assetBundleName, certificateData.assetName);
            btnAddCertificate.interactable = false;

            this.certificateId = certificateId;
        });
    }

    public void OnInputCourseName() {
        btnConfirm.interactable = !string.IsNullOrEmpty(lessonName.text);
    }
}
