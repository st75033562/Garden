using Gameboard;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using g_WebRequestManager = Singleton<WebRequestManager>;

public class PopupSinglePkDetail : PopupController
{
    public Text m_createTimeText;
    public Text m_startPointInfoText;
    public Text m_descriptionText;
    public GameObject m_leaderboardButton;
    public GameObject m_uploadButton;
    public GameObject m_downloadButton;
    public GameObject m_previewButton;
    public GameObject m_playButton;

    private GameBoard m_gameboard;
    private PopupILPeriod.PassModeType passModeType;

    protected override void Start()
    {
        base.Start();

        m_gameboard = payload as GameBoard;
        m_createTimeText.text = m_gameboard.CreationTime.ToLocalTime().ToString("ui_single_pk_creation_time".Localize());
        m_startPointInfoText.text = m_gameboard.Parameter.jsPointInfo;
        passModeType = (PopupILPeriod.PassModeType)m_gameboard.Parameter.jsPassMode;
        m_descriptionText.text = m_gameboard.GbDescription;

        m_uploadButton.SetActive(m_gameboard.GbCreateId != UserManager.Instance.UserId);

        if(passModeType == PopupILPeriod.PassModeType.Submit) {
            m_uploadButton.SetActive(true);
            m_downloadButton.SetActive(true);
            m_previewButton.SetActive(true);
            m_playButton.SetActive(false);
        } else {
            m_uploadButton.SetActive(false);
            m_downloadButton.SetActive(false);
            m_previewButton.SetActive(false);
            m_playButton.SetActive(true);
        }
    }

    public void ShowLeaderboardButton(bool visible)
    {
        m_leaderboardButton.SetActive(visible);
    }

    public void OnClickPk()
    {
        PopupSinglePk singlePK = FindObjectOfType<PopupSinglePk>();
        if (singlePK != null)
        {
            singlePK.ShowCanvas(false);
        }
        PopupManager.SinglePkLeaderboard(m_gameboard);
    }

    public void OnClickPlay()
    {
        PopupSinglePk singlePK = FindObjectOfType<PopupSinglePk>();
        if (singlePK != null)
        {
            singlePK.ShowCanvas(false);
        }
        PopupManager.GameboardPlayer(ProjectPath.Remote(m_gameboard.ProjPath),
            editable: true,
            customBindings: new Gameboard.GameboardCustomCodeGroups());
    }

    public void OnClickUpload()
    {
        var helper = new SinglePkChallengeHelper(m_gameboard);
        helper.UploadAnswer();
    }

    public void OnClickPlayGame() {
        PopupSinglePk singlePK = FindObjectOfType<PopupSinglePk>();
        if (singlePK != null)
        {
            singlePK.ShowCanvas(false);
        }

        var helper = new SinglePkChallengeHelper(m_gameboard);
        helper.EvaluateAnswer(null, false, (PopupILPeriod.PassModeType)m_gameboard.Parameter.jsPassMode);
    }
}
