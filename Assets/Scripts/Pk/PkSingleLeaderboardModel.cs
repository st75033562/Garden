using System;
using System.Collections.Generic;

public class PkSingleLeaderboardModel : SimpleListItemModel<LeaderboardRecord>
{
    public event Action onInitialized;

    private readonly ISingleLeaderboardDataSource m_dataSource;

    private bool m_fetched;
    private LeaderboardRecord m_myRank;

    public PkSingleLeaderboardModel(ISingleLeaderboardDataSource dataSource)
        : base(new List<LeaderboardRecord>())
    {
        if (dataSource == null)
        {
            throw new ArgumentNullException("dataSource");
        }
        m_dataSource = dataSource;
    }

    public override void fetchMore()
    {
        if (m_fetched)
        {
            return;
        }

        m_fetched = true;
        m_dataSource.Retrieve(result => {
            m_fetched = true;

            items.AddRange(result.ranks);
            m_myRank = result.myRank;
            didInsertItems(new Range(0, items.Count));

            if (onInitialized != null)
            {
                onInitialized();
            }
        });
    }

    public LeaderboardRecord myRank
    {
        get { return m_myRank; }
    }
}
