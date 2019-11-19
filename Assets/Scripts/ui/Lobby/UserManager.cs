using System.Collections;
using System.Collections.Generic;
using System;

using g_WebRequestManager = Singleton<WebRequestManager>;
using System.Linq;

public class MemberInfo
{
	public uint userId;
	public string nickName;
	public uint iconId;

	public void parseUser(A8_User_Display_Info displayInfo)
	{
		userId = displayInfo.UserId;
		nickName = displayInfo.UserNickname;
		iconId = displayInfo.UserInconId;
	}
}

public class MailInfo
{
	public ulong m_MailID;
	public uint m_SenderID;
	public string m_SenderName;
	public uint m_ReceiverID;
	public string m_ReceiverName;
	public string m_Title;
	public string m_Content;
	public byte[] m_AttachMent;
	public uint m_ExpiredTime;
	public uint m_MailFlag;
	public ulong m_CreateTime;
	public uint m_MailType;
	public byte[] m_Extension;
	public uint m_IconID;
}

public class AttachUnit {
    public K8_Attach_Type attachType;
    public string url;
    public string attachName;
    public FileList fileList;
    public string clientData;

    public void SetValue(K8_Attach_Unit unit) {
        attachType = unit.AttachType;
        url = unit.AttachUrl;
        attachName = unit.AttachName;
        fileList = unit.AttachFiles;
        clientData = unit.ClientData;
    }
}

public class ClassStuAttach {
    public uint id;
    public AttachUnit attachUnit;
    public void SetValue(uint id ,AttachUnit attachUnit) {
        this.id = id;
        this.attachUnit = attachUnit;
    }
}

public class TaskSubmitInfo
{
	public long m_Time;
	public uint m_ID;
	public uint m_CommentID;
	public string m_ProgramName;
	public int m_Grade;
    public List<ClassStuAttach> attachUnits;

    public void SetValue(A8_Student_Submit_Task info)
	{
        attachUnits = new List<ClassStuAttach>();
        m_Time = (long)info.SubmittedTime;
		m_ID = m_CommentID = info.SubmitId;
		m_ProgramName = info.ProjectName;

        if(info.SubmitAttachInfo != null) {
            foreach(uint key in info.SubmitAttachInfo.AttachList.Keys) {
                var classAttachUnit = new ClassStuAttach();
                var attachUnit = new AttachUnit();
                attachUnit.SetValue(info.SubmitAttachInfo.AttachList[key]);
                classAttachUnit.SetValue(key, attachUnit);
                attachUnits.Add(classAttachUnit);
            }
        }

        if(string.IsNullOrEmpty(info.TaskGrade))
		{
			m_Grade = 0;
        }
		else
		{
			m_Grade = int.Parse(info.TaskGrade);
		}
	}
}

public class TaskInfo
{
	public long m_CreateTime;
	public uint m_ID;
	public string m_Name;
	public string m_Description;
	public string m_ProgramName;
    public long m_updateTime;
    public bool prohibitSubmit;

    List<TaskSubmitInfo> m_SubmitList = new List<TaskSubmitInfo>();

    public K8_Attach_Info attachs;
    public List<TaskSubmitInfo> SubmitList
	{
		get
		{
			return m_SubmitList;
		}
	}

	public void SetValue(A8_Task_Info info)
	{
		m_CreateTime = (long)info.TaskCreateTime;
		m_ID = info.TaskId;
		m_Name = info.TaskName;
		m_Description = info.TaskDescription;
		m_ProgramName = info.TaskProgramName;
        m_updateTime = (long)info.TaskUpdateTime;
        prohibitSubmit = info.TaskNotAllowSubmit;
        UpdateSubmit(info.TaskSubmitInfo);

        attachs = info.TaskAttachInfoNew;       
    }

	public void UpdateSubmit(A8_Task_Submit_Info info)
	{
		if (null != info)
		{
			m_SubmitList.Clear();
			foreach (var sub in info.SubmittedTask)
			{
				TaskSubmitInfo tNewSub = new TaskSubmitInfo();
				tNewSub.SetValue(sub.Value);
				m_SubmitList.Add(tNewSub);
			}
		}
	}

    public TaskSubmitInfo GetSubmit(uint id)
    {
        return m_SubmitList.Find(x => x.m_ID == id);
    }
}

public class ClassInfo
{
	public enum Status
	{
		Default_Status = 0,
		Attend_Status = 1 << 0,
		Create_Status = 1 << 2,
		Applied_Status = 1 << 3,
	}

