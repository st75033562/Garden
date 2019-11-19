using LitJson;
using System;
using System.Linq;
using UnityEngine;
using Google.Protobuf;
using System.Collections.Generic;

/**
 * convert protobuf message to app data model
 */

public partial class CMD
{
    public Command_ID Id { get { return (Command_ID)CmdId; } }

    public Command_Result Result { get { return (Command_Result)CmdResult; } }
}

public partial class Mail
{
    public Mail_Type Type
    {
        get { return (Mail_Type)MailType; }
    }

    /// <summary>
    /// to generic mail info
    /// </summary>
    /// <returns></returns>
    public MailInfo ToMailInfo()
    {
        MailInfo tNewMail = new MailInfo();
        tNewMail.m_MailID = MailId;
        tNewMail.m_SenderID = SenderId;
        if (null == SenderName)
        {
            tNewMail.m_SenderName = "";
        }
        else
        {
            tNewMail.m_SenderName = SenderName.ToStringUtf8();
        }
        tNewMail.m_ReceiverID = ReceiverId;
        if (null == ReceiverName)
        {
            tNewMail.m_ReceiverName = "";
        }
        else
        {
            tNewMail.m_ReceiverName = ReceiverName.ToStringUtf8();
        }
        if (null == Title)
        {
            tNewMail.m_Title = "";
        }
        else
        {
            tNewMail.m_Title = Title.ToStringUtf8();
        }
        if (null == Content)
        {
            tNewMail.m_Content = "";
        }
        else
        {
            tNewMail.m_Content = Content.ToStringUtf8();
        }
        if (null != Attachment)
        {
            tNewMail.m_AttachMent = Attachment.ToByteArray();
        }
        tNewMail.m_ExpiredTime = ExpiredTime;
        tNewMail.m_MailFlag = MailFlag;
        tNewMail.m_CreateTime = CreateTime;
        tNewMail.m_MailType = MailType;
        if (null != Extension)
        {
            tNewMail.m_Extension = Extension.ToByteArray();
        }
        tNewMail.m_IconID = IconId;
        return tNewMail;
    }

    public ClassRequestInfo ToClassRequestInfo()
    {
        if (Type != Mail_Type.MailClassReq)
        {
            throw new InvalidOperationException();
        }

        A8_Class_Info classInfoData = A8_Class_Info.Parser.ParseFrom(Extension);
        ClassRequestInfo requestInfo = new ClassRequestInfo();
        requestInfo.m_ClassID = classInfoData.ClassId;
        requestInfo.m_MailID = MailId;
        requestInfo.m_NickName = SenderName.ToStringUtf8();
        requestInfo.m_IconID = IconId;
        requestInfo.m_UserID = SenderId;

        return requestInfo;
    }

    public KickNotification ToKickNotification()
    {
        if (Type != Mail_Type.MailLeaveClass)
        {
            throw new InvalidOperationException();
        }

        KickNotification notification = new KickNotification();
        A8_Class_Info classInfoData = A8_Class_Info.Parser.ParseFrom(Extension);
        notification.id = MailId;
        notification.classId = classInfoData.ClassId;
        notification.className = classInfoData.ClassName;
        return notification;
    }
}

public partial class FileList {
    public Gameboard.Gameboard GetGameboard() {
        var gameboardData = GetFileData(GameboardRepository.GameBoardFileName);
        if(gameboardData == null) {
            return null;
        }

        try {
            return Gameboard.Gameboard.Parse(gameboardData);
        } catch(InvalidProtocolBufferException) {
            return null;
        }
    }

    public FileNode GetFile(string name) {
        return FileList_.FirstOrDefault(x => x.PathName == name);
    }

    public byte[] GetFileData(string name) {
        var file = GetFile(name);
        return file != null ? file.FileContents.ToByteArray() : null;
    }

    public Project ToProject() {
        if(fileList_ == null) {
            return null;
        }

        Project project = new Project();
        for(int i = 0; i < fileList_.Count; ++i) {
            FileNode tCurFile = fileList_[i];
            string[] splitPath = tCurFile.PathName.Split('/');
            if(CodeProjectRepository.ProjectFileName == splitPath[splitPath.Length -1]) {
                project.code = tCurFile.FileContents.ToByteArray();
            } else if(CodeProjectRepository.LeaveMessageFileName == splitPath[splitPath.Length - 1]) {
                project.leaveMessageData = tCurFile.FileContents.ToByteArray();
            }
        }
        return project;
    }

