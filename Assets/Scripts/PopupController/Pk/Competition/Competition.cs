using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Competition
{
    public event Action<CompetitionProblem> onProblemAdded;
    public event Action<CompetitionProblem> onProblemRemoved;

    public enum OpenState
    {
        Invalid,
        NotStarted,
        Open,
        Closed,
    }

    private string m_name = string.Empty;
    private string m_coverUrl = string.Empty;
    private readonly List<CompetitionProblem> m_problems = new List<CompetitionProblem>();
    private CourseTrophySetting m_trophySettings;

    public uint id
    {
        get;
        set;
    }

    public string name
    {
        get { return m_name; }
        set
        {
            if (value == null)
            {
                throw new ArgumentNullException();
            }
            m_name = value;
        }
    }

    public uint creatorId
    {
        get;
        set;
    }

    public DateTime startTime
    {
        get;
        set;
    }

    public DateTime endTime
    {
        get;
        private set;
    }

    public CompetitionCategory category
    {
        get;
        set;
    }

    public string coverUrl
    {
        get { return m_coverUrl; }
        set
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            m_coverUrl = value;
        }
    }

    public TimeSpan duration
    {
        get { return endTime - startTime; }
        set
        {
            if (value < TimeSpan.Zero)
            {
                throw new ArgumentException();
            }
            endTime = startTime + value;
        }
    }

    public OpenState state
    {
        get
        {
            if (isScratch) { return OpenState.Invalid; }

            var now = ServerTime.UtcNow;
            if (now < startTime)
            {
                return OpenState.NotStarted;
            }
            else if (now < endTime)
            {
                return OpenState.Open;
            }
            else
            {
                return OpenState.Closed;
            }
        }
    }

    public bool isScratch { get; set; }

    public void AddProblem(CompetitionProblem problem)
    {
        if (problem == null)
        {
            throw new ArgumentNullException();
        }
        if (problem.competition != null)
        {
            problem.competition.RemoveProblem(problem);
        }
        problem.competition = this;
        m_problems.Add(problem);

        if (onProblemAdded != null)
        {
            onProblemAdded(problem);
        }
    }

    public void RemoveProblem(CompetitionProblem problem)
    {
        if (problem == null)
        {
            throw new ArgumentNullException();
        }
        if (problem.competition != this)
        {
            throw new ArgumentException();
        }
        problem.competition = null;
        m_problems.Remove(problem);

        if (onProblemRemoved != null)
        {
            onProblemRemoved(problem);
        }
    }

    public IEnumerable<CompetitionProblem> problems
    {
        get { return m_problems; }
    }

    public CompetitionProblem GetProblem(uint id)
    {
        return m_problems.Find(x => x.id == id);
    }

    public int problemCount
    {
        get { return m_problems.Count; }
    }

    public bool HasUserJoined(uint userId)
    {
        return m_problems.Any(x => x.GetAnswer(userId) != null);
    }

    public bool UserJoinedAll(uint userId)
    {
        foreach (var problem in m_problems)
        {
            if (problem.GetAnswer(userId) == null) {
                return false;
            }
        }
        return true;
    }

    public CourseTrophySetting courseTrophySetting
    {
        get { return m_trophySettings; }
        set
        {
            m_trophySettings = value;
            if (value != null)
            {
                m_trophySettings.courseId = id;
            }
        }
    }

    public Competition Clone()
    {
        var comp = new Competition();
        comp.id = id;
        comp.creatorId = creatorId;
        comp.startTime = startTime;
        comp.endTime = endTime;
        comp.m_name = m_name;
        comp.category = category;
        comp.isScratch = isScratch;
        comp.m_coverUrl = m_coverUrl;
        comp.m_problems.AddRange(m_problems.Select(x => {
            var clone = x.Clone();
            clone.competition = comp;
            return clone;
        }));
        comp.courseTrophySetting = m_trophySettings != null ? m_trophySettings.Clone() : null;
        return comp;
    }
}

public class CourseTrophySetting
{
    public static CourseTrophySetting Parse(Course_Honor_Setting courseHonorSetting)
    {
        var courseTrophySetting = new CourseTrophySetting();
        courseTrophySetting.courseId = courseHonorSetting.CourseId;
        courseTrophySetting.courseRaceType = courseHonorSetting.CourseRaceType;
        courseTrophySetting.goldTrophy = TrophySetting.Parse(courseHonorSetting.CourseTrophiesSetting.CourseGoldTrophySetting);
        courseTrophySetting.silverTrophy = TrophySetting.Parse(courseHonorSetting.CourseTrophiesSetting.CourseSiliverTrophySetting);
        courseTrophySetting.bronzeTrophy = TrophySetting.Parse(courseHonorSetting.CourseTrophiesSetting.CourseBronzeTrophySetting);

        return courseTrophySetting;
    }

    public CourseTrophySetting() { }

    public CourseTrophySetting(CourseTrophySetting rhs)
    {
        courseId = rhs.courseId;
        courseRaceType = rhs.courseRaceType;
        if (rhs.goldTrophy != null)
        {
            goldTrophy = rhs.goldTrophy.Clone();
        }
        if (rhs.silverTrophy != null)
        {
            silverTrophy = rhs.silverTrophy.Clone();
        }
        if (rhs.bronzeTrophy != null)
        {
            bronzeTrophy = rhs.bronzeTrophy.Clone();
        }
    }

    public uint courseId { get; set; }

    public Course_Race_Type courseRaceType { get; set; }

    public TrophySetting goldTrophy;
    public TrophySetting silverTrophy;
    public TrophySetting bronzeTrophy;