	public uint m_ID;
	public Status m_ClassStatus;
	public string m_Name;
	public string m_Description;
	public uint m_IconID;
    public DateTime m_createTime ;
    public uint teacherId;
	public MemberInfo teacherInfo;
    public ScriptLanguage languageType;
    public List<MemberInfo> studentsInfos = new List<MemberInfo>();
	List<TaskInfo> m_Task = new List<TaskInfo>();

	public void Reset()
	{
		m_ID = 0;
		m_ClassStatus = Status.Default_Status;
		m_Name = "";
		m_Description = "";
		m_IconID = 0;
    }

	public void UpdateInfo(A8_Class_Info info)
	{
        m_ID = info.ClassId;
        m_Name = info.ClassName;
		m_Description = info.ClassDescription;
		m_IconID = info.ClassInconId;
		teacherId = info.TeacherId;
        languageType = (ScriptLanguage)info.ClassProjectType;
        m_createTime = TimeUtils.FromEpochSeconds((long)info.ClassCreateTime);
        studentsInfos.Clear();
		foreach (A8_User_Display_Info displayInfo in info.MemberInfo.MemberList.Values)
		{
			MemberInfo user = new MemberInfo();
			user.parseUser(displayInfo);
			if (user.userId == teacherId)
			{
				teacherInfo = user;
			}
			else
			{
				studentsInfos.Add(user);
			}
		}
		m_Task.Clear();
    }

	public void DeleteMember(uint memberId)
	{
        int index = studentsInfos.FindIndex(x => x.userId == memberId);
        if (index != -1)
        {
            studentsInfos.RemoveAt(index);
        }
	}

	public void AddMemebr(A8_User_Info newMember)
	{
		MemberInfo tNew = new MemberInfo();
		tNew.userId = newMember.UserId;
		tNew.iconId = newMember.UserInconId;
		tNew.nickName = newMember.UserNickname;
		studentsInfos.Add(tNew);
	}

	public MemberInfo GetMember(uint memberID)
	{
        return studentsInfos.Find(x => x.userId == memberID);
	}

    public void SetTasks(IEnumerable<TaskInfo> tasks)
    {
        if (tasks == null)
        {
            throw new ArgumentNullException("tasks");
        }
        m_Task.Clear();
        m_Task.AddRange(tasks);
        SortTasks();
    }

    public TaskInfo AddTask(A8_Task_Info info)
	{
		TaskInfo tNew = new TaskInfo();
		tNew.SetValue(info);
        AddTask(tNew);
        return tNew;
    }

    public void AddTask(TaskInfo task)
    {
        if (task == null)
        {
            throw new ArgumentNullException("task");
        }

        m_Task.Add(task);
        SortTasks();
    }

	public void DeleteTask(uint id)
	{
        int index = m_Task.FindIndex(x => x.m_ID == id);
        if (index != -1)
        {
            m_Task.RemoveAt(index);
        }
	}

	public TaskInfo GetTask(uint id)
	{
        return m_Task.Find(x => x.m_ID == id);
	}

    public TaskInfo GetTask(string name)
    {
        return m_Task.Find(x => x.m_Name == name);
    }

	public List<TaskInfo> TaskList
	{
		get { return m_Task; }
	}

	void SortTasks()
	{
		m_Task.Sort((ite1, ite2) => { return ite2.m_updateTime.CompareTo(ite1.m_updateTime); });
	}
}

public class ClassRequestInfo
{
	public ulong m_MailID;
	public uint m_ClassID;
	public uint m_UserID;
	public string m_NickName;
	public uint m_IconID;
}

public class KickNotification
{
    public ulong id;
    public uint classId;
    public string className;
}

public enum AppRunModel {
    Normal,
    Guide,
    NoviceBoot
}
public class UserManager {
    User_Type m_UserPermissions;

    List<ClassInfo> m_Class = new List<ClassInfo>();
    List<MailInfo> m_Mail = new List<MailInfo>();
    Dictionary<uint, List<ClassRequestInfo>> m_ClassRequestList = new Dictionary<uint, List<ClassRequestInfo>>();
    Dictionary<ulong, uint> m_MailAndClass = new Dictionary<ulong, uint>();

    public event Action<KickNotification> onNewKickNotification;
    public event Action<int> onCoinChange;
    public event Action onAvatarIdChanged;

    private readonly List<KickNotification> m_kickNotifications = new List<KickNotification>();

    public GuideLevelData guideLevelData;
    public int guideLevel = 1;
    public AppRunModel appRunModel;
    public bool isSimulationModel;

    public UserSettings userSettings;
    public readonly UserARObjects arObjects = new UserARObjects();

