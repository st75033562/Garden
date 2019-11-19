using System;
using System.Collections.Generic;

public class SingleLeaderboardResponse
{
    public IEnumerable<LeaderboardRecord> ranks;
    public LeaderboardRecord myRank;
}

public interface ISingleLeaderboardDataSource
{
    void Retrieve(Action<SingleLeaderboardResponse> callback);
}