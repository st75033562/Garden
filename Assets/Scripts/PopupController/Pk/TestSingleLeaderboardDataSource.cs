using System;
using System.Collections.Generic;

public class TestSingleLeaderboardDataSource : ISingleLeaderboardDataSource
{
    private int m_count;
    private bool m_hasMyRank;

    public TestSingleLeaderboardDataSource(int count, bool hasMyRank)
    {
        if (count < 0)
        {
            throw new ArgumentOutOfRangeException("count");
        }
        m_count = count;
        m_hasMyRank = hasMyRank;
    }

    public void Retrieve(Action<SingleLeaderboardResponse> callback)
    {
        var result = new SingleLeaderboardResponse();
        var ranks = new List<LeaderboardRecord>();
        result.ranks = ranks;

        if (m_count > 0)
        {
            int myRank = m_hasMyRank ? UnityEngine.Random.Range(0, m_count) : -1;
            for (int i = 0; i < m_count; ++i)
            {
                var rank = CreateRank(i + 1, (uint)(i + 1), "user" + i, m_count - i);
                ranks.Add(rank);
                if (myRank == i)
                {
                    result.myRank = rank;
                    rank.userId = UserManager.Instance.UserId;
                    rank.username = UserManager.Instance.Nickname;
                }
            }
        }
        callback(result);
    }

    private LeaderboardRecord CreateRank(int rank, uint userId, string username, int score)
    {
        return new LeaderboardRecord {
            userId = userId,
            rank = rank,
            username = username,
            score = score
        };
    }
}
