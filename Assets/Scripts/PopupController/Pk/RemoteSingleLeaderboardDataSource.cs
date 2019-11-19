using Google.Protobuf;
using System;
using System.Linq;
using UnityEngine;

public class RemoteSingleLeaderboardDataSource : ISingleLeaderboardDataSource
{
    private Action<SingleLeaderboardResponse> m_callback;

    private readonly uint m_gameboardId;
    private readonly uint m_courseId;
    private readonly uint m_periodId;

    public RemoteSingleLeaderboardDataSource(uint gameboardId)
    {
        m_gameboardId = gameboardId;
    }

    public RemoteSingleLeaderboardDataSource(uint courseId, uint periodId)
    {
        m_courseId = courseId;
        m_periodId = periodId;
    }

    public void Retrieve(Action<SingleLeaderboardResponse> callback)
    {
        if (m_callback != null)
        {
            throw new InvalidOperationException();
        }

        m_callback = callback;
        var request = new CMD_Get_Ranklist_r_Parameters();
        request.GbId = m_gameboardId;
        request.CourseId = m_courseId;
        request.PeriodId = m_periodId;
        SocketManager.instance.send(Command_ID.CmdGetRanklistR, request.ToByteString(), OnGetRankList);
    }

    private void OnGetRankList(Command_Result res, ByteString data)
    {
        var result = new SingleLeaderboardResponse();
        if (res == Command_Result.CmdNoError)
        {
            var response = CMD_Get_Ranklist_a_Parameters.Parser.ParseFrom(data);
            result.ranks = response.RankList.Select(x => ToLeaderboardRank(x));
            if (response.SelfInfo != null)
            {
                result.myRank = ToLeaderboardRank(response.SelfInfo);
            }
        }
        else
        {
            Debug.LogError(res);
            result.ranks = new LeaderboardRecord[0];
        }
        m_callback(result);
        m_callback = null;
    }

    private static LeaderboardRecord ToLeaderboardRank(Rank_Unit unit)
    {
        return new LeaderboardRecord {
            rank = (int)unit.RankId,
            userId = unit.UserDisplayInfo.UserId,
            iconId = (int)unit.UserDisplayInfo.UserInconId,
            username = unit.UserDisplayInfo.UserNickname,
            score = unit.RankScore
        };
    }
}
