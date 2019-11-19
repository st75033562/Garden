using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Google.Protobuf;
using System;

using g_WebRequestManager = Singleton<WebRequestManager>;

public class GradeCell : ScrollableCell {
    [SerializeField]
    private Text taskName;
    [SerializeField]
    private Image gradeImage;
    [SerializeField]
    private Sprite[] spriteGrade;
    [SerializeField]
    private GameObject loadingImage;

    private TaskInfoCellData infoData;
    // Use this for initialization
    void Start () {
	
	}
	
    public override void ConfigureCellData ()
    {
        infoData = (TaskInfoCellData)dataObject;
        if(infoData == null)
            return;

        taskName.text = infoData.m_Name;
        TaskSubmitInfo taskInfos = infoData.SubmitList.Find ((x)=> { return x.m_ID == UserManager.Instance.UserId; });
        loadingImage.SetActive(false);

        if(taskInfos != null && taskInfos.m_Grade > 0) {
            gradeImage.gameObject.SetActive (true);
            gradeImage.sprite = spriteGrade[taskInfos.m_Grade - 1];
        } else {
            gradeImage.gameObject.SetActive (false);
        }
    }

    void NetFail(WebRequestData data)
    {
        taskName.text = "error";
        infoData.taskSceneController.NetFail(data);
    }

    public void OnClick () {
        if (loadingImage.activeSelf)
            return;

        var request = Downloads.DownloadTask(
            UserManager.Instance.CurClass.m_ID,
            infoData.m_ID,
            UserManager.Instance.UserId);

        request.blocking = true;
        request.Success(tRt => {
                var project = tRt.ToProject();
                if (project != null)
                {
                    SceneDirector.Push("Main", CodeSceneArgs.FromTempCode(project));
                }
                else
                {
                    Debug.LogError("project didn't download data");
                }
            })
        .Execute();
    }
}
