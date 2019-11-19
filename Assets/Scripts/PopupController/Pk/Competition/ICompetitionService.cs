using System;
using System.Collections.Generic;

public class CompetitionLeaderboardResult
{
    public IEnumerable<CompetitionLeaderboardRecord> ranks;
    public CompetitionLeaderboardRecord myRank;
}

public class CompetitionUpdate
{
    public string name;
    public DateTime startTime;
    public DateTime endTime;
    public string coverUrl;
    public CourseTrophySetting trophySetting;

    public void Update(Competition competition)
    {
        competition.name = name;
        competition.startTime = startTime;
        competition.duration = endTime - startTime;
        if (coverUrl != null)
        {
            competition.coverUrl = coverUrl;
        }
        if (trophySetting != null)
        {
            competition.courseTrophySetting = trophySetting;
        }
    }

    public void Validate()
    {
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentException("name");
        }
        if (startTime >= endTime)
        {
            throw new ArgumentException("startTime must less than endTime");
        }
    }
}

public class CompetitionProblemUpdate
{
    public string name;
    public string description;

    public void Update(CompetitionProblem problem)
    {
        problem.name = name;
        problem.description = description;
    }

    public void Validate()
    {
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentException("name");
        }
        //if (string.IsNullOrEmpty(description))
        //{
        //    throw new ArgumentException("description");
        //}
    }
}

public class CompetitionProblemAnswerInfo
{
    public string path;
    public string answerName;
    public uint userId;
    public string userNickname;
    public int score;

    public void Validate()
    {
        if (string.IsNullOrEmpty(answerName))
        {
            throw new ArgumentException("answerName");
        }
    }

    public void Update(CompetitionProblem problem, string codePath)
    {
        var answer = problem.GetAnswer(userId);
        if (answer == null)
        {
            problem.AddAnswer(new CompetitionProblemAnswer(userId, userNickname, score, codePath));
        }
        else
        {
            answer.score = score;
        }
    }
}

public interface ICompetitionService
{
    void RetrieveCompetitions(CompetitionCategory category, Action<IEnumerable<Competition>> callback);

    /// <summary>
    /// create a competition, on success, the id and creator id will be updated
    /// </summary>
    void CreateCompetition(Competition competition, Action<Command_Result> callback);

    /// <summary>
    /// update the basic info of a competition
    /// </summary>
    void UpdateCompetition(Competition competition, CompetitionUpdate update, Action<Command_Result> callback);

    void DeleteCompetition(uint competitionId, Action<Command_Result> callback);

    void PublishCompetition(Competition competition, Action<Command_Result> callback);

    void TestCompetition(Competition competition, string password, Action<Command_Result> callback);

    /// <summary>
    /// create a problem, on success, problem will be added to the competition
    /// </summary>
    void CreateProblem(Competition competition, CompetitionProblem problem, Action<Command_Result> callback);

    void UpdateProblem(CompetitionProblem problem, CompetitionProblemUpdate update, Action<Command_Result> callback);

    void DeleteProblem(CompetitionProblem problem, Action<Command_Result> callback);

    void AddGameboard(CompetitionProblem problem, CompetitionGameboardItem item, string projectName, Action<Command_Result> callback);

    void UpdateGameboard(CompetitionGameboardItem item, string name, string projectPath, Action<Command_Result> callback);

    void AddItem(CompetitionProblem problem, CompetitionItem item, Action<Command_Result> callback);

    void DeleteItem(CompetitionItem item, Action<Command_Result> callback);

    void GetLeaderboard(uint competitionId, Action<CompetitionLeaderboardResult> callback);

    void UploadAnswer(CompetitionProblem problem, CompetitionProblemAnswerInfo info, Action<Command_Result> callback);

    void EndCompetition(uint competitonId, Action<Command_Result> callback);

    void DeleteTrophySettings(uint competitionId, Action<Command_Result> callback);
}