    public CourseTrophySetting Clone()
    {
        return new CourseTrophySetting(this);
    }
}
public class CompetitionItem
{
    public enum Type
    {
        Doc = 0,
        Image = 1,
        Video = 2,
        Gb = 3,
    }

    public CompetitionItem(Type type)
    {
        this.type = type;
    }

    protected CompetitionItem() { }

    public CompetitionProblem problem
    {
        get;
        set;
    }

    public uint id;
    public string url;
    public string nickName;
    public Type type { get; protected set; }

    public virtual CompetitionItem Clone()
    {
        var item = (CompetitionItem)MemberwiseClone();
        item.problem = null;
        return item;
    }
}

public class CompetitionGameboardItem : CompetitionItem
{
    private string m_name = string.Empty;

    public CompetitionGameboardItem()
    {
        this.type = Type.Gb;
    }

    public string name
    {
        get { return m_name; }
        set
        {
            if (m_name == null)
            {
                throw new ArgumentNullException();
            }
            m_name = value;
        }
    }

    public int sceneId;

    public string downloadPath;
}

public class CompetitionProblemAnswer
{
    public uint userId;
    public string userNickname;
    public int score;
    public string codePath;

    public CompetitionProblemAnswer(uint userId, string userNickname, int score, string codePath)
    {
        this.userId = userId;
        this.userNickname = userNickname;
        this.score = score;
        this.codePath = codePath;
    }

    public CompetitionProblemAnswer Clone()
    {
        return (CompetitionProblemAnswer)MemberwiseClone();
    }
}

public class CompetitionProblem
{
    public enum PeriodType {
        Submit,
        Play
    }

    public event Action onAddedAnswer;

    private string m_name = string.Empty;
    private string m_description = string.Empty;

    private CompetitionGameboardItem m_gameboardItem;
    private readonly List<CompetitionItem> m_attachments = new List<CompetitionItem>();
    private readonly Dictionary<uint, CompetitionProblemAnswer> m_answers
        = new Dictionary<uint, CompetitionProblemAnswer>();

    public uint id
    {
        get;
        set;
    }

    public uint periodType {
        get;
        set;
    }

    public string name
    {
        get { return m_name; }
        set
        {
            if (value == null)
            {
                throw new ArgumentNullException();
            }
            m_name = value;
        }
    }

    public string description
    {
        get { return m_description; }
        set
        {
            if (value == null)
            {
                throw new ArgumentNullException();
            }
            m_description = value;
        }
    }

    public Competition competition
    {
        get;
        set;
    }

    public int answerCount
    {
        get { return m_answers.Count; }
    }

    public void AddAnswer(CompetitionProblemAnswer answer)
    {
        m_answers.Add(answer.userId, answer);
        if (onAddedAnswer != null)
        {
            onAddedAnswer();
        }
    }

    public void AddAnswers(IEnumerable<CompetitionProblemAnswer> answers)
    {
        foreach (var answer in answers)
        {
            m_answers.Add(answer.userId, answer);
        }
    }

    public CompetitionProblemAnswer GetAnswer(uint userId)
    {
        CompetitionProblemAnswer answer;
        m_answers.TryGetValue(userId, out answer);
        return answer;
    }

    public void AddOrUpdateAnswer(CompetitionProblemAnswer answer)
    {
        var oldAnswer = GetAnswer(answer.userId);
        if (oldAnswer != null)
        {
            m_answers.Remove(answer.userId);
            m_answers.Add(answer.userId, answer);
        }
        else
        {
            AddAnswer(answer);
        }
    }

    public int GetScore(uint userId)
    {
        var answer = GetAnswer(userId);
        return answer != null ? answer.score : -1;
    }

    public CompetitionGameboardItem gameboardItem
    {
        get { return m_gameboardItem; }
        set
        {
            if (m_gameboardItem != null)
            {
                m_gameboardItem.problem = null;
            }
            m_gameboardItem = value;
            if (m_gameboardItem != null)
            {
                m_gameboardItem.problem = this;
            }
        }
    }

    public IEnumerable<CompetitionItem> attachments
    {
        get { return m_attachments; }
    }

    public void AddAttachment(CompetitionItem item)
    {
        if (item == null)
        {
            throw new ArgumentNullException();
        }
        if (item.problem != null)
        {
            item.problem.RemoveAttachment(item);
        }
        if (item.type == CompetitionItem.Type.Gb)
        {
            throw new InvalidOperationException("use gameboardItem property");
        }
        item.problem = this;
        m_attachments.Add(item);
    }

    public void RemoveAttachment(CompetitionItem item)
    {
        if (item == null)
        {
            throw new ArgumentNullException();
        }
        if (item.problem != this)
        {
            throw new ArgumentException();
        }
        if (item.type == CompetitionItem.Type.Gb)
        {
            throw new InvalidOperationException("use gameboardItem property");
        }
        item.problem = null;
        m_attachments.Remove(item);
    }

    public void RemoveAttachById(uint id) {
        var attach = m_attachments.Find(x=> { return x.id == id; });
        if (attach != null) {
            m_attachments.Remove(attach);
        }
    }

    public CompetitionProblem Clone()
    {
        var problem = new CompetitionProblem();
        problem.id = id;
        problem.m_name = m_name;
        problem.m_description = m_description;

        problem.gameboardItem = (CompetitionGameboardItem)m_gameboardItem.Clone();
        problem.m_attachments.AddRange(m_attachments.Select(x => {
            var clone = x.Clone();
            clone.problem = problem;
            return clone;
        }));

        foreach (var kv in m_answers)
        {
            problem.m_answers.Add(kv.Key, kv.Value.Clone());
        }
        return problem;
    }
}