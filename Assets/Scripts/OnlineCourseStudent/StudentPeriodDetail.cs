using Gameboard;
using Google.Protobuf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

using g_WebRequestManager = Singleton<WebRequestManager>;

public enum PerioditemStatus
{
    Default,
    Finish
} 

public class StudentPeriodDetail : ScrollCell {
    public StudentPeriodManager periodManager;
    public Sprite[] spriteIcons;
    public Image icon;
    public GameObject passGo; 
    public Text periodName;

    private Period_Item_Info periodItemInfo;
    private byte[] imageData;
    private St_Period_Info stPeriodInfo;
    private Gameboard.Gameboard onlineGameboard;
    public OnlineCourseStudentController onLineCourseStController;
    private bool showSubmitResult;

    public override void configureCellData() {
        this.stPeriodInfo = periodManager.stPeriodInfo;
        this.periodItemInfo = (Period_Item_Info)DataObject;
        icon.sprite = spriteIcons[periodItemInfo.ItemType];
        periodName.text = periodItemInfo.ItemName;
        Refresh();
    }

    public void OnClickCell() {
        switch(periodItemInfo.eItemType) {
            case Period_Item_Type.ItemImage:
                if(Utils.IsValidUrl(periodItemInfo.ItemUrl)) {
                    Application.OpenURL(periodItemInfo.ItemUrl);
                    SendPeriodItemState();
                    return;
                }

                PopupManager.ImagePreview(periodItemInfo.ItemUrl);
                SendPeriodItemState();
                break;
            case Period_Item_Type.ItemDoc:
                if(Utils.IsValidUrl(periodItemInfo.ItemUrl)) {
                    Application.OpenURL(periodItemInfo.ItemUrl);
                    SendPeriodItemState();
                    return;
                }

                break;
            case Period_Item_Type.ItemVideo:
                if(Utils.IsValidUrl(periodItemInfo.ItemUrl)) {
                    Application.OpenURL(periodItemInfo.ItemUrl);
                    SendPeriodItemState();
                    return;
                }
                SharedVideoInfo sharedVideoInfo = new SharedVideoInfo();
                sharedVideoInfo.filename = g_WebRequestManager.instance.GetMediaPath(periodItemInfo.ItemUrl, true);
                SharedVideo shareVideo = new SharedVideo(0, 0, null, sharedVideoInfo);
                PopupManager.VideoPreview(shareVideo);
                SendPeriodItemState();
                break;
            case Period_Item_Type.ItemGb:
                OpenGameboard();
                break;
            case Period_Item_Type.ItemProject:
                PopupManager.YesNo("ui_download_project".Localize(), () => {
                    if(Preference.scriptLanguage == ScriptLanguage.Visual) {
                        if(CodeProjectRepository.instance.hasProject("", periodItemInfo.ItemName)) {
                            PopupManager.YesNo("local_down_notice".Localize(periodItemInfo.ItemName), ConfirmDownLoad);
                        } else {
                            ConfirmDownLoad();
                        }
                    } else {
                        ConfirmDownLoad();
                        //if(PythonRepository.instance.hasProject(stPeriodInfo.courseInfo.CourseName, periodItemInfo.ItemName)) {
                        //    PopupManager.YesNo("local_down_notice".Localize(periodItemInfo.ItemName), ConfirmDownLoad);
                        //} else {
                            
                        //}
                    }
                    Debug.Log("===>"+ periodItemInfo.ProjectPath);
                });
                break;
        }
    }

    void ConfirmDownLoad() {
        var request = new SaveProjectAsRequest();
        if(Preference.scriptLanguage == ScriptLanguage.Visual) {
            request.saveAsType = CloudSaveAsType.Project;
        } else {
            request.saveAsType = CloudSaveAsType.ProjectPy;
        }
        request.basePath = periodItemInfo.ProjectPath;
        if(Preference.scriptLanguage == ScriptLanguage.Visual) {
            request.saveAs = periodItemInfo.ItemName;
        } else {
            request.saveAs = stPeriodInfo.courseInfo.CourseName;
        }
        request.blocking = true;
        request.Success(flist => {
            if(Preference.scriptLanguage == ScriptLanguage.Visual) {
                CodeProjectRepository.instance.save(periodItemInfo.ItemName, flist.FileList_);
                PatchGameboardCodeGroups(periodItemInfo, SendPeriodItemState);
            } else {
                PythonRepository.instance.saveSameNameNotice(stPeriodInfo.courseInfo.CourseName, flist.FileList_.ToList(),(downloadStr)=> {
                    if(downloadStr.Count > 0) {
                        PopupManager.Notice("ui_download_sucess".Localize());
                    }
                });
            }
        })
            .Execute();
    }

