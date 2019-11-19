using Google.Protobuf;
using UnityEngine;
using UnityEngine.UI;

using g_WebRequestManager = Singleton<WebRequestManager>;

public class SinglePkLeaderboardCell : PkLeaderboardCell
{
    public UILikeWidget m_likeWidget;
    public GameObject m_downloadGo;
    public GameObject m_previewGo;
    public GameObject m_lockGo;

    public Text m_downloadCount;

    public override void ConfigureCellData()
    {
        Configure(item.rankRecord);

        bool hasAnswer = item.answer != null;
        m_downloadGo.SetActive(hasAnswer && !item.isPlayMode);
        m_previewGo.SetActive(hasAnswer && !item.isPlayMode);
        m_likeWidget.gameObject.SetActive(hasAnswer);

        if (hasAnswer)
        {
            m_downloadCount.text = item.answer.DownloadCount.ToString();
            m_lockGo.SetActive(item.answer.GbScriptShow == 0);

            UpdateLikeWidget(false);
        }
    }

    private void UpdateLikeWidget(bool showAnim)
    {
        m_likeWidget.likeCount = item.answer.GbLikeList.Count;
        m_likeWidget.SetLiked(item.answer.Liked(UserManager.Instance.UserId), showAnim);
    }

    private SinglePkLeaderboardItem item
    {
        get { return (SinglePkLeaderboardItem)dataObject; }
    }

    public void OnClickDownLoad()
    {
        if (CodeProjectRepository.instance.hasProject("", item.answer.AnswerName))
        {
            PopupManager.YesNo("local_down_notice".Localize(item.answer.AnswerName), DownloadSaveAnswer);
        }
        else
        {
            DownloadSaveAnswer();
        }
    }

    void DownloadSaveAnswer()
    {
        var request = new SaveProjectAsRequest();
        request.basePath = item.answer.ProjPath;
        request.saveAs = item.answer.AnswerName;
        request.saveAsType = CloudSaveAsType.Project;
        request.blocking = true;
        request.Success(fileList => {
                CodeProjectRepository.instance.save(item.answer.AnswerName, fileList.FileList_);
                item.answer.DownloadCount++;

                ConfigureCellData();
            })
            .Execute();
    }

    public void OnClickLook()
    {
        PopupManager.GameboardPlayer(ProjectPath.Remote(item.gameboardPath), new[] { item.answer.ToRobotCodeInfo() });
    }

    public void OnClickLike()
    {
        var like_r = new CMD_Like_Gameboard_r_Parameters();
        like_r.GbId = item.gameboardId;
        like_r.AnswerId = item.answer.AnswerId;

        var answer = item.answer;
        bool liked = answer.Liked(UserManager.Instance.UserId);
        answer.SetLiked(UserManager.Instance.UserId, !liked);
        UpdateLikeWidget(true);

        SocketManager.instance.send(Command_ID.CmdLikeGameboardR, like_r.ToByteString(), (res, content) => {
            if (res != Command_Result.CmdNoError && answer == item.answer)
            {
                answer.SetLiked(UserManager.Instance.UserId, liked);
                UpdateLikeWidget(false);
            }
        });
    }
}
