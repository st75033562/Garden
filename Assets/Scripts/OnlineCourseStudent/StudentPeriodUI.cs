using Gameboard;
using Google.Protobuf;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class St_Period_Info{
    public Course_Info courseInfo;
    public Period_Info periodInfo;

    private User_Period_Info_List m_userPeriodInfolist;
    private User_Period_Info m_userPeriodInfo;
    public St_Period_Info(Course_Info courseInfo , Period_Info periodInfo){
        this.courseInfo = courseInfo;
        this.periodInfo = periodInfo;
    }

    public User_Period_Info_List UserPeriodInfoList {  //包含所有课时集合
        get {
            if (m_userPeriodInfolist == null)
                UserManager.Instance.UserCourseInfo.UserAttendCourseMap.TryGetValue(courseInfo.CourseId, out m_userPeriodInfolist);
            return m_userPeriodInfolist;
        }
    }

    public User_Period_Info UserPeriod {  //当前课时包含所有课时内容
        get {
            if(UserPeriodInfoList != null && m_userPeriodInfo == null)
                UserPeriodInfoList.UserAttendPeriodList.TryGetValue(periodInfo.PeriodId, out m_userPeriodInfo);
            return m_userPeriodInfo;
        }
    }

    void AddUserCourse(User_Period_Info_List userCourse) {
        UserManager.Instance.UserCourseInfo.UserAttendCourseMap.Add(courseInfo.CourseId , userCourse);
        
    }
    public void AddOrModifyUserPeriod(User_Period_Info userPeriodInfo) {
        if(UserPeriodInfoList == null) {
            m_userPeriodInfolist = new User_Period_Info_List();
            m_userPeriodInfolist.UserAttendPeriodList.Add(periodInfo.PeriodId, userPeriodInfo);
            
            AddUserCourse(m_userPeriodInfolist);
        } else {
            UserPeriodInfoList.UserAttendPeriodList[periodInfo.PeriodId] = userPeriodInfo;
        }
    }

    public void AddOrModifyUserPeriodItem(uint periodItemId, User_PeriodItem_Info periodItemInfo) {
        if(UserPeriod == null) {
            m_userPeriodInfo = new User_Period_Info();
            m_userPeriodInfo.UserPerioditemList.Add(periodItemId , periodItemInfo);

            AddOrModifyUserPeriod(m_userPeriodInfo);
        } else {
            UserPeriod.UserPerioditemList[periodItemId] = periodItemInfo;
        }
    }

    public bool IsItemFinished(uint itemId)
    {
        User_PeriodItem_Info itemInfo;
        if (UserPeriod != null && UserPeriod.UserPerioditemList.TryGetValue(itemId, out itemInfo))
        {
            return itemInfo.ItemStatus == (uint)PerioditemStatus.Finish;
        }
        return false;
    }

    public bool IsPass() {
        return GetAllGbScore() >= periodInfo.PeriodFinsishCon.PassScore;
    }

    public int GetAllGbScore() {
        int allScore = 0;
        foreach(Period_Item_Info periodItem in periodInfo.PeriodItems.Values) {
            if(periodItem.GbInfo == null)
                continue;
            allScore += GetPeridGbScore(periodItem);
        }
        return allScore;
    }

    public int GetPeridGbScore(Period_Item_Info periodItem) {
        int score = 0;
        foreach(GBAnswer answer in periodItem.GbInfo.GbAnswerList.Values) {
            if(answer.AnswerUserId == UserManager.Instance.UserId) {
                score += answer.GbScore;
                break;
            }
        }
        score = score < 0 ? 0 : score;
        return score;
    }

    public int GetStarCount() {
        if(periodInfo == null || GetAllGbScore() == 0) {
            return 0;
        }else if(GetAllGbScore() >= periodInfo.PeriodFinsishCon.ThreestarScore) {
            return 3;
        } else if(GetAllGbScore() >= periodInfo.PeriodFinsishCon.DoublestarScore) {
            return 2;
        }
        return 1;
    }

    public string FinishProgress() {
        foreach(uint i in courseInfo.PeriodDisplayList) {
            //periodInfos.Add(new St_Period_Info(courseInfo, courseInfo.PeriodList[i]));
        }

        return "";
    }
}

public class StudentPeriodUI : PopupController {
    public class PayLoad {
        public string courseName;
        public List<St_Period_Info> periodInfos;
    }
    public ScrollLoopController scroll;
    public Text textPoints;
    public Text progressText;
    public Text TitleText;

  //  private StOnlineCourseStep stOnlineCourseStep;

    public int CurPeriodLevel { set; get; }

    private List<St_Period_Info> periodInfos ;