    void PatchGameboardCodeGroups(Period_Item_Info projectItem, Action done)
    {
        var gameboardItem = stPeriodInfo.periodInfo.GetGameboard();
        if (gameboardItem != null)
        {
            DownloadOnlineGameboard(gameboardItem.GbInfo.ProjPath, () => {
                try
                {
                    if (onlineGameboard == null)
                    {
                        return;
                    }

                    // find the corresponding robot index
                    var path = Gameboard.ProjectUrl.ToRemote(projectItem.ItemId.ToString());
                    int robotIndex = onlineGameboard.GetCodeGroups(ScriptLanguage.Visual)
                                                    .codeGroups.FindIndex(x => x.projectPath == path);
                    if (robotIndex == -1)
                    {
                        // code not assigned, no need to patch
                        return;
                    }

                    var gameboard = GameboardRepository.instance.getGameboard(gameboardItem.ItemName);
                    if (gameboard == null)
                    {
                        return;
                    }

                    // if the robot has no code assigned, probably the binding was broken
                    var group = gameboard.GetCodeGroups(ScriptLanguage.Visual).GetGroup(robotIndex);
                    if (group == null)
                    {
                        // assign the local project
                        gameboard.GetCodeGroups(ScriptLanguage.Visual).SetRobotCode(robotIndex, projectItem.ItemName);
                        WebRequestUtils.UploadAndSaveGameboard(gameboard);
                    }
                }
                finally
                {
                    done();
                }
            });
        }
        else
        {
            done();
        }
    }

    void DownloadOnlineGameboard(string path, Action done)
    {
        if (onlineGameboard == null)
        {
            var request = new ProjectDownloadRequest();
            request.preview = true;
            request.basePath = path;
            request.blocking = true;
            request.Success((FileList dir) => {
                    onlineGameboard = dir.GetGameboard();
                })
                .Finally(done)
                .Execute();
        }
        else
        {
            done();
        }
    }

    void SendPeriodItemState() {
        int popupId = PopupManager.ShowMask();
        CMD_Set_Perioditem_Status_r_Paramters periodItemStatus = new CMD_Set_Perioditem_Status_r_Paramters();
        periodItemStatus.CourseId = stPeriodInfo.courseInfo.CourseId;
        periodItemStatus.PeriodId = stPeriodInfo.periodInfo.PeriodId;
        periodItemStatus.PerioditemId = periodItemInfo.ItemId;
        periodItemStatus.PerioditemStatus = (uint)PerioditemStatus.Finish;

        SocketManager.instance.send(Command_ID.CmdSetPeroditemStatusR, periodItemStatus.ToByteString(), (res, content) => {
            PopupManager.Close(popupId);
            if(res != Command_Result.CmdNoError) {
                PopupManager.Notice(res.Localize());
            } else {
                User_PeriodItem_Info _periodItemInfo = new User_PeriodItem_Info();
                _periodItemInfo.ItemStatus = (uint)PerioditemStatus.Finish;

                stPeriodInfo.AddOrModifyUserPeriodItem(periodItemInfo.ItemId, _periodItemInfo);
                Refresh();
                if((Period_Item_Type)periodItemInfo.ItemType == Period_Item_Type.ItemProject) {
                    var repoPath = CodeProjectRepository.instance.createFilePath(periodItemInfo.ItemName);
                    PopupManager.Workspace(CodeSceneArgs.FromPath(repoPath.ToString()));
                }
            }
        });
    }

    public void Refresh() {
        if((Period_Item_Type)periodItemInfo.ItemType == Period_Item_Type.ItemGb) {
            periodManager.GbScore = stPeriodInfo.GetPeridGbScore(periodItemInfo);
        }

        passGo.SetActive(stPeriodInfo.UserPeriod != null && stPeriodInfo.UserPeriod.UserPerioditemList.ContainsKey(periodItemInfo.ItemId));
    }