    public GameboardProject ToGameboardProject() {
        var project = new GameboardProject();
        for(int i = 0; i < fileList_.Count; ++i) {
            FileNode tCurFile = fileList_[i];
            string[] splitPath = tCurFile.PathName.Split('/');
            if(GameboardRepository.ProjectFileName == splitPath[splitPath.Length - 1]) {
                project.code = tCurFile.FileContents.ToByteArray();
            } else if(GameboardRepository.LeaveMessageFileName == splitPath[splitPath.Length - 1]) {
                project.leaveMessageData = tCurFile.FileContents.ToByteArray();
            } else if(GameboardRepository.GameBoardFileName == splitPath[splitPath.Length - 1]) {
                try {
                    project.gameboard = Gameboard.Gameboard.Parse(tCurFile.FileContents.ToByteArray());
                } catch(InvalidProtocolBufferException) {
                    return null;
                }
            }
        }
        return project.gameboard != null ? project : null;
    }


}
public enum GbScriptShowType {
    Invalid,
    Hide,
    Show
}

public partial class GameBoard
{
    public bool HasSourceCode
    {
        get { return (GbScriptShowType)GbScriptShow == GbScriptShowType.Show; }
        set
        {
            GbScriptShow = (uint)(value ? GbScriptShowType.Show : GbScriptShowType.Hide);
        }
    }

    public DateTime CreationTime
    {
        get { return TimeUtils.FromEpochSeconds((long)GbCreateTime); }
    }

    public bool Liked(uint userId)
    {
        return GbLikeList.ContainsKey(userId);
    }

    public void SetLiked(uint userId, bool liked)
    {
        if (liked)
        {
            GbLikeList[userId] = new GBLike_Info();
        }
        else
        {
            GbLikeList.Remove(userId);
        }
    }
}

public class PkParameter
{
    // TODO: remap the name
    public string jsPointInfo;
    public string jsGBName;
    public int jsPassMode;

    public PkParameter() { }

    public PkParameter(string startPointInfo, string gbName, int passMode)
    {
        jsPointInfo = startPointInfo;
        jsGBName = gbName;
        jsPassMode = passMode;
    }

    public override string ToString()
    {
        return JsonMapper.ToJson(this);
    }

    public static PkParameter Parse(string data)
    {
        return string.IsNullOrEmpty(data) ? new PkParameter() : JsonMapper.ToObject<PkParameter>(data);
    }
}

public partial class PK
{
    public event Action onUpdated;
    public event Action<PKAnswer> onAnswerAdded;

    private PkParameter m_pkParameter;

    // readonly, do not modify directly
    public PkParameter Parameter
    {
        get
        {
            if (m_pkParameter == null)
            {
                m_pkParameter = PkParameter.Parse(PkParameters);
            }
            return m_pkParameter;
        }
        set
        {
            m_pkParameter = value;
            PkParameters = value.ToString();
        }
    }

    public bool ContainsUserAnswer(uint userId)
    {
        return PkAnswerList.Values.Any(x => x.AnswerUserId == userId);
    }

    public bool AllowRepeatAnswer
    {
        get { return PkAllowRepeatAnswer == 0; }
        set { PkAllowRepeatAnswer = value ? 0u : 1u; }
    }

    public bool CanUserUploadAnswer(uint userId)
    {
        return AllowRepeatAnswer || !ContainsUserAnswer(userId);
    }

    public void AddAnswer(PKAnswer answer)
    {
        PkAnswerList.Add(answer.AnswerId, answer);
        if (onAnswerAdded != null)
        {
            onAnswerAdded(answer);
        }
        NotfiyUpdated();
    }

    public IEnumerable<PKAnswer> GetUserAnswers(uint userId)
    {
        return PkAnswerList.Values.Where(x => x.AnswerUserId == userId);
    }

