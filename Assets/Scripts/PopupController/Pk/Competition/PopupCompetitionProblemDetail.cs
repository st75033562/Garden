using Gameboard;
using System;
using System.Linq;
using UnityEngine.UI;

public class PopupCompetitionProblemDetail : PopupController
{
    public Text m_endTimeText;
    public Text m_descText;
    public ScrollableAreaController m_attachmentView;

    public LayoutElement m_playButtonLayout;
    public Button m_playButton;
    public Button m_uploadButton;

    private CompetitionProblem m_problem;
    private ICompetitionService m_service;
    private bool m_showUploadButton;

    public void Initialize(CompetitionProblem problem, ICompetitionService service, bool showUploadButton)
    {
        if (problem == null)
        {
            throw new ArgumentNullException("problem");
        }
        if (service == null)
        {
            throw new ArgumentNullException("service");
        }

        m_problem = problem;
        m_service = service;
        m_showUploadButton = showUploadButton;
    }

    protected override void Start()
    {
        base.Start();

        var endTime = m_problem.competition.endTime.ToLocalTime().ToString("ui_pk_competition_date_format".Localize());
        m_endTimeText.text = "ui_pk_competition_problem_deadline".Localize(endTime);

        _titleText.text = m_problem.name;
        m_descText.text = m_problem.description;

        UpdateButtons();
        m_attachmentView.InitializeWithData(CompetitionUtils.GetAttachmentResources(m_problem).ToArray());
        m_problem.onAddedAnswer += UpdateButtons;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        m_problem.onAddedAnswer -= UpdateButtons;
    }

    private void UpdateButtons()
    {
        bool validProblem = m_problem.gameboardItem != null;
        m_playButton.interactable = validProblem;
        m_uploadButton.interactable = validProblem;

        if (validProblem && m_problem.GetScore(UserManager.Instance.UserId) >= 0 || !m_showUploadButton || m_problem.periodType == (int)CompetitionProblem.PeriodType.Play)
        {
            m_uploadButton.gameObject.SetActive(false);
        }

        m_playButtonLayout.enabled = !m_uploadButton.gameObject.activeSelf;
    }

    private void OnProblemUpdated(object arg)
    {
        if (arg == m_problem)
        {
            UpdateButtons();
        }
    }

    public void OnClickPlay()
    {
        var player = PopupManager.GameboardPlayer();

        SubmitHandler submitHandler = null;
        if (m_problem.GetScore(UserManager.Instance.UserId) < 0 && !UserManager.Instance.IsAdmin)
        {
            submitHandler = (path, mode, res) => {
                int score = 0;
                if (mode == PopupILPeriod.PassModeType.Submit)
                {
                    score = res.robotScores[0];
                }
                else
                {
                    score = res.sceneScore;
                }
                var helper = new CompetitionProblemChallengeHelper(m_problem, m_service);
                helper.onUploaded += () => {
                    player.Close();
                    Close();
                };
                helper.UploadAnswer(path, score);
            };
        }

        player.payload = new GameboardPlayerConfig {
            gameboardPath = ProjectPath.Remote(m_problem.gameboardItem.url),
            submitHandler = submitHandler,
            editable = true,
            customBindings = GetCustomCodeGroups()
        };
    }

    private Gameboard.GameboardCustomCodeGroups GetCustomCodeGroups()
    {
        var key = CompetitionCodeGroupsKey.Create(m_problem);
        var setting = (GameboardCodeGroups)UserManager.Instance.userSettings.Get(key, true);
        return new Gameboard.GameboardCustomCodeGroups(setting.codeGroups);
    }

    public void OnClickUploadAnswer()
    {
        var helper = new CompetitionProblemChallengeHelper(m_problem, m_service);
        helper.onUploaded += () => {
            Close();
        };
        helper.UploadAnswer();
    }

    public void OnClickLeaderboard()
    {
        PopupManager.CompetitionProblemLeaderboard(m_problem, m_service, !UserManager.Instance.IsAdmin);
    }
}
