using System;
using System.IO;
using UnityEngine;

public class CompetitionProblemChallengeHelper
{
    public event Action onUploaded;

    private readonly CompetitionProblem m_problem;
    private readonly ICompetitionService m_service;

    public CompetitionProblemChallengeHelper(CompetitionProblem problem, ICompetitionService service)
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
    }

    public void UploadAnswer()
    {
        PopupManager.ProjectView(path => {
            EvaluateAnswer(path);
        }, showAddCell: false);
    }

    private void EvaluateAnswer(IRepositoryPath path)
    {
        int popupId = 0;
        popupId = PopupManager.GameboardPlayer(
            ProjectPath.Remote(m_problem.gameboardItem.url),
            new[] { RobotCodeInfo.Local(path.ToString(), UserManager.Instance.Nickname) },
            (Type, result) => {
                PopupManager.Close(popupId);
                if (Type == PopupILPeriod.PassModeType.Submit) {
                    UploadAnswer(path, result.robotScores[0]);
                }
                else {
                    UploadAnswer(path, result.sceneScore);
                }
            });
    }

    public void UploadAnswer(IRepositoryPath path, int score)
    {
        int maskId = PopupManager.ShowMask();
        var answerInfo = new CompetitionProblemAnswerInfo();
        answerInfo.userId = UserManager.Instance.UserId;
        answerInfo.userNickname = UserManager.Instance.Nickname;
        answerInfo.score = score;
        if (path != null) {
            answerInfo.path = path.ToString();
            answerInfo.answerName = UserManager.Instance.Nickname + "_" + path.name;
        }
        try
        {
            m_service.UploadAnswer(m_problem, answerInfo, res => {
                PopupManager.Close(maskId);
                if (res == Command_Result.CmdNoError)
                {
                    EventBus.Default.AddEvent(EventId.CompetitionProblemUpdated, m_problem);
                    if (onUploaded != null)
                    {
                        onUploaded();
                    }
                }
                else
                {
                    if (res == Command_Result.CmdCourseNotFound)
                    {
                        PopupManager.Notice("ui_pk_competition_already_deleted".Localize());
                    }
                    else
                    {
                        PopupManager.Notice("ui_pk_competition_upload_answer_failed".Localize());
                    }
                }
            });
        }
        catch (IOException e)
        {
            PopupManager.Close(maskId);
            Debug.LogError(e);
        }
    }
}