    public PKAnswer GetAnswer(uint answerId)
    {
        PKAnswer answer;
        PkAnswerList.TryGetValue(answerId, out answer);
        return answer;
    }

    public void NotfiyUpdated()
    {
        if (onUpdated != null)
        {
            onUpdated();
        }
    }

    public bool Liked(uint userId)
    {
        return PkLikeList.ContainsKey(userId);
    }

    public DateTime CreationTime
    {
        get { return TimeUtils.FromEpochSeconds((long)PkCreateTime); }
    }
}

public partial class GameBoard
{
    // for UI update
    public Action onUpdated;

    private PkParameter m_pkParameter;

    public void SetDownloadCount(uint count)
    {
        GbDownloadCount = count;
        if (onUpdated != null)
        {
            onUpdated();
        }
    }

    public void SetAnswer(uint answerId, GBAnswer answer)
    {
        GbAnswerList[answerId] = answer;
        if (onUpdated != null)
        {
            onUpdated();
        }
    }

    // readonly, do not modify directly
    public PkParameter Parameter
    {
        get
        {
            if (m_pkParameter == null)
            {
                m_pkParameter = PkParameter.Parse(GbParameters);
            }
            return m_pkParameter;
        }
        set
        {
            m_pkParameter = value;
            GbParameters = value.ToString();
        }
    }

    public bool ContainsUserAnswer(uint userId)
    {
        return GbAnswerList.Values.Any(x => x.AnswerUserId == userId);
    }

    public bool CanUserUploadAnswer(uint userId)
    {
        return GbAllowRepeatAnswer || !ContainsUserAnswer(userId);
    }

    public GBAnswer GetUserAnswer(uint userId)
    {
        return GbAnswerList.Values.FirstOrDefault(x => x.AnswerUserId == userId);
    }
}

public partial class Period_Item_Info
{
    public Period_Item_Type eItemType
    {
        get { return (Period_Item_Type)ItemType; }
        set { ItemType = (uint)value; }
    }

    public CompetitionItem ToCompetitionItem()
    {
        if (eItemType == Period_Item_Type.ItemGb)
        {
            var item = new CompetitionGameboardItem();
            item.id = ItemId;
            item.name = ItemName;
            item.nickName = ItemName;
            item.url = GbInfo.ProjPath;
            item.sceneId = (int)GbInfo.GbSenceId;
            item.downloadPath = GbInfo.ProjPath;
            return item;
        }
        else if (eItemType != Period_Item_Type.ItemGb)
        {
            var item = new CompetitionItem((CompetitionItem.Type)eItemType);
            item.id = ItemId;
            item.url = ItemUrl;
            item.nickName = ItemName;
            return item;
        }
        else
        {
            return null;
        }
    }
}

public partial class Period_Info
{

    public PeriodOperation periodOperation;
    public Period_Item_Info GetGameboard()
    {
        return PeriodItems.Values.FirstOrDefault(x => x.eItemType == Period_Item_Type.ItemGb);
    }

    public Period_Item_Info GetItem(uint itemId)
    {
        Period_Item_Info item;
        PeriodItems.TryGetValue(itemId, out item);
        return item;
    }

    public IEnumerable<Period_Item_Info> DisplayItems
    {
        get
        {
            foreach (var id in PeriodItemDisplayList)
            {
                yield return PeriodItems[id];
            }
        }
    }

    public IEnumerable<Period_Item_Info> GetDisplayItems(Period_Item_Type type)
    {
        return DisplayItems.Where(x => x.eItemType == type);
    }

    public CompetitionProblem ToCompetitionProblem()
    {
        var problem = new CompetitionProblem();
        problem.id = PeriodId;
        problem.periodType = PeriodType;
        problem.name = PeriodName;
        problem.description = PeriodDescription;
        foreach (var item in DisplayItems)
        {
            var competitionItem = item.ToCompetitionItem();
            if (competitionItem.type == CompetitionItem.Type.Gb)
            {
                problem.gameboardItem = (CompetitionGameboardItem)competitionItem;
                problem.AddAnswers(item.GbInfo.GbAnswerList.Values.Select(x => {
                    return new CompetitionProblemAnswer(x.AnswerUserId, x.AnswerNickname, x.GbScore, x.ProjPath);
                }));
            }
            else
            {
                problem.AddAttachment(competitionItem);
            }
        }
        return problem;
    }
}