    public void ClickCell(St_Period_Info stPeriodInfo)
    {
        PopupManager.ILPeriodItem(stPeriodInfo, () => {
            //CalculatePeriodLevel();
            //scroll.initWithData(periodInfos);
        });
    }


    void OpenGameboard() {
        var player = PopupManager.GameboardPlayer();

        //SubmitHandler submitHandler = (path, (PopupILPeriod.PassModeType)1, res) => {
        //    UploadAnswerAndFinishItem(path, res.robotScores[0], () => {
        //        player.Close();
        //    });
        //};

        SubmitHandler submitHandler = null;
        submitHandler = (path, mode, res) => {
            if (mode == (PopupILPeriod.PassModeType)stPeriodInfo.periodInfo.PeriodType)
            {
                int score = 0;
                if (mode == PopupILPeriod.PassModeType.Submit)
                {
                    score = res.robotScores[0];
                }
                else
                {
                    score = res.sceneScore;
                }
                UploadAnswerAndFinishItem(path, score, () => {
                    player.Close();
                });
            }
        };



        SaveHandler saveHandler = null;
        if (stPeriodInfo.courseInfo.CourseAllowDownloadGb)
        {
            saveHandler = () => {
                if (GameboardRepository.instance.hasProject("", periodItemInfo.ItemName))
                {
                    PopupManager.YesNo("local_down_notice".Localize(periodItemInfo.ItemName), DownloadSaveGb);
                }
                else
                {
                    DownloadSaveGb();
                }
            };
        }

        List<string> relations = null;
        if(Preference.scriptLanguage == ScriptLanguage.Python) {
            relations = new List<string>();
            foreach (var period in stPeriodInfo.periodInfo.PeriodItems.Values)
            {
                if((Period_Item_Type)period.ItemType == Period_Item_Type.ItemProject) {
                    relations.Add(period.ProjectPath);
                }
            } 
        }

        var config = new GameboardPlayerConfig {
            gameboardPath = ProjectPath.Remote(periodItemInfo.GbInfo.ProjPath),
            customBindings = GetCustomCodeBindings(),
            relations = relations,
            gameboardModifier = gameboard => {
                // patch the path to the online code
                var groups = gameboard.GetCodeGroups(Preference.scriptLanguage);
                foreach (var g in groups)
                {
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
            editable = true,

        };

        if(Preference.scriptLanguage == ScriptLanguage.Visual) {
            config.isPreview = true;
        } else {
            config.isPreview = false ;
        }
        player.payload = config;
    }

    private GameboardCustomCodeGroups GetCustomCodeBindings()
    {
        var bindingKey = CourseCodeGroupsKey.Create(stPeriodInfo.courseInfo.CourseId, stPeriodInfo.periodInfo.PeriodId);
        var bindSettings = (GameboardCodeGroups)UserManager.Instance.userSettings.Get(bindingKey, true);
        return new GameboardCustomCodeGroups(bindSettings.codeGroups);
    }

    void UploadAnswerAndFinishItem(IRepositoryPath path, int score, Action done)
    {
        int popupId = PopupManager.ShowMask();
        UploadAnswer(path, score, uploadRes => {
            if (uploadRes != Command_Result.CmdNoError)
            {
                PopupManager.Notice(uploadRes.Localize());
                PopupManager.Close(popupId);
                return;
            }

            UpdatePeriodItemStatus(updateRes => {
                if (updateRes == Command_Result.CmdNoError)
                {
                    Refresh();
                    showSubmitResult = true;
                    CloseAndShowResult(score);

                    if (done != null)
                    {
                        done();
                    }
                }
                else
                {
                    PopupManager.Notice(uploadRes.Localize());
                }
                PopupManager.Close(popupId);
            });
        });
        
    }

    void UploadAnswer(IRepositoryPath fileOrDirPath, int score, Action<Command_Result> callback) {
        var request = new CMD_Answer_Perioditem_r_Parameters();
        if (fileOrDirPath != null)
        {
            IEnumerable<FileData> project;
            if (Preference.scriptLanguage == ScriptLanguage.Visual)
            {
                project = CodeProjectRepository.instance.loadCodeProject(fileOrDirPath.ToString());
            }
            else
            {
                project = PythonRepository.instance.loadProjectFiles(fileOrDirPath.ToString());
            }
            if (project != null)
            {
                request.PeriodFiles = new FileList();
                request.PeriodFiles.FileList_.AddRange(project.ToFileNodeList(""));
            }
        }

        request.CourseId = stPeriodInfo.courseInfo.CourseId;
        request.PeriodId = stPeriodInfo.periodInfo.PeriodId;
        request.PerioditemId = periodItemInfo.ItemId;

        var answer = new GBAnswer();
        answer.GbScriptShow = (uint)GbScriptShowType.Show;
        if (fileOrDirPath != null)
        {
            answer.AnswerName = UserManager.Instance.Nickname + "_" + fileOrDirPath.name;
        }
        else
        {
            answer.AnswerName = UserManager.Instance.Nickname;
        }

        answer.GbScore = score;

        var oldAnswer = periodItemInfo.GbInfo.GetUserAnswer(UserManager.Instance.UserId);
        answer.AnswerId = oldAnswer != null ? oldAnswer.AnswerId : 0;
        request.PeriodAnswerInfo = answer;

        SocketManager.instance.send(Command_ID.CmdAnswerPerioditemR, request.ToByteString(), (res, content) => {
            if (res == Command_Result.CmdNoError)
            {
                var answerA = CMD_Answer_Perioditem_a_Parameters.Parser.ParseFrom(content);
                periodItemInfo.GbInfo.GbAnswerList[answerA.PeriodAnswerInfo.AnswerId] = answerA.PeriodAnswerInfo;
            }
            callback(res);
        });
    }

    private void UpdatePeriodItemStatus(Action<Command_Result> callback)
    {
        if (stPeriodInfo.IsItemFinished(periodItemInfo.ItemId))
        {
            callback(Command_Result.CmdNoError);
            return;
        }

        var request = new CMD_Set_Perioditem_Status_r_Paramters();
        request.CourseId = stPeriodInfo.courseInfo.CourseId;
        request.PeriodId = stPeriodInfo.periodInfo.PeriodId;
        request.PerioditemId = periodItemInfo.ItemId;
        request.PerioditemStatus = (uint)PerioditemStatus.Finish;

        SocketManager.instance.send(Command_ID.CmdSetPeroditemStatusR, request.ToByteString(), (res, content) => {
            if (res == Command_Result.CmdNoError)
            {
                var itemInfo = new User_PeriodItem_Info();
                itemInfo.ItemStatus = (uint)PerioditemStatus.Finish;

                stPeriodInfo.AddOrModifyUserPeriodItem(periodItemInfo.ItemId, itemInfo);
            }
            callback(res);
        });
    }

    void CloseAndShowResult(int score)
    {
        if (showSubmitResult)
        {
            if (score >= stPeriodInfo.periodInfo.PeriodFinsishCon.PassScore)
            {
                PopupManager.Notice("ui_text_pass_level".Localize(), () => {
                    periodManager.Close();
                });
            }
            else
            {
                PopupManager.Notice("ui_text_fail_level".Localize());
            }
        }
        showSubmitResult = false;
    }

    void DownloadSaveGb() {
        var request = new SaveProjectAsRequest();
        request.basePath = periodItemInfo.GbInfo.ProjPath;
        request.saveAsType = CloudSaveAsType.GameBoard;
        request.saveAs = periodItemInfo.ItemName;
        request.blocking = true;
        request.Success(fileList => {
                GameboardRepository.instance.save(periodItemInfo.ItemName, fileList.FileList_);
                var gameboard = fileList.GetGameboard();
                if (gameboard != null)
                {
                    gameboard.name = periodItemInfo.ItemName;
                    PatchGameboardCodeGroups(gameboard);
                }
            })
            .Execute();
    }

    void PatchGameboardCodeGroups(Gameboard.Gameboard gameboard)
    {
        var groups = gameboard.GetCodeGroups(ScriptLanguage.Visual);
        var newGroups = new List<Gameboard.RobotCodeGroupInfo>();
        foreach (var group in groups)
        {
            var itemId = uint.Parse(Gameboard.ProjectUrl.GetPath(group.projectPath));
            var item = stPeriodInfo.periodInfo.GetItem(itemId);
            if (item != null)
            {
                newGroups.Add(new Gameboard.RobotCodeGroupInfo(item.ItemName, group.robotIndices));
            }
        }
        groups.ClearCodeGroups();
        groups.Add(newGroups);

        WebRequestUtils.UploadAndSaveGameboard(gameboard);
    }

}