    private int m_coin;
    private int m_avatarId;
    private User_Course_Info m_userCourseInfo;

    public static readonly UserManager Instance = new UserManager();

    private UserManager()
    {
        Reset();
    }

    public void Reset() {
        UserId = 0;
        Coin = 0;
        AccountName = "";
        Password = "";
        AccountId = 0;
        Token = "";
        m_UserPermissions = User_Type.Student;
        Nickname = "";
        m_avatarId = 0;
        m_Class.Clear();
        AccountExpireTimeUTC = DateTime.MinValue;
        CurClass = null;
        CurSubmit = null;
        CurTask = null;
        PhoneNum = "";
        mailAddr = "";
        m_kickNotifications.Clear();
        if(userSettings != null) {
            userSettings.Reset();
        }
        arObjects.Reset();
        UserCourseInfo = new User_Course_Info();
        HonorWallData.instance.Clear();
    }

    public string AccountName {
        get;
        set;
    }

    public string Password {
        get;
        set;
    }

    public uint AccountId {
        get;
        set;
    }

    public string Token {
        get;
        set;
    }

    public string Nickname {
        get;
        set;
    }
    public string PhoneNum {
        get;
        set;
    }

    public string mailAddr {
        get;
        set;
    }

    public int AvatarID {
        get { return m_avatarId; }
        set
        {
            if (m_avatarId != value)
            {
                m_avatarId = value;
                if (onAvatarIdChanged != null)
                {
                    onAvatarIdChanged();
                }
            }
        }
    }

    public string baiduToken
    {
        get;
        set;
    }

    public List<ClassInfo> ClassList {
        get { return m_Class; }
    }

    public void ClassSort() {
        m_Class.Sort((ite1, ite2) => { return ite1.m_ID.CompareTo(ite2.m_ID); });
    }

    public ClassInfo GetClass(uint id) {
        for(int i = 0; i < m_Class.Count; ++i) {
            if(m_Class[i].m_ID == id) {
                return m_Class[i];
            }
        }
        return null;
    }

    public void DeleteClass(uint id) {
        for(int i = 0; i < m_Class.Count; ++i) {
            if(m_Class[i].m_ID == id) {
                m_Class.RemoveAt(i);
                break;
            }
        }
    }

    public uint UserId {
        get;
        set;
    }

    public int Coin
    {
        get { return m_coin; }
        set
        {
            if (m_coin < 0)
            {
                throw new ArgumentOutOfRangeException("value");
            }

            m_coin = value;
            if (onCoinChange != null)
            {
                onCoinChange(m_coin);
            }
        }
    }

    public User_Course_Info UserCourseInfo
    {
        get { return m_userCourseInfo; }
        set
        {
            m_userCourseInfo = value;
        }
    }

    public User_Type Authority {
        set { m_UserPermissions = value; }
        get { return m_UserPermissions; }
    }

    public bool IsStudent 
    {
        get { return !IsTeacher; }
    }

    public bool IsTeacher
    {
        get { return (m_UserPermissions & User_Type.Teacher) != 0; }
    }

    public bool IsAdmin
    {
        get { return (m_UserPermissions & User_Type.Admin) != 0; }
    }

    public bool IsAdminOrTeacher
    {
        get { return IsTeacher || IsAdmin; }
    }

    public bool IsPythonUser
    {
        get { return IsAdminOrTeacher || (m_UserPermissions & User_Type.Python) != 0; }
        set
        {
            if (value) { m_UserPermissions |= User_Type.Python; }
            else { m_UserPermissions &= ~User_Type.Python; }
        }
    }

    public bool IsGameboardUser
    {
        get { return IsAdminOrTeacher || (m_UserPermissions & User_Type.Gameboard) != 0; }
        set
        {
            if (value) { m_UserPermissions |= User_Type.Gameboard; }
            else { m_UserPermissions &= ~User_Type.Gameboard; }
        }
    }

    public bool IsArUser {
        get { return IsAdminOrTeacher || (m_UserPermissions & User_Type.Ar) != 0; }
        set {
            if(value) { m_UserPermissions |= User_Type.Ar; } else { m_UserPermissions &= ~User_Type.Ar; }
        }
    }
    public bool IsLoggedIn
    {
        get { return 0 != UserId; }
    }

    public void AddMail(MailInfo info) {
        m_Mail.Add(info);
    }

    public void DeleteMail(ulong id) {
        for(int i = 0; i < m_Mail.Count; ++i) {
            if(m_Mail[i].m_MailID == id) {
                m_Mail.RemoveAt(i);
                break;
            }
        }
    }

