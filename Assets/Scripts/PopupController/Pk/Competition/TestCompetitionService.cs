using System;
using System.Collections.Generic;

public class TestCompetitionService : ICompetitionService
{
    private readonly List<Competition>[] m_competitions = new List<Competition>[(int)CompetitionCategory.Num];
    private uint m_nextCompetitionId = 1;
    private uint m_nextProblemId = 1;
    private uint m_nextItemId = 1;

    public void RetrieveCompetitions(CompetitionCategory category, Action<IEnumerable<Competition>> callback)
    {
        if (m_competitions[(int)category] == null)
        {
            var competitions = m_competitions[(int)category] = new List<Competition>();
            for (int i = 0; i < 10; ++i)
            {
                var comp = new Competition {
                    id = m_nextCompetitionId++,
                    name = category + " test " + i,
                    category = category,
                    startTime = DateTime.UtcNow,
                    duration = TimeSpan.FromDays(1)
                };
                competitions.Add(comp);

                for (int j = 0; j < i; ++j)
                {
                    var problem = new CompetitionProblem();
                    problem.name = comp.name + " " + j;
                    problem.description = "test " + j;
                    comp.AddProblem(problem);
                }
            }
        }
        callback(m_competitions[(int)category]);
    }

    public void CreateCompetition(Competition competition, Action<Command_Result> callback)
    {
        competition.id = m_nextCompetitionId++;
        callback(Command_Result.CmdNoError);
    }

    public void UpdateCompetition(Competition competition, CompetitionUpdate update, Action<Command_Result> callback)
    {
        update.Validate();
        update.Update(competition);
        callback(Command_Result.CmdNoError);
    }

    public void DeleteCompetition(uint competitionId, Action<Command_Result> callback)
    {
        callback(Command_Result.CmdNoError);
    }

    public void PublishCompetition(Competition competition, Action<Command_Result> callback)
    {
        throw new NotImplementedException();
    }

    public void CreateProblem(Competition competition, CompetitionProblem problem, Action<Command_Result> callback)
    {
        problem.id = m_nextProblemId++;
        competition.AddProblem(problem);
        callback(Command_Result.CmdNoError);
    }

    public void UpdateProblem(CompetitionProblem problem, CompetitionProblemUpdate update, Action<Command_Result> callback)
    {
        update.Validate();
        update.Update(problem);
        callback(Command_Result.CmdNoError);
    }

    public void DeleteProblem(CompetitionProblem problem, Action<Command_Result> callback)
    {
        problem.competition.RemoveProblem(problem);
        callback(Command_Result.CmdNoError);
    }

    public void AddGameboard(CompetitionProblem problem, CompetitionGameboardItem item, string projectName, Action<Command_Result> callback)
    {
        item.id = m_nextItemId++;
        problem.gameboardItem = item;
        callback(Command_Result.CmdNoError);
    }

    public void UpdateGameboard(CompetitionGameboardItem item, string name, string projectPath, Action<Command_Result> callback)
    {
        throw new NotImplementedException();
    }

    public void AddItem(CompetitionProblem problem, CompetitionItem item, Action<Command_Result> callback)
    {
        item.id = m_nextItemId++;
        problem.AddAttachment(item);
        callback(Command_Result.CmdNoError);
    }

    public void DeleteItem(CompetitionItem item, Action<Command_Result> callback)
    {
        item.problem.RemoveAttachment(item);
        callback(Command_Result.CmdNoError);
    }

    public void GetLeaderboard(uint competitionId, Action<CompetitionLeaderboardResult> callback)
    {
        var result = new CompetitionLeaderboardResult();
        var ranks = new List<CompetitionLeaderboardRecord>();
        result.ranks = ranks;

        const int Count = 100;
        int myRank = UnityEngine.Random.Range(0, Count);
        for (int i = 0; i < Count; ++i)
        {
            var rank = new CompetitionLeaderboardRecord {
                rank = i,
                userId = myRank == i ? UserManager.Instance.UserId : (uint)i,
                username = "test " + i,
                score = Count - i,
                answeredProblemCount = Count - i,
            };

            ranks.Add(rank);
            if (i == myRank)
            {
                result.myRank = rank;
            }
        }
        callback(result);
    }


    public void UploadAnswer(CompetitionProblem problem, CompetitionProblemAnswerInfo info, Action<Command_Result> callback)
    {
        info.Update(problem, null);
        callback(Command_Result.CmdNoError);
    }

    public void EndCompetition(uint competitonId, Action<Command_Result> callback)
    {
        throw new NotImplementedException();
    }

    public void DeleteTrophySettings(uint competitionId, Action<Command_Result> callback)
    {
        throw new NotImplementedException();
    }

    public void TestCompetition(Competition competition, string password, Action<Command_Result> callback)
    {
        throw new NotImplementedException();
    }
}