public static class FileDataExtension
{
    public static IEnumerable<FileNode> ToFileNodeList(this IEnumerable<FileData> files, string path)
    {
        return files.Select(x => new FileNode {
            PathName = path + x.filename,
            FileContents = ByteString.CopyFrom(x.data),
            FnType = (uint)FN_TYPE.FnFile
        });
    }
}


public partial class Course_Info
{
    public DateTime StartTime
    {
        get { return TimeUtils.FromEpochSeconds((long)CourseStartTime); }
    }

    public DateTime EndTime
    {
        get { return TimeUtils.FromEpochSeconds((long)CourseEndTime); }
    }

    public Competition ToCompetition()
    {
        var competition = new Competition();
        competition.id = CourseId;
        competition.creatorId = CourseCreaterUserid;
        competition.name = CourseName;
        competition.startTime = StartTime;
        competition.duration = EndTime - StartTime;
        competition.isScratch = CourseStatus == Course_Status.Commit;
        competition.coverUrl = CourseCoverImageUrl;
        foreach (var period in PeriodList.Values.OrderBy(x => x.PeriodId))
        {
            competition.AddProblem(period.ToCompetitionProblem());
        }
        if (CourseHonorSetting != null && CourseHonorSetting.CourseTrophiesSetting != null)
        {
            competition.courseTrophySetting = CourseTrophySetting.Parse(CourseHonorSetting);
        }
        return competition;
    }
}

public partial class GBAnswer
{
    public RobotCodeInfo ToRobotCodeInfo()
    {
        return RobotCodeInfo.Remote(ProjPath, AnswerNickname);
    }

    public bool Liked(uint userId)
    {
        return GbLikeList.ContainsKey(userId);
    }

    public void SetLiked(uint userId, bool liked)
    {
        if (liked)
        {
            GbLikeList[userId] = new GBLike_Info();
        }
        else
        {
            GbLikeList.Remove(userId);
        }
    }
}

public partial class PKAnswer
{
    public event Action<PK_Result> onPKResultAdded;

    public RobotCodeInfo ToRobotCodeInfo()
    {
        return RobotCodeInfo.Remote(ProjPath, AnswerNickname);
    }

    public bool Liked(uint userId)
    {
        return PkLikeList.ContainsKey(userId);
    }

    public DateTime CreationTime
    {
        get { return TimeUtils.FromEpochSeconds((long)AnswerTime); }
    }

    public void AddPKResult(PK_Result result)
    {
        if (result.AccepterId == AnswerId)
        {
            if (result.AccepterScore > result.ChanllengerScore)
            {
                ++PkWinCount;
            }
            else if (result.AccepterScore < result.ChanllengerScore)
            {
                ++PkLostCount;
            }
        }
        else if (result.ChanllengerAnswerId == AnswerId)
        {
            if (result.AccepterScore > result.ChanllengerScore)
            {
                ++PkLostCount;
            }
            else if (result.AccepterScore < result.ChanllengerScore)
            {
                ++PkWinCount;
            }
        }
        else
        {
            throw new ArgumentException("Invalid result");
        }

        PkResultList.Add(result);
        if (onPKResultAdded != null)
        {
            onPKResultAdded(result);
        }
    }
}

public partial class PK_Result
{
    public enum Outcome
    {
        Win,
        Lose,
        Draw
    }

    public Outcome ChallengerOutcome
    {
        get
        {
            if (ChanllengerScore > AccepterScore)
            {
                return Outcome.Win;
            }
            else if (ChanllengerScore < AccepterScore)
            {
                return Outcome.Lose;
            }
            else
            {
                return Outcome.Draw;
            }
        }
    }

    public Outcome AcceptorOutcome
    {
        get
        {
            if (ChanllengerScore > AccepterScore)
            {
                return Outcome.Lose;
            }
            else if (ChanllengerScore < AccepterScore)
            {
                return Outcome.Win;
            }
            else
            {
                return Outcome.Draw;
            }
        }
    }
}