    public void ClearMail() {
        m_Mail.Clear();
    }

    public MailInfo GetMail(ulong id) {
        for(int i = 0; i < m_Mail.Count; ++i) {
            if(m_Mail[i].m_MailID == id) {
                return m_Mail[i];
            }
        }
        return null;
    }

    public void AddClassRequestInfo(ClassRequestInfo request) {
        List<ClassRequestInfo> tClass;
        if(!m_ClassRequestList.TryGetValue(request.m_ClassID, out tClass)) {
            tClass = new List<ClassRequestInfo>();
            m_ClassRequestList.Add(request.m_ClassID, tClass);
        }
        tClass.Add(request);
        m_MailAndClass.Add(request.m_MailID, request.m_ClassID);
    }

    public List<ClassRequestInfo> GetClassRequestList(uint classID) {
        List<ClassRequestInfo> tClass = null;
        m_ClassRequestList.TryGetValue(classID, out tClass);
        return tClass;
    }

    public ClassRequestInfo GetClassRequestInfo(ulong RequestID) {
        uint tClassID = 0;
        List<ClassRequestInfo> tClass = null;
        if(m_MailAndClass.TryGetValue(RequestID, out tClassID) && m_ClassRequestList.TryGetValue(tClassID, out tClass)) {
            for(int i = 0; i < tClass.Count; ++i) {
                ClassRequestInfo tCurRequest = tClass[i];
                if(tCurRequest.m_MailID == RequestID) {
                    return tCurRequest;
                }
            }
        }

        return null;
    }

    public void DeleteClassRequest(ulong RequestID) {
        uint tClassID = 0;
        List<ClassRequestInfo> tClass = null;
        if(m_MailAndClass.TryGetValue(RequestID, out tClassID) && m_ClassRequestList.TryGetValue(tClassID, out tClass)) {
            for(int i = 0; i < tClass.Count; ++i) {
                ClassRequestInfo tCurRequest = tClass[i];
                if(tCurRequest.m_MailID == RequestID) {
                    tClass.RemoveAt(i);
                    break;
                }
            }
        }
    }

    public void ClearClassRequests() {
        m_ClassRequestList.Clear();
        m_MailAndClass.Clear();
    }

    public void AddKickNotification(KickNotification notification) {
        m_kickNotifications.Add(notification);

        if(onNewKickNotification != null) {
            onNewKickNotification(notification);
        }
    }

    public void RemoveKickNotification(ulong notificationId) {
        m_kickNotifications.RemoveAll(x => x.id == notificationId);
    }

    public void ClearKickNotifications() {
        m_kickNotifications.Clear();
    }

    public IList<KickNotification> KickNotifications {
        get { return m_kickNotifications; }
    }

    public ClassInfo CurClass {
        get;
        set;
    }

    public TaskInfo CurTask {
        get;
        set;
    }

    public TaskSubmitInfo CurSubmit {
        get;
        set;
    }

    public DateTime? AccountExpireTimeUTC
    {
        get;
        set;
    }

    public bool IsAccountExpired
    {
        get
        {
            if (AccountExpireTimeUTC == null) { return false; }
            return ServerTime.UtcNow >= AccountExpireTimeUTC;
        }
    }

    public bool IsCourseEnded(uint courseId)
    {
        if (!UserCourseInfo.UserAttendCourseMap.ContainsKey(courseId))
        {
            return false;
        }
        var courseInfo = UserCourseInfo.UserAttendCourseMap[courseId];
        return courseInfo.CourseStatus != 0;
    }

    public bool IsCourseFociblyEnded(uint courseId)
    {
        if (!UserCourseInfo.UserAttendCourseMap.ContainsKey(courseId))
        {
            return false;
        }
        var courseInfo = UserCourseInfo.UserAttendCourseMap[courseId];
        return (courseInfo.CourseStatus & (uint)End_Course_Type.EctForce) != 0;
    }

    public void ForceEndingCourse(uint courseId)
    {
        User_Period_Info_List courseInfo;
        if (!UserCourseInfo.UserAttendCourseMap.TryGetValue(courseId, out courseInfo))
        {
            courseInfo = new User_Period_Info_List();
            UserCourseInfo.UserAttendCourseMap.Add(courseId, courseInfo);
        }
        courseInfo.CourseStatus |= (uint)End_Course_Type.EctForce;
    }

    public Dictionary<uint, User_Topic_Unit_Info> CreateTopics;
    public Dictionary<uint, User_Topic_Unit_Info> AttendTopics;
}