    protected override void Start() {
        base.Start();
        var data = (PayLoad)payload;
        SetData(data.courseName, data.periodInfos, null);

    }
    public void SetData(string courseName, List<St_Period_Info> periodInfos, StOnlineCourseStep stOnlineCourseStep) {
        CurPeriodLevel = 0;
        this.periodInfos = periodInfos;

        CalculatePeriodLevel();
        scroll.context = this;
        scroll.initWithData(periodInfos);
        TitleText.text = courseName;
    }

    public void OnClickBack() {
        gameObject.SetActive(false);
    }

    public void ClickCell(St_Period_Info stPeriodInfo) {
        if (stPeriodInfo.periodInfo.PeriodItems.Count == 1) {
            foreach (var periodItemInfo in stPeriodInfo.periodInfo.PeriodItems.Values)
            {
                if (periodItemInfo.eItemType == Period_Item_Type.ItemGbPlay || periodItemInfo.eItemType == Period_Item_Type.ItemGb)
                {
                    OpenGameboard(stPeriodInfo, periodItemInfo);
                    return;
                }
            }
        }
        PopupManager.ILPeriodItem(stPeriodInfo, () => {
            CalculatePeriodLevel();
            scroll.initWithData(periodInfos);
        });
    }

    void OpenGameboard(St_Period_Info stPeriodInfo, Period_Item_Info periodItemInfo) {
        var player = PopupManager.GameboardPlayer();
        SubmitHandler submitHandler = null;
        submitHandler = (path, mode, res) => {
            if(mode == (PopupILPeriod.PassModeType)stPeriodInfo.periodInfo.PeriodType) {
                int score = 0;
                if(mode == PopupILPeriod.PassModeType.Submit) {
                    score = res.robotScores[0];
                } else {
                    score = res.sceneScore;
                }
                UploadAnswerAndFinishItem(stPeriodInfo, periodItemInfo, path, score, () => {
                    player.Close();
                    CalculatePeriodLevel();
                    scroll.initWithData(periodInfos);
                });
            }
        };

        SaveHandler saveHandler = null;
        List<string> relations = null;
        if(Preference.scriptLanguage == ScriptLanguage.Python) {
            relations = new List<string>();
            foreach(var period in stPeriodInfo.periodInfo.PeriodItems.Values) {
                if((Period_Item_Type)period.ItemType == Period_Item_Type.ItemProject) {
                    relations.Add(period.ProjectPath);
                }
            }
        }

        var config = new GameboardPlayerConfig {
            gameboardPath = ProjectPath.Remote(periodItemInfo.GbInfo.ProjPath),
            customBindings = GetCustomCodeBindings(stPeriodInfo),
            relations = relations,
            gameboardModifier = gameboard => {
                // patch the path to the online code
                var groups = gameboard.GetCodeGroups(Preference.scriptLanguage);
                foreach(var g in groups) {
                    var itemId = uint.Parse(Gameboard.ProjectUrl.GetPath(g.projectPath));
                    var realPath = WebRequestManager.GetProjectPath(
                        stPeriodInfo.courseInfo.CourseId,
                        stPeriodInfo.periodInfo.PeriodId,
                        itemId) + "/0"; //服务端是创建一个0的文件夹
                    groups.ChangeGroupPath(g.projectPath, Gameboard.ProjectUrl.ToRemote(realPath));
                }
            },
            submitHandler = submitHandler,
            saveHandler = saveHandler,
            showPySubmit = true,
            editable = false,
            NoTopBarMode = true
        };

        if(Preference.scriptLanguage == ScriptLanguage.Visual) {
            config.isPreview = true;
        } else {
            config.isPreview = false;
        }
        player.payload = config;
    }

