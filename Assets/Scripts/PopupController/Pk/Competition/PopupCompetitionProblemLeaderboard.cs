using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PopupCompetitionProblemLeaderboard : PopupController
{
    public ScrollableAreaController m_scrollController;
    public GameObject m_myRankUIRoot;
    public CompetitionProblemLeaderboardCell m_myRankCell;

    private CompetitionProblem m_problem;
    private ICompetitionService m_service;

    public void Initialize(CompetitionProblem problem, ICompetitionService service, bool showMyRank)
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
        m_problem.onAddedAnswer += Refresh;
        m_service = service;

        _titleText.text = m_problem.name;
        m_myRankUIRoot.SetActive(showMyRank);
        Refresh();
        UpdateMyRank(null);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        m_problem.onAddedAnswer -= Refresh;
    }

    public void OnClickChallenge()
    {
        var helper = new CompetitionProblemChallengeHelper(m_problem, m_service);
        helper.UploadAnswer();
    }

    public void ShowOverallLeaderboard()
    {
        PopupManager.CompetitionOverrallLeaderboard(m_problem.competition, m_service);
    }

    private void Refresh()
    {
        var dataSource = new RemoteSingleLeaderboardDataSource(m_problem.competition.id, m_problem.id);
        dataSource.Retrieve(res => {
            if (!this) { return; }

            var rankRecords = res.ranks.Select(x => new CompetitionProblemLeaderboardCellData {
                rankRecord = x,
                problem = m_problem
            }).ToArray();
            m_scrollController.InitializeWithData(rankRecords);

            if (m_myRankUIRoot.activeSelf)
            {
                UpdateMyRank(res.myRank);
            }
        });
    }

    private void UpdateMyRank(LeaderboardRecord myRank)
    {
        if (myRank == null)
        {
            myRank = new LeaderboardRecord {
                userId = UserManager.Instance.UserId,
                iconId = UserManager.Instance.AvatarID,
                username = UserManager.Instance.Nickname
            };
        }
        m_myRankCell.DataObject = new CompetitionProblemLeaderboardCellData {
            rankRecord = myRank,
            problem = m_problem,
        };
    }
}
