using Google.Protobuf;
using System;
using UnityEngine;
using UnityEngine.UI;

public class PopupStudentCompetitionProblems : PopupController
{
    public ScrollableAreaController m_scrollController;
    public Text m_titleText;
    public GameObject btnComplete;

    private Competition m_competition;
    private ICompetitionService m_service;
    private CompetitionProblemListModel m_model;

    public void Initialize(Competition competition, ICompetitionService service)
    {
        if (competition == null)
        {
            throw new ArgumentNullException("competition");
        }
        if (service == null)
        {
            throw new ArgumentNullException("service");
        }

        m_competition = competition;
        m_service = service;
        m_model = new CompetitionProblemListModel(competition, false);
    }

    protected override void Start()
    {
        base.Start();

        m_titleText.text = m_competition.name;
        m_scrollController.InitializeWithData(m_model);
        
        UpdateCompleteButton();

        EventBus.Default.AddListener(EventId.CompetitionProblemUpdated, OnProblemUpdated);
    }

    void UpdateCompleteButton()
    {
        bool isRegularSeason = false;
        var courseTrophySettting = m_competition.courseTrophySetting;
        if (courseTrophySettting != null && courseTrophySettting.courseRaceType == Course_Race_Type.CrtRegularSeason)
        {
            isRegularSeason = true;
        }

        //btnComplete.SetActive(m_competition.state == Competition.OpenState.Open && 
        //                      m_competition.UserJoinedAll(UserManager.Instance.UserId)
        //                      && !UserManager.Instance.IsCourseFociblyEnded(m_competition.id) && isRegularSeason);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        EventBus.Default.RemoveListener(EventId.CompetitionProblemUpdated, OnProblemUpdated);
    }

    private void OnProblemUpdated(object obj)
    {
        UpdateCompleteButton();
        m_scrollController.model.updatedItem(obj);
    }

    public void ShowOverallLeaderboard()
    {
        PopupManager.CompetitionOverrallLeaderboard(m_competition, m_service);
    }

    public void OnClickProblemCell(CompetitionProblemStudentCell cell)
    {
        //PopupManager.CompetitionProblemDetail(cell.problem, m_service, 
        //                                      (cell.problem.competition.state == Competition.OpenState.Open) && 
        //                                      !UserManager.Instance.IsCourseFociblyEnded(m_competition.id));

        PopupManager.CompetitionProblemLeaderboard(cell.problem, m_service, !UserManager.Instance.IsAdmin);
    }

    public void OnClickPlay(CompetitionProblemStudentCell cell)
    {
        var player = PopupManager.GameboardPlayer();

        Gameboard.SubmitHandler submitHandler = null;
        if (cell.problem.GetScore(UserManager.Instance.UserId) < 0 && !UserManager.Instance.IsAdmin)
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
                var helper = new CompetitionProblemChallengeHelper(cell.problem, m_service);
                helper.onUploaded += () => {
                    player.Close();
                    //  Close();
                    OnClickCloseCourse();
                };
                helper.UploadAnswer(path, score);
            };
        }

        player.payload = new GameboardPlayerConfig
        {
            gameboardPath = ProjectPath.Remote(cell.problem.gameboardItem.url),
            submitHandler = submitHandler,
            editable = true,
            customBindings = GetCustomCodeGroups(cell.problem)
        };
    }

    private Gameboard.GameboardCustomCodeGroups GetCustomCodeGroups(CompetitionProblem m_problem)
    {
        var key = CompetitionCodeGroupsKey.Create(m_problem);
        var setting = (GameboardCodeGroups)UserManager.Instance.userSettings.Get(key, true);
        return new Gameboard.GameboardCustomCodeGroups(setting.codeGroups);
    }


    public void OnClickCloseCourse()
    {
        if (m_competition.UserJoinedAll(UserManager.Instance.UserId))
        {
          //  PopupManager.CloseCourse(() => {
                int popupId = PopupManager.ShowMask();
                m_service.EndCompetition(m_competition.id, (res) => {
                    PopupManager.Close(popupId);
                    if (res == Command_Result.CmdNoError)
                    {
                        UserManager.Instance.ForceEndingCourse(m_competition.id);
                        btnComplete.SetActive(false);
                    }
                    else
                    {
                        PopupManager.Notice(res.ToString());
                    }
                });
            //});
        }
    }
}
