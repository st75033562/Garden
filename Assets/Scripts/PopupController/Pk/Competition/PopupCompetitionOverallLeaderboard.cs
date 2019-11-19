using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PopupCompetitionOverallLeaderboard : PopupController
{
    public ScrollableAreaController m_scrollController;
    public Text m_leaderboardNameText;
    public CompetitionLeaderboardCell m_myRankCell;

    private Competition m_competition;
    private ICompetitionService m_service;

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
    }

    protected override void Start()
    {
        base.Start();

        m_leaderboardNameText.text = m_competition.name;
        m_service.GetLeaderboard(m_competition.id, OnGetLeaderboard);

        m_myRankCell.DataObject = CreateCellData(new CompetitionLeaderboardRecord {
            iconId = UserManager.Instance.AvatarID,
            userId = UserManager.Instance.UserId,
            username = UserManager.Instance.Nickname
        });
    }
    
    private void OnGetLeaderboard(CompetitionLeaderboardResult result)
    {
        if (result.myRank != null)
        {
            m_myRankCell.DataObject = CreateCellData(result.myRank);
        }
        m_scrollController.InitializeWithData(result.ranks.Select(x => CreateCellData(x)).ToArray());
    }

    private CompetitionLeaderboarCellData CreateCellData(CompetitionLeaderboardRecord rankRecord)
    {
        return new CompetitionLeaderboarCellData {
            rankRecord = rankRecord,
            totalProblemCount = m_competition.problemCount
        };
    }
}