    void UploadAnswerAndFinishItem(St_Period_Info stPeriodInfo, Period_Item_Info periodItemInfo, IRepositoryPath path, int score, Action done) {
        int popupId = PopupManager.ShowMask();
        UploadAnswer(path, stPeriodInfo, periodItemInfo, score, uploadRes => {
            if(uploadRes != Command_Result.CmdNoError) {
                PopupManager.Notice(uploadRes.Localize());
                PopupManager.Close(popupId);
                return;
            }

            UpdatePeriodItemStatus(stPeriodInfo, periodItemInfo, updateRes => {
                if(updateRes == Command_Result.CmdNoError) {
                    CloseAndShowResult(stPeriodInfo, score);

                    if(done != null) {
                        done();
                    }
                } else {
                    PopupManager.Notice(uploadRes.Localize());
                }
                PopupManager.Close(popupId);
            });
        });

    }
    private void UpdatePeriodItemStatus(St_Period_Info stPeriodInfo, Period_Item_Info periodItemInfo, Action<Command_Result> callback) {
        if(stPeriodInfo.IsItemFinished(periodItemInfo.ItemId)) {
            callback(Command_Result.CmdNoError);
            return;
        }

        var request = new CMD_Set_Perioditem_Status_r_Paramters();
        request.CourseId = stPeriodInfo.courseInfo.CourseId;
        request.PeriodId = stPeriodInfo.periodInfo.PeriodId;
        request.PerioditemId = periodItemInfo.ItemId;
        request.PerioditemStatus = (uint)PerioditemStatus.Finish;

        SocketManager.instance.send(Command_ID.CmdSetPeroditemStatusR, request.ToByteString(), (res, content) => {
            if(res == Command_Result.CmdNoError) {
                var itemInfo = new User_PeriodItem_Info();
                itemInfo.ItemStatus = (uint)PerioditemStatus.Finish;

                stPeriodInfo.AddOrModifyUserPeriodItem(periodItemInfo.ItemId, itemInfo);
            }
            callback(res);
        });
    }

    void UploadAnswer(IRepositoryPath fileOrDirPath, St_Period_Info stPeriodInfo, Period_Item_Info periodItemInfo, int score, Action<Command_Result> callback) {
        var request = new CMD_Answer_Perioditem_r_Parameters();
        if(fileOrDirPath != null) {
            IEnumerable<FileData> project;
            if(Preference.scriptLanguage == ScriptLanguage.Visual) {
                project = CodeProjectRepository.instance.loadCodeProject(fileOrDirPath.ToString());
            } else {
                project = PythonRepository.instance.loadProjectFiles(fileOrDirPath.ToString());
            }
            if(project != null) {
                request.PeriodFiles = new FileList();
                request.PeriodFiles.FileList_.AddRange(project.ToFileNodeList(""));
            }
        }

        request.CourseId = stPeriodInfo.courseInfo.CourseId;
        request.PeriodId = stPeriodInfo.periodInfo.PeriodId;
        request.PerioditemId = periodItemInfo.ItemId;

        var answer = new GBAnswer();
        answer.GbScriptShow = (uint)GbScriptShowType.Show;
        if(fileOrDirPath != null) {
            answer.AnswerName = UserManager.Instance.Nickname + "_" + fileOrDirPath.name;
        } else {
            answer.AnswerName = UserManager.Instance.Nickname;
        }

        answer.GbScore = score;

        var oldAnswer = periodItemInfo.GbInfo.GetUserAnswer(UserManager.Instance.UserId);
        answer.AnswerId = oldAnswer != null ? oldAnswer.AnswerId : 0;
        request.PeriodAnswerInfo = answer;

        SocketManager.instance.send(Command_ID.CmdAnswerPerioditemR, request.ToByteString(), (res, content) => {
            if(res == Command_Result.CmdNoError) {
                var answerA = CMD_Answer_Perioditem_a_Parameters.Parser.ParseFrom(content);
                periodItemInfo.GbInfo.GbAnswerList[answerA.PeriodAnswerInfo.AnswerId] = answerA.PeriodAnswerInfo;
            }
            callback(res);
        });
    }

    private GameboardCustomCodeGroups GetCustomCodeBindings(St_Period_Info stPeriodInfo) {
        var bindingKey = CourseCodeGroupsKey.Create(stPeriodInfo.courseInfo.CourseId, stPeriodInfo.periodInfo.PeriodId);
        var bindSettings = (GameboardCodeGroups)UserManager.Instance.userSettings.Get(bindingKey, true);
        return new GameboardCustomCodeGroups(bindSettings.codeGroups);
    }

    void CalculatePeriodLevel() {
        CurPeriodLevel = 0;

        int totalPoints = 0;
        foreach(St_Period_Info info in periodInfos) {
            if(info.IsPass()) {
                CurPeriodLevel++;
            } else {
                break;
            }
            totalPoints += info.GetStarCount();
        }
        textPoints.text = totalPoints.ToString();
        float progressValue = 0;
        if(periodInfos.Count != 0)
            progressValue = (float)CurPeriodLevel / periodInfos.Count;
        progressText.text = Math.Round(progressValue, 2) * 100 + "%";
    }

    void CloseAndShowResult(St_Period_Info stPeriodInfo, int score) {
        if(score >= stPeriodInfo.periodInfo.PeriodFinsishCon.PassScore) {
            PopupManager.Notice("ui_text_pass_level".Localize(), () => {

            });
        } else {
            PopupManager.Notice("ui_text_fail_level".Localize());
        }
    }

}
