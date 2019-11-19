using UnityEngine;
public class CompetitionProblemLeaderboardCellData
{
    public LeaderboardRecord rankRecord;
    public CompetitionProblem problem;

    public CompetitionProblemAnswer answer
    {
        get { return problem.GetAnswer(rankRecord.userId);}
    }

    public bool canPreview
    {
        get
        {
            return problem.gameboardItem != null && answer != null && problem.periodType == (uint)CompetitionProblem.PeriodType.Submit;
        }
    }
}


public class CompetitionProblemLeaderboardCell : PkLeaderboardCell
{
    public GameObject m_previewGo;

    public override void ConfigureCellData()
    {
        Configure(item.rankRecord);
        m_previewGo.SetActive(item.canPreview);
    }

    protected CompetitionProblemLeaderboardCellData item
    {
        get { return (CompetitionProblemLeaderboardCellData)dataObject; }
    }

    public void OnClickPreview()
    {
        PopupManager.GameboardPlayer(ProjectPath.Remote(item.problem.gameboardItem.url),
                                     new[] { RobotCodeInfo.Remote(item.answer.codePath, item.answer.userNickname) });
    }
}
