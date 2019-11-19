using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class RemoteCompetitionService : ICompetitionService
{
    public void RetrieveCompetitions(CompetitionCategory category, Action<IEnumerable<Competition>> callback)
    {
        var request = new CMD_Get_Course_List_r_Parameters();
        request.ReqCourseType = Course_type.Race;
        switch (category)
        {
        case CompetitionCategory.Open:
            request.ReqType = GetCourseListType.GetCoursePublished;
            break;
        case CompetitionCategory.Closed:
            request.ReqType = GetCourseListType.GetCourseClosed;
            break;
        case CompetitionCategory.Mine:
            request.ReqType = GetCourseListType.GetCourseMyself;
            break;
        case CompetitionCategory.OpenTest:
            request.ReqType = GetCourseListType.GetCourseTest;
                break;
        case CompetitionCategory.ClosedTest:
                request.ReqType = GetCourseListType.GetCourseTestHistory;
                break;
            default:
            throw new ArgumentException("category");
        }

        SocketManager.instance.send(Command_ID.CmdGetCourseListR, request.ToByteString(), (res, data) => {
            var competitions = new List<Competition>();
            if (res == Command_Result.CmdNoError)
            {
                var response = CMD_Get_Course_List_a_Parameters.Parser.ParseFrom(data);
                competitions.AddRange(response.CouseList.Select(x => {
                    var comp = x.ToCompetition();
                    comp.category = category;
                    return comp;
                }));
            }
            callback(competitions);
        });
    }

    public void CreateCompetition(Competition competition, Action<Command_Result> callback)
    {
        if (competition == null)
        {
            throw new ArgumentNullException("competition");
        }

        var request = new CMD_Create_Course_r_Parameters();

        var courseInfo = new Course_Info();
        courseInfo.CourseId = competition.id;
        courseInfo.CourseName = competition.name;
        courseInfo.CourseStartTime = (ulong)TimeUtils.ToEpochSeconds(competition.startTime);
        courseInfo.CourseEndTime = (ulong)TimeUtils.ToEpochSeconds(competition.endTime);
        courseInfo.CourseType = Course_type.Race;
        courseInfo.CourseCoverImageUrl = competition.coverUrl;
        PackHonor(courseInfo, competition);

        // Commit for scratch
        courseInfo.CourseStatus = Course_Status.Commit;
        request.CreateInfo = courseInfo;

        SocketManager.instance.send(Command_ID.CmdCreateCourseR, request.ToByteString(), (res, data) => {
            if (res == Command_Result.CmdNoError)
            {
                var response = CMD_Create_Course_a_Parameters.Parser.ParseFrom(data);
                competition.id = response.CourseInfo.CourseId;
                competition.category = CompetitionCategory.Mine;
            }
            callback(res);
        });
    }

    void PackHonor(Course_Info courseInfo, Competition competition, CompetitionUpdate update = null)
    {
        var trophySetting = (update != null ? update.trophySetting : null) ?? competition.courseTrophySetting;
        if (trophySetting != null)
        {
            string name = competition.name;
            if (update != null)
            {
                name = update.name;
            }
            courseInfo.CourseHonorSetting = CreateHonorSetting(trophySetting, name);
        }
    }

    private Course_Honor_Setting CreateHonorSetting(CourseTrophySetting trophySetting, string name)
    {
        trophySetting.goldTrophy.courseName = name;
        trophySetting.silverTrophy.courseName = name;
        trophySetting.bronzeTrophy.courseName = name;

        var honorSetting = new Course_Honor_Setting();
        honorSetting.CourseId = trophySetting.courseId;
        honorSetting.CourseRaceType = trophySetting.courseRaceType;
        Trophies_Setting trophiesSetting = new Trophies_Setting();
        trophiesSetting.CourseGoldTrophySetting = trophySetting.goldTrophy.PackPb();
        trophiesSetting.CourseSiliverTrophySetting = trophySetting.silverTrophy.PackPb();
        trophiesSetting.CourseBronzeTrophySetting = trophySetting.bronzeTrophy.PackPb();
        honorSetting.CourseTrophiesSetting = trophiesSetting;

        return honorSetting;
    }

    public void UpdateCompetition(Competition competition, CompetitionUpdate update, Action<Command_Result> callback)
    {
        if (competition == null)
        {
            throw new ArgumentNullException("competition");
        }
        if (update == null)
        {
            throw new ArgumentNullException("update");
        }
        update.Validate();

        var request = new CMD_Modify_Course_r_Parameters();

        var courseInfo = new Course_Info();
        courseInfo.CourseId = competition.id;
        courseInfo.CourseName = update.name;
        courseInfo.CourseStartTime = (ulong)TimeUtils.ToEpochSeconds(update.startTime);
        courseInfo.CourseEndTime = (ulong)TimeUtils.ToEpochSeconds(update.endTime);
        PackHonor(courseInfo, competition, update);
        if (update.coverUrl != null)
        {
            courseInfo.CourseCoverImageUrl = update.coverUrl;
        }
        request.ModifyInfo = courseInfo;

        SocketManager.instance.send(Command_ID.CmdModifyCourseR, request.ToByteString(), (res, data) => {
            if (res == Command_Result.CmdNoError)
            {
                update.Update(competition);
            }
            callback(res);
        });
    }

    public void DeleteCompetition(uint competitionId, Action<Command_Result> callback)
    {
        var request = new CMD_Del_Course_r_Parameters();
        request.CourseId = competitionId;

        SocketManager.instance.send(Command_ID.CmdDelCourseR, request.ToByteString(), (res, data) => {
            callback(res);
        });
    }

    public void PublishCompetition(Competition competition, Action<Command_Result> callback)
    {
        if (competition == null)
        {
            throw new ArgumentNullException();
        }

        var request = new CMD_Set_Course_Status_r_Parameters();
        request.CourseId = competition.id;
        request.CourseStatus = Course_Status.WaitforPublish;

        SocketManager.instance.send(Command_ID.CmdSetCourseStatusR, request.ToByteString(), (res, data) => {
            callback(res);
        });
    }

    public void TestCompetition(Competition competition, string password, Action<Command_Result> callback)
    {
        if (competition == null)
        {
            throw new ArgumentNullException();
        }

        var request = new CMD_Set_Course_Status_r_Parameters();
        request.CourseId = competition.id;
        request.CoursePassword = password;
        request.CourseStatus = Course_Status.Test;

        SocketManager.instance.send(Command_ID.CmdSetCourseStatusR, request.ToByteString(), (res, data) => {
            callback(res);
        });
    }

    public void CreateProblem(Competition competition, CompetitionProblem problem, Action<Command_Result> callback)
    {
        if (competition == null)
        {
            throw new ArgumentNullException("competition");
        }
        if (problem == null)
        {
            throw new ArgumentNullException("problem");
        }

        var request = new CMD_Add_Period_r_Parameters();
        request.CourseId = competition.id;
        request.PeriodInfo = new Period_Info();
        request.PeriodInfo.PeriodName = problem.name;
        request.PeriodInfo.PeriodDescription = problem.description;
        request.PeriodInfo.PeriodType = problem.periodType;

        SocketManager.instance.send(Command_ID.CmdAddPeriodR, request.ToByteString(), (res, data) => {
            if (res == Command_Result.CmdNoError)
            {
                var response = CMD_Add_Period_a_Parameters.Parser.ParseFrom(data);
                problem.id = response.PeriodInfo.PeriodId;
                competition.AddProblem(problem);
            }
            callback(res);
        });
    }

    public void UpdateProblem(CompetitionProblem problem, CompetitionProblemUpdate update, Action<Command_Result> callback)
    {
        if (problem == null)
        {
            throw new ArgumentNullException("problem");
        }
        if (update == null)
        {
            throw new ArgumentNullException("update");
        }
        update.Validate();

        var request = new CMD_Modify_Period_r_Parameters();
        request.CourseId = problem.competition.id;
        request.PeriodInfo = new Period_Info();
        request.PeriodInfo.PeriodId = problem.id;
        request.PeriodInfo.PeriodName = update.name;
        //request.PeriodInfo.PeriodDescription = update.description;
        request.PeriodInfo.PeriodType = problem.periodType;

        SocketManager.instance.send(Command_ID.CmdModifyPeriodR, request.ToByteString(), (res, data) => {
            if (res == Command_Result.CmdNoError)
            {
                update.Update(problem);
            }
            callback(res);
        });
    }

    public void DeleteProblem(CompetitionProblem problem, Action<Command_Result> callback)
    {
        if (problem == null)
        {
            throw new ArgumentNullException("problem");
        }

        var request = new CMD_Del_Period_r_Parameters();
        request.CourseId = problem.competition.id;
        request.PeriodId = problem.id;

        SocketManager.instance.send(Command_ID.CmdDelPeriodR, request.ToByteString(), (res, data) => {
            if (res == Command_Result.CmdNoError)
            {
                problem.competition.RemoveProblem(problem);
            }
            callback(res);
        });
    }

    public void AddGameboard(CompetitionProblem problem, CompetitionGameboardItem item, string projectPath, Action<Command_Result> callback)
    {
        if (problem == null)
        {
            throw new ArgumentNullException("problem");
        }
        if (item == null)
        {
            throw new ArgumentNullException("item");
        }
        var project = GameboardRepository.instance.loadGameboardProject(projectPath);
        if (project == null)
        {
            throw new IOException();
        }

        item.sceneId = project.gameboard.themeId;

        var request = new CMD_Add_Perioditem_r_Parameters();
        request.CourseId = problem.competition.id;
        request.PeriodId = problem.id;
        request.PeriodItemInfo = CreateGBItemInfo(item, item.sceneId, item.name);

        project.gameboard.sourceCodeAvailable = true;
        project.gameboard.ClearCodeGroups();

        request.PeriodFiles = new FileList();
        request.PeriodFiles.FileList_.AddRange(project.ToFileNodeList(""));

        SocketManager.instance.send(Command_ID.CmdAddPerioditemR, request.ToByteString(), (res, data) => {
            if (res == Command_Result.CmdNoError)
            {
                var response = CMD_Add_Perioditem_a_Parameters.Parser.ParseFrom(data);
                item.id = response.PeriodItemInfo.ItemId;
                item.url = response.PeriodItemInfo.GbInfo.ProjPath;
                item.downloadPath = response.PeriodItemInfo.GbInfo.ProjPath;
                problem.gameboardItem = item;
            }
            callback(res);
        });
    }

    public void UpdateGameboard(CompetitionGameboardItem item, string name, string projectPath, Action<Command_Result> callback)
    {
        if (item == null)
        {
            throw new ArgumentNullException();
        }

        var project = GameboardRepository.instance.loadGameboardProject(projectPath);
        if (project == null)
        {
            throw new IOException();
        }

        var request = new CMD_Modify_Perioditem_r_Parameters();
        request.CourseId = item.problem.competition.id;
        request.PeriodId = item.problem.id;
        request.PeriodItemInfo = CreateGBItemInfo(item, project.gameboard.themeId, name);
        request.PeriodItemInfo.ItemName = item.nickName;

        project.gameboard.sourceCodeAvailable = true;
        project.gameboard.ClearCodeGroups();

        request.PeriodFiles = new FileList();
        request.PeriodFiles.FileList_.AddRange(project.ToFileNodeList(""));

        SocketManager.instance.send(Command_ID.CmdModifyPerioditemR, request.ToByteString(), (res, data) => {
            if (res == Command_Result.CmdNoError)
            {
                item.name = name;
                item.sceneId = project.gameboard.themeId;
            }
            callback(res);
        });
    }

    private Period_Item_Info CreateGBItemInfo(CompetitionGameboardItem item, int sceneId, string name)
    {
        var itemInfo = new Period_Item_Info();
        itemInfo.ItemId = item.id;
        itemInfo.eItemType = Period_Item_Type.ItemGb;
        itemInfo.ItemName = name;
        itemInfo.GbInfo = new GameBoard();
        itemInfo.GbInfo.GbSenceId = (uint)sceneId;
        itemInfo.GbInfo.HasSourceCode = false;
        return itemInfo;
    }

    public void AddItem(CompetitionProblem problem, CompetitionItem item, Action<Command_Result> callback)
    {
        if (problem == null)
        {
            throw new ArgumentNullException("problem");
        }
        if (item == null)
        {
            throw new ArgumentNullException("item");
        }
        if (item.type == CompetitionItem.Type.Gb)
        {
            throw new ArgumentException("use AddGameboard");
        }

        var request = new CMD_Add_Perioditem_r_Parameters();
        request.CourseId = problem.competition.id;
        request.PeriodId = problem.id;
        request.PeriodItemInfo = new Period_Item_Info();
        request.PeriodItemInfo.eItemType = (Period_Item_Type)item.type;
        request.PeriodItemInfo.ItemUrl = item.url;
        request.PeriodItemInfo.ItemName = item.nickName;

        SocketManager.instance.send(Command_ID.CmdAddPerioditemR, request.ToByteString(), (res, data) => {
            if (res == Command_Result.CmdNoError)
            {
                var response = CMD_Add_Perioditem_a_Parameters.Parser.ParseFrom(data);
                item.id = response.PeriodItemInfo.ItemId;
                problem.AddAttachment(item);
            }
            callback(res);
        });
    }

    public void DeleteItem(CompetitionItem item, Action<Command_Result> callback)
    {
        if (item == null)
        {
            throw new ArgumentNullException("item");
        }

        var request = new CMD_Del_Perioditem_r_Parameters();
        request.CourseId = item.problem.competition.id;
        request.PeriodId = item.problem.id;
        request.PerioditemId = item.id;

        SocketManager.instance.send(Command_ID.CmdDelPerioditemR, request.ToByteString(), (res, data) => {
            if (res == Command_Result.CmdNoError)
            {
                item.problem.RemoveAttachById(item.id);
            }
            callback(res);
        });
    }


    public void GetLeaderboard(uint competitionId, Action<CompetitionLeaderboardResult> callback)
    {
        var request = new CMD_Get_Ranklist_r_Parameters();
        request.CourseId = competitionId;
        SocketManager.instance.send(Command_ID.CmdGetRanklistR, request.ToByteString(), (res, data) => {
            var result = new CompetitionLeaderboardResult();
            result.ranks = new List<CompetitionLeaderboardRecord>();
            if (res == Command_Result.CmdNoError)
            {
                var response = CMD_Get_Ranklist_a_Parameters.Parser.ParseFrom(data);
                result.ranks = response.RankList.Select(x => ToLeaderboardRank(x));
                if (response.SelfInfo != null)
                {
                    result.myRank = ToLeaderboardRank(response.SelfInfo);
                }
            }
            callback(result);
        });
    }

    private static CompetitionLeaderboardRecord ToLeaderboardRank(Rank_Unit unit)
    {
        return new CompetitionLeaderboardRecord {
            rank = (int)unit.RankId,
            userId = unit.UserDisplayInfo.UserId,
            iconId = (int)unit.UserDisplayInfo.UserInconId,
            username = unit.UserDisplayInfo.UserNickname,
            score = unit.RankScore,
            answeredProblemCount = (int)unit.RankSampleCount
        };
    }

    public void UploadAnswer(CompetitionProblem problem, CompetitionProblemAnswerInfo info, Action<Command_Result> callback)
    {
        if (problem == null)
        {
            throw new ArgumentNullException("problem");
        }
        if (problem.gameboardItem == null)
        {
            throw new InvalidOperationException("no valid gameboard");
        }
        if (info == null)
        {
            throw new ArgumentNullException("info");
        }

        var request = new CMD_Answer_Perioditem_r_Parameters();
        request.CourseId = problem.competition.id;
        request.PeriodId = problem.id;
        request.PerioditemId = problem.gameboardItem.id;
        if (info.path != null) {
            var project = CodeProjectRepository.instance.loadCodeProject(info.path);
            request.PeriodFiles = new FileList();
            request.PeriodFiles.FileList_.AddRange(project.ToFileNodeList(""));
        }
        request.PeriodAnswerInfo = new GBAnswer();
        request.PeriodAnswerInfo.GbScriptShow = (uint)GbScriptShowType.Hide;
        request.PeriodAnswerInfo.AnswerUserId = info.userId;
        if (info.answerName != null) {
            request.PeriodAnswerInfo.AnswerName = info.answerName;
        }
        request.PeriodAnswerInfo.GbScore = info.score;

        SocketManager.instance.send(Command_ID.CmdAnswerPerioditemR, request.ToByteString(), (res, data) => {
            if (res == Command_Result.CmdNoError)
            {
                var response = CMD_Answer_Perioditem_a_Parameters.Parser.ParseFrom(data);
                info.Update(problem, response.PeriodAnswerInfo.ProjPath);
            }
            callback(res);
        });
    }


    public void EndCompetition(uint competitonId, Action<Command_Result> callback)
    {
        var request = new CMD_End_Course_r_Parameters();
        request.CourseId = competitonId;
        SocketManager.instance.send(Command_ID.CmdEndCourseR, request.ToByteString(), (res, content) => {
            callback(res);
        });
    }


    public void DeleteTrophySettings(uint competitionId, Action<Command_Result> callback)
    {
        var request = new CMD_Del_Course_Trophy_Setting_r_Parameters();
        request.CourseId = competitionId;

        SocketManager.instance.send(Command_ID.CmdDelCourseTrophySettingR, request.ToByteString(), (res, content) => {
            callback(res);
        });
    }
}
