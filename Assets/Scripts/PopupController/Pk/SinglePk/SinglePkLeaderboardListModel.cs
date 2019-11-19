using System;
using System.Collections.Generic;
using System.Linq;

public class SinglePkLeaderboardItem
{
    private readonly GameBoard m_gameboard;

    public SinglePkLeaderboardItem(LeaderboardRecord rank, GameBoard gameboard)
    {
        if (rank == null)
        {
            throw new ArgumentNullException("rank");
        }
        if (gameboard == null)
        {
            throw new ArgumentNullException("gameboard");
        }

        rankRecord = rank;
        m_gameboard = gameboard;
        answer = m_gameboard.GetUserAnswer(rank.userId);
    }

    public LeaderboardRecord rankRecord { get; private set; }

    public GBAnswer answer { get; private set; }

    public uint gameboardId {  get { return m_gameboard.GbId; } }

    public string gameboardPath { get { return m_gameboard.ProjPath; } }

    public bool isPlayMode { get { return (PopupILPeriod.PassModeType)m_gameboard.Parameter.jsPassMode == PopupILPeriod.PassModeType.Play; } }
}

public class SinglePkLeaderboardListModel : SimpleListItemModel<SinglePkLeaderboardItem>
{
    private readonly GameBoard m_gameboard;
    private readonly ISingleLeaderboardDataSource m_leaderboard;
    private bool m_fetching;

    public SinglePkLeaderboardListModel(GameBoard gameboard, ISingleLeaderboardDataSource leaderboard)
        : base(new List<SinglePkLeaderboardItem>())
    {
        if (gameboard == null)
        {
            throw new ArgumentNullException("gameboard");
        }
        if (leaderboard == null)
        {
            throw new ArgumentNullException("leaderboard");
        }
        m_gameboard = gameboard;
        m_leaderboard = leaderboard;
        InitMyDataItem(null);
    }

    public void fetch()
    {
        if (m_fetching)
        {
            return;
        }

        m_fetching = true;
        m_leaderboard.Retrieve(res => {
            m_fetching = false;

            items.Clear();
            items.AddRange(res.ranks.Select(x => CreateItem(x)));

            InitMyDataItem(res.myRank);
            fireReset();
        });
    }

    private void InitMyDataItem(LeaderboardRecord rank)
    {
        if (rank == null)
        {
            rank = new LeaderboardRecord {
                userId = UserManager.Instance.UserId,
                iconId = UserManager.Instance.AvatarID,
                username = UserManager.Instance.Nickname
            };
        }
        myItem = CreateItem(rank);
    }

    private SinglePkLeaderboardItem CreateItem(LeaderboardRecord rank)
    {
        return new SinglePkLeaderboardItem(rank, m_gameboard);
    }

    public SinglePkLeaderboardItem myItem
    {
        get;
        private set;
    }
}
