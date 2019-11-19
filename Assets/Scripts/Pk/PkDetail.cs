using UnityEngine;
using UnityEngine.UI;
using g_WebRequestManager = Singleton<WebRequestManager>;

public class PkDetail : MonoBehaviour {
    [SerializeField]
    private Text pointerInfo;
    [SerializeField]
    private Text createTime;
    [SerializeField]
    private Text description;
    [SerializeField]
    private Text title;

    [SerializeField]
    private PkAnswersView pkAnswers;
    [SerializeField]
    private GameObject uploadBtn;

    private PK pk;

    public void SetData(PK pk) {
        this.pk = pk;

        uploadBtn.SetActive(pk.PkCreateId != UserManager.Instance.UserId);
        createTime.text = pk.CreationTime.ToLocalTime().ToString("ui_pk_battle_creation_time".Localize());
        pointerInfo.text = pk.Parameter.jsPointInfo;
        description.text = pk.PkDescription;
        title.text = pk.PkName;
    }

    public void OnClickClose() {
        gameObject.SetActive(false);
    }

    public void OnClickPreview() {
        // HACK: disable rendering of UI to avoid hiding gameboard
        GetComponentInParent<Canvas>().enabled = false;
        PopupManager.GameboardPlayer(ProjectPath.Remote(pk.ProjPath),
                                     editable: true,
                                     customBindings: new Gameboard.GameboardCustomCodeGroups(),
                                     onClose: () => {
                                         GetComponentInParent<Canvas>().enabled = true;
                                     });
    }

    public void OnClickDownload() {
        if(GameboardRepository.instance.hasProject("", pk.PkName)) {
            PopupManager.YesNo("local_down_notice".Localize(pk.PkName), DownloadSaveGb);
        } else {
            DownloadSaveGb();
        }       
    }

    void DownloadSaveGb() {
        var request = new SaveProjectAsRequest();
        request.basePath = pk.ProjPath;
        request.saveAsType = CloudSaveAsType.GameBoard;
        request.saveAs = pk.PkName;
        request.blocking = true;
        request.Success(tRt => {
                GameboardRepository.instance.save(pk.PkName, tRt.FileList_);

                pk.PkDownloadCount++;
                pk.NotfiyUpdated();
            })
            .Execute();
    }

    public void OnClickUpload() {
        var helper = new PKAnswerUploadHelper(pk);
        helper.Upload();
    }

    public void OnClickPk() {
        pkAnswers.gameObject.SetActive(true);
        pkAnswers.SetData(pk);
    }
}
