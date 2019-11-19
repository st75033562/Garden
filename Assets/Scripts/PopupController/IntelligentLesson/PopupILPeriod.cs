using Google.Protobuf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PopupILPeriod : PermitUiBase {
    public ScrollLoopController scroll;
    public ScrollLoopController scrollAttach;
    public GameObject mainPanel;
    public GameObject createEveryLessonGo;
    public GameObject everyLessonContentGo;
    public Text textPeriodName;
    public Text textPeriodDescription;
    public InputField periodName;
    public InputField periodDescription;
    public InputField threeStarText;
    public InputField twoStarText;
    public InputField passText;
    public Text titleName;
    public Text passModeText;
    public GameObject passModeToggle;

    public InputField inputPrice;

    public Button editPeriodNextButton;
    public Button btnEditor;

    public GameObject CourseCreateGo;
    public Toggle toggleSubmit;
    public Toggle togglePlay;

    private List<PeriodItem> periodItems = new List<PeriodItem>();  //课时中每条目：图片、视频、gameboard
    private List<Period_Info> periodInfos = new List<Period_Info>();

    private Course_Info course_Info;
    private CourseInfo courseInfo;
    private Period_Info editingPeriodInfo;
    // may be null if we failed to download the corresponding gameboard
    private PeriodItem gameboardItem;
    private List<PeriodItem> codeBindings = new List<PeriodItem>();
    private bool shouldModifyGameboard;
    private PeriodOperation periodOperation;

    private List<AttachData> attachDatas = new List<AttachData>();
    private PassModeType passModeType = PassModeType.Play;

    public enum PassModeType {
        Submit,
        Play
    }

    protected override void Awake() {
        base.Awake();
        periodName.onValueChanged.AddListener(delegate { UpdateEditPeriodNextButton(); });
    }

    protected override void Start() {
        base.Start();
        InitJurisdiction();
        SetData((CourseInfo)payload);
    }

    private void UpdateEditPeriodNextButton() {
        bool existName = !string.IsNullOrEmpty(periodName.text);
        var gameboard = periodItems.Find(x => { return x.periodItem.eItemType == Period_Item_Type.ItemGb && x.state != PeriodItem.State.DELETE; });
        editPeriodNextButton.interactable = existName && gameboard != null;
    }

    public void SetData(CourseInfo courseInfo) {
        this.courseInfo = courseInfo;
        this.course_Info = courseInfo.proCourseInfo;
        titleName.text = course_Info.CourseName;
        inputPrice.text = courseInfo.CoursePrice.ToString();
        SynchroShowPeriodInfo();
        scroll.context = this;
        scroll.initWithData(periodInfos);
    }

    public void SetOperation(int type) {
        periodOperation = (PeriodOperation)type;
        foreach(Period_Info info in periodInfos) {
            info.periodOperation = periodOperation;
        }
        foreach(ScrollCell cell in scroll.GetCellsInUse()) {
            cell.GetComponent<EveryLessonCell>().UpdateOperation();
        }
        ActiveCourseCreateGo();
    }

    void ActiveCourseCreateGo() {
        if(periodInfos.Count == 0 && periodOperation == PeriodOperation.NONE) {
            CourseCreateGo.SetActive(true);
        } else {
            CourseCreateGo.SetActive(false);
        }
    }

    void SynchroShowPeriodInfo() {
        periodInfos.Clear();
        for(int i = 0; i < course_Info.PeriodDisplayList.Count; i++) {
            periodInfos.Add(course_Info.PeriodList[course_Info.PeriodDisplayList[i]]);
        }
        ActiveCourseCreateGo();
        RelyOnDataMenu(periodInfos.Count != 0);
    }

    public void OnClickSavePrice() {
        uint price = 0;
        try {
            price = uint.Parse(inputPrice.text);
        } catch {
            PopupManager.Notice("ui_input_number".Localize());
            return;
        }
        courseInfo.CoursePrice = price;

        int maskId = PopupManager.ShowMask();
        CMD_Modify_Course_r_Parameters modifyCourseR = new CMD_Modify_Course_r_Parameters();
        modifyCourseR.ModifyInfo = course_Info;
        SocketManager.instance.send(Command_ID.CmdModifyCourseR, modifyCourseR.ToByteString(), (res, content) => {
            PopupManager.Close(maskId);
            if(res != Command_Result.CmdNoError) {
                PopupManager.Notice(res.Localize());
            }
        });
    }

    public void ClickAdd() {
        attachDatas.Clear();
        periodItems.Clear();
        PeriodInfoMode(true);
        editingPeriodInfo = null;
        periodName.text = "";
        periodDescription.text = "";
        UpdateEditPeriodNextButton();
        createEveryLessonGo.SetActive(true);
        toggleSubmit.isOn = false;
        togglePlay.isOn = true;
        passModeType = PassModeType.Play;
    }

    public void ClickShowCreateLesson(Period_Info period) {
        PeriodInfoMode(false);
        editingPeriodInfo = period;
        textPeriodName.text = period.PeriodName;
        textPeriodDescription.text = period.PeriodDescription;
        periodName.text = period.PeriodName;
        periodDescription.text = period.PeriodDescription;
        UpdateEditPeriodNextButton();
        createEveryLessonGo.SetActive(true);

        if((PassModeType)period.PeriodType == PassModeType.Submit) {
            passModeText.text = "ui_problem_solving".Localize();
            toggleSubmit.isOn = true;
            togglePlay.isOn = false;
            passModeType = PassModeType.Submit;
        } else {
            toggleSubmit.isOn = false;
            togglePlay.isOn = true;
            passModeType = PassModeType.Play;
            passModeText.text = "ui_gb_m_game".Localize();
        }

        var gbItemInfo = editingPeriodInfo.GetGameboard();
        if(gbItemInfo != null) {
            DownloadGB(gbItemInfo.GbInfo.ProjPath);
        } else {
            InitPeriodItems(null);
        }
    }

    void PeriodInfoMode(bool isEditor) {
        textPeriodName.gameObject.SetActive(!isEditor);
        textPeriodDescription.gameObject.SetActive(!isEditor);
        passModeText.gameObject.SetActive(!isEditor);
        periodName.gameObject.SetActive(isEditor);
        periodDescription.gameObject.SetActive(isEditor);
        editPeriodNextButton.gameObject.SetActive(isEditor);
        btnEditor.gameObject.SetActive(!isEditor);
        passModeToggle.SetActive(isEditor);

        if(isEditor) {
            if(attachDatas.Count == 0 || attachDatas.Last() != null) {
                attachDatas.Add(null);
            }
        } else {
            if(attachDatas.Count > 0 && attachDatas.Last() == null) {
                attachDatas.Remove(null);
            }
        }

        InitAttachmentScroll();
    }

    private void InitAttachmentScroll()
    {
        var effectAttach = attachDatas.FindAll(x => { return x == null || x.state != AttachData.State.Delete; });
        scrollAttach.initWithData(effectAttach.Select(x => {
            return new AddAttachmentCellData(x, effectAttach.Where(y => y != null));
        }).ToArray());
    }

    public void OnClickEdiortPeriod() {
        PeriodInfoMode(true);
        UpdateEditPeriodNextButton();
    }

    public void OnClickAttachment() {
        int programCount = 0;
        if(passModeType == PassModeType.Submit) {
            programCount = PopupAttachmentManager.MaxAttachCount;
        }

        PopupManager.AttachmentManager(attachDatas, null, ()=> {
            InitAttachmentScroll();

            periodItems.RemoveAll(x=> {return x.periodItem.ItemId == 0; }); //删除本地未同步到服务的item
            foreach (var pItem in periodItems)
            {
                var attachData = attachDatas.Find(x=> { return x != null && x.itemId == pItem.periodItem.ItemId ; });
                if(attachData != null && attachData.state != AttachData.State.Delete) {
                    if(attachData.type == AttachData.Type.Res) {
                        if(attachData.resData.nickName != pItem.periodItem.ItemName) {
                            pItem.state = PeriodItem.State.CHANGED;
                            pItem.periodItem.ItemName = attachData.resData.nickName;
                        }
                    } else {
                        if(attachData.programNickName != pItem.periodItem.ItemName) {
                            pItem.periodItem.ItemName = attachData.programNickName;
                            pItem.state = PeriodItem.State.CHANGED;
                        }
                    }
                } else {
                    pItem.state = PeriodItem.State.DELETE;
                }
            }

            shouldModifyGameboard = true;
            codeBindings.Clear();
            foreach (var attachData in attachDatas)
            {
                if(attachData == null || attachData.state == AttachData.State.Delete) {
                    continue;
                }

                if(attachData.itemId == 0) {
                    if(attachData.type == AttachData.Type.Res) {
                        UploadSucess(attachData.resData);
                    } else if(attachData.type == AttachData.Type.Project) {
                        PeriodItem periodItemPro = PeriodItem.NewProject(attachData.programNickName, attachData.programPath);
                        AddPeriodItem(periodItemPro);
                        if(attachData.isRelation) {
                            codeBindings.Add(periodItemPro);
                        }
                    } else if(attachData.type == AttachData.Type.Gameboard) {
                        var item = PeriodItem.NewGameboard(attachData.programPath, attachData.programNickName, attachData.gameboard);
                        AddPeriodItem(item);
                        gameboardItem = item;
                    }
                } else if(attachData.type == AttachData.Type.Project && attachData.isRelation) {
                    PeriodItem periodItemPro = periodItems.Find(x => { return x != null && x.periodItem.ItemId == attachData.itemId; });
                    if(periodItemPro != null) { 
                        codeBindings.Add(periodItemPro);
                    }
                }
            }

            UpdateEditPeriodNextButton();
        }, programCount: programCount, gameboardCount:1);
    }

    public void ClickDelPeriod(Period_Info period) {
        CMD_Del_Period_r_Parameters delPeriod = new CMD_Del_Period_r_Parameters();
        delPeriod.CourseId = course_Info.CourseId;
        delPeriod.PeriodId = period.PeriodId;
        int maskId = PopupManager.ShowMask();
        SocketManager.instance.send(Command_ID.CmdDelPeriodR, delPeriod.ToByteString(), (res, content) => {
            PopupManager.Close(maskId);
            if(res == Command_Result.CmdNoError) {
                course_Info.PeriodDisplayList.Remove(period.PeriodId);
                course_Info.PeriodList.Remove(period.PeriodId);
                SynchroShowPeriodInfo();
                scroll.refresh();
            } else {
                PopupManager.Notice(res.Localize());
            }
        });
    }

    public void OnClickClosePriodPanel() {
        createEveryLessonGo.SetActive(false);
        periodName.text = "";
        periodDescription.text = "";
    }

    public void OnClickNext() {
        if(passModeType == PassModeType.Play) {
            var projects = attachDatas.FindAll(x => { return x != null && x.state != AttachData.State.Delete && x.type == AttachData.Type.Project; });
            if(projects.Count > 0) {
                PopupManager.Notice("ui_gb_only_play_notice".Localize());
                return;
            }
        }
        createEveryLessonGo.SetActive(false);
        everyLessonContentGo.SetActive(true);

        if(editingPeriodInfo != null && editingPeriodInfo.PeriodFinsishCon != null) {
            threeStarText.text = editingPeriodInfo.PeriodFinsishCon.ThreestarScore.ToString();
            twoStarText.text = editingPeriodInfo.PeriodFinsishCon.DoublestarScore.ToString();
            passText.text = editingPeriodInfo.PeriodFinsishCon.PassScore.ToString();
        } else {
            threeStarText.text = "";
            twoStarText.text = "";
            passText.text = "";
        }
    }

    private void InitPeriodItems(Gameboard.Gameboard gameboard) {
        periodItems.Clear();
        for(int i = 0; i < editingPeriodInfo.PeriodItemDisplayList.Count; i++) {
            PeriodItem item = new PeriodItem();
            item.state = PeriodItem.State.EXISTING;
            item.periodItem = editingPeriodInfo.PeriodItems[editingPeriodInfo.PeriodItemDisplayList[i]];
            periodItems.Add(item);

            if(gameboard != null) {
                if(item.periodItem.eItemType == Period_Item_Type.ItemGb) {
                    item.gameboard = gameboard;
                    gameboardItem = item;
                } else if(item.periodItem.eItemType == Period_Item_Type.ItemProject) {
                    var path = Gameboard.ProjectUrl.ToRemote(item.periodItem.ItemId.ToString());
                    var groups = gameboard.GetCodeGroups(Preference.scriptLanguage);
                    if(groups.GetGroup(path) != null) {
                        codeBindings.Add(item);
                    }
                }
            }
        }

        PeriodItemToAttachment();
    }

    void PeriodItemToAttachment() {
        attachDatas.Clear();
        foreach(var item in periodItems) {
            AttachData data = new AttachData();
            data.itemId = item.periodItem.ItemId;
            if(item.periodItem.eItemType == Period_Item_Type.ItemProject) {
                data.type = AttachData.Type.Project;
                data.webProgramPath = item.periodItem.ProjectPath;
                data.programNickName = item.periodItem.ItemName;
                data.isRelation = IsCodeSelected(item);
            } else if(item.periodItem.eItemType == Period_Item_Type.ItemGb) {
                data.type = AttachData.Type.Gameboard;
                data.webProgramPath = item.periodItem.GbInfo.ProjPath;
                data.programNickName = item.periodItem.ItemName;
                data.gameboard = item.gameboard;
            } else {
                data.type = AttachData.Type.Res;
                LocalResData resData = new LocalResData();
                resData.nickName = item.periodItem.ItemName;
                resData.name = item.periodItem.ItemUrl;
                if(item.periodItem.eItemType == Period_Item_Type.ItemImage) {
                    resData.resType = ResType.Image;
                } else if(item.periodItem.eItemType == Period_Item_Type.ItemVideo) {
                    resData.resType = ResType.Video;
                } else if(item.periodItem.eItemType == Period_Item_Type.ItemDoc) {
                    resData.resType = ResType.Course;
                }
                data.resData = resData;
            }
            attachDatas.Add(data);
        }

        InitAttachmentScroll();
    }

    private void DownloadGB(string path) {
        var request = new ProjectDownloadRequest();
        request.basePath = path;
        request.preview = true;
        request.blocking = true;
        request.Success(dir => {
            var gameboard = dir.GetGameboard();
            if(gameboard == null) {
                Debug.LogError("invalid gameboard");
            }
            InitPeriodItems(gameboard);
        })
            .Error(() => InitPeriodItems(null))
            .Execute();
    }

    void SelectResBack(LocalResData data) {
        if(!Utils.IsValidUrl(data.name)) {
            if(data.resType == ResType.Video) {
                int maskId = PopupManager.ShowMask();
                LoadResource.instance.LoadLocalRes(data.filePath, (www) => {
                    PopupManager.Close(maskId);

                    Uploads.UploadMedia(www.bytes, data.name, true)
                           .Blocking()
                           .Success(() => {
                               UploadSucess(data);
                           })
                           .Execute();
                });
            } else {
                Uploads.UploadMedia(data.textureData, data.name, false)
                       .Blocking()
                       .Success(() => {
                           UploadSucess(data);
                       })
                       .Execute();
            }
        } else {
            UploadSucess(data);
        }
    }

    void UploadSucess(LocalResData data) {
        Period_Item_Info periodInfo = new Period_Item_Info();
        switch(data.resType) {
            case ResType.Image:
                periodInfo.eItemType = Period_Item_Type.ItemImage;
                break;
            case ResType.Video:
                periodInfo.eItemType = Period_Item_Type.ItemVideo;
                break;
            case ResType.Course:
                periodInfo.eItemType = Period_Item_Type.ItemDoc;
                break;
        }
        periodInfo.ItemName = data.nickName;
        if(data.resType == ResType.Course && !Utils.IsValidUrl(data.name)) {
            periodInfo.ItemUrl = Singleton<WebRequestManager>.instance.GetMediaPath(data.name, false);
        } else {
            periodInfo.ItemUrl = data.name;
        }
        AddPeriodItem(PeriodItem.New(periodInfo));
    }

    void AddPeriodItem(PeriodItem contentItem) {
        periodItems.Add(contentItem);
    }

    void DeleteItem(List<PeriodItem> items, Action done) {
        if(items.Count > 0) {
            var item = items[0];
            items.Remove(item);
            DeleteItem(item, ()=> {
                DeleteItem(items, done);
            });
        } else {
            done() ;
        }
    }

    void DeleteItem(PeriodItem item, Action done) {
        if(editingPeriodInfo != null) {
            Period_Item_Info periodItemInfo;
            editingPeriodInfo.PeriodItems.TryGetValue(item.periodItem.ItemId, out periodItemInfo);
            if(periodItemInfo != null) {
                CMD_Del_Perioditem_r_Parameters delPeriodItem = new CMD_Del_Perioditem_r_Parameters();
                delPeriodItem.CourseId = course_Info.CourseId;
                delPeriodItem.PeriodId = editingPeriodInfo.PeriodId;
                delPeriodItem.PerioditemId = periodItemInfo.ItemId;
                int maskId = PopupManager.ShowMask();
                SocketManager.instance.send(Command_ID.CmdDelPerioditemR, delPeriodItem.ToByteString(), (res, content) => {
                    PopupManager.Close(maskId);
                    if(res == Command_Result.CmdNoError) {
                        editingPeriodInfo.PeriodItems.Remove(item.periodItem.ItemId);
                        editingPeriodInfo.PeriodItemDisplayList.Remove(item.periodItem.ItemId);

                        DeleteItemInternal(item);
                    } else {
                        PopupManager.Notice(res.Localize());
                    }
                    done();
                });
            } else {
                DeleteItemInternal(item);
            }
        } else {
            DeleteItemInternal(item);
        }
    }

    void DeleteItemInternal(PeriodItem item) {
        if(item.periodItem.eItemType == Period_Item_Type.ItemProject) {
            codeBindings.Remove(item);
            // no need to upload for a new gameboard
            if(gameboardItem != null &&
                gameboardItem.state != PeriodItem.State.NEW &&
                IsCodeSelected(item)) {
                UploadGameboardCodeGroups();
            }
        }
        periodItems.Remove(item);
    }

    public void OnClickBack() {
        mainPanel.SetActive(true);
        createEveryLessonGo.SetActive(false);
        everyLessonContentGo.SetActive(false);
        gameObject.SetActive(false);
        Reset();
    }

    public void OnClickCreateLessonBack() {
        mainPanel.SetActive(true);
        createEveryLessonGo.SetActive(false);
        everyLessonContentGo.SetActive(false);
        Reset();
    }

    public void OnClickContentBack() {
        mainPanel.SetActive(true);
        createEveryLessonGo.SetActive(true);
        everyLessonContentGo.SetActive(false);
     //   Reset();
    }

    public void OnClickFinish() {
        var delPeriods = periodItems.FindAll(x => { return x.state == PeriodItem.State.DELETE; });
        DeleteItem(delPeriods, ()=> {
            Period_Finish_Condition finishCondition = new Period_Finish_Condition();
            try {
                finishCondition.ThreestarScore = uint.Parse(threeStarText.text);
                finishCondition.DoublestarScore = uint.Parse(twoStarText.text);
                finishCondition.PassScore = uint.Parse(passText.text);
            } catch(Exception) {
                PopupManager.Notice("ui_input_number".Localize());
                return;
            }
            threeStarText.text = "";
            twoStarText.text = "";
            passText.text = "";

            if(editingPeriodInfo == null) {
                Period_Info periodContent = new Period_Info();
                periodContent.PeriodName = periodName.text;
                periodContent.PeriodDescription = periodDescription.text;
                periodContent.PeriodType = (uint)passModeType;

                CMD_Add_Period_r_Parameters addPeriodR = new CMD_Add_Period_r_Parameters();
                addPeriodR.PeriodInfo = periodContent;
                addPeriodR.PeriodInfo.PeriodFinsishCon = finishCondition;
                addPeriodR.CourseId = course_Info.CourseId;

                int maskId = PopupManager.ShowMask();
                SocketManager.instance.send(Command_ID.CmdAddPeriodR, addPeriodR.ToByteString(), (res, content) => {
                    PopupManager.Close(maskId);
                    if(res == Command_Result.CmdNoError) {
                        CMD_Add_Period_a_Parameters periodA = CMD_Add_Period_a_Parameters.Parser.ParseFrom(content);
                        Period_Info periodInfo = periodA.PeriodInfo;
                        course_Info.PeriodList.Add(periodInfo.PeriodId, periodInfo);
                        course_Info.PeriodDisplayList.Add(periodInfo.PeriodId);

                        editingPeriodInfo = periodInfo;
                        UploadPerioditem();
                    } else {
                        PopupManager.Notice(res.Localize());
                    }
                });
            } else {
                editingPeriodInfo.PeriodName = periodName.text;
                editingPeriodInfo.PeriodDescription = periodDescription.text;
                editingPeriodInfo.PeriodFinsishCon = finishCondition;
                editingPeriodInfo.PeriodType = (uint)passModeType;

                CMD_Modify_Period_r_Parameters modifyPeriodR = new CMD_Modify_Period_r_Parameters();
                modifyPeriodR.PeriodInfo = editingPeriodInfo;
                modifyPeriodR.CourseId = course_Info.CourseId;

                int maskId = PopupManager.ShowMask();
                SocketManager.instance.send(Command_ID.CmdModifyPeriodR, modifyPeriodR.ToByteString(), (res, content) => {
                    PopupManager.Close(maskId);
                    if(res == Command_Result.CmdNoError) {
                        UploadPerioditem();
                    } else {
                        PopupManager.Notice(res.Localize());
                    }
                });
            }
        });       
    }

    void UploadPerioditem() {
        if(periodItems.Count == 0) {
            UploadingFinished();
            return;
        }
        PeriodItem pct = periodItems[0];
        periodItems.Remove(pct);

        if(pct.state == PeriodItem.State.EXISTING) {
            UploadPerioditem();
        } else if(pct.state == PeriodItem.State.NEW) {
            var perioditemR = new CMD_Add_Perioditem_r_Parameters();
            perioditemR.CourseId = course_Info.CourseId;
            perioditemR.PeriodId = editingPeriodInfo.PeriodId;
            perioditemR.PeriodItemInfo = pct.periodItem;
            perioditemR.PeriodFiles = new FileList();
            PackProject(perioditemR.PeriodFiles, pct);

            int maskId = PopupManager.ShowMask();
            SocketManager.instance.send(Command_ID.CmdAddPerioditemR, perioditemR.ToByteString(), (res, content) => {
                PopupManager.Close(maskId);
                if(res == Command_Result.CmdNoError) {
                    var periodItemA = CMD_Add_Perioditem_a_Parameters.Parser.ParseFrom(content);
                    editingPeriodInfo.PeriodItems.Add(periodItemA.PeriodItemInfo.ItemId, periodItemA.PeriodItemInfo);
                    editingPeriodInfo.PeriodItemDisplayList.Add(periodItemA.PeriodItemInfo.ItemId);

                    // update the item info, so we can know the download url
                    pct.periodItem = periodItemA.PeriodItemInfo;
                    UploadPerioditem();
                } else {
                    PopupManager.Notice(res.Localize());
                }
            });
        } else {  //修改
            UploadChangedItem(pct, () => {
                UploadPerioditem();
            });
        }
    }

    void UploadingFinished() {
        if(shouldModifyGameboard) {
            UploadGameboardCodeGroups();
        } else {
            CloseAndShowPeriodList();
        }
    }

    void UploadGameboardCodeGroups() {
        // reset gameboard code binding info
        gameboardItem.gameboard.ClearCodeGroups();
        var groups = gameboardItem.gameboard.GetCodeGroups(Preference.scriptLanguage);
        for(int i = 0; i < codeBindings.Count; ++i) {
            // the relative path
            var remotePath = Gameboard.ProjectUrl.ToRemote(codeBindings[i].periodItem.ItemId.ToString());
            // add the robot to the group
            var group = new Gameboard.RobotCodeGroupInfo(remotePath);
            group.Add(i);
            group.projectName = codeBindings[i].periodItem.ItemName;
            groups.Add(group);
        }

        UploadChangedItem(gameboardItem, () => {
            shouldModifyGameboard = false;
            CloseAndShowPeriodList();
        });
    }

    void CloseAndShowPeriodList() {
        gameboardItem = null;
        everyLessonContentGo.SetActive(false);
        Reset();
        SynchroShowPeriodInfo();
        scroll.refresh();
    }

    void UploadChangedItem(PeriodItem content, Action done = null) {
        var perioditemR = new CMD_Modify_Perioditem_r_Parameters();
        perioditemR.CourseId = course_Info.CourseId;
        perioditemR.PeriodId = editingPeriodInfo.PeriodId;
        perioditemR.PeriodItemInfo = content.periodItem;

        perioditemR.PeriodFiles = new FileList();
        PackProject(perioditemR.PeriodFiles, content);

        int maskId = PopupManager.ShowMask();
        SocketManager.instance.send(Command_ID.CmdModifyPerioditemR, perioditemR.ToByteString(), (res, data) => {
            PopupManager.Close(maskId);
            if(res == Command_Result.CmdNoError) {
                if(done != null) {
                    done();
                }
            } else {
                PopupManager.Notice(res.Localize());
            }
        });
    }

    void PackProject(FileList uploadFiles, PeriodItem item) {
        if(item.gameboard != null) {
            PackGameboard(uploadFiles, item.gameboard);
            if(item.state == PeriodItem.State.NEW) {
                PackProject(uploadFiles, GameboardRepository.instance.loadCodeProject(item.originalPath));
            }
        } else if(item.periodItem.eItemType == Period_Item_Type.ItemProject &&
                  item.state == PeriodItem.State.NEW) {
            if(Preference.scriptLanguage == ScriptLanguage.Visual) {
                PackProject(uploadFiles, CodeProjectRepository.instance.loadCodeProject(item.originalPath));
            } else {
                string dirPath = "";
                if(!string.IsNullOrEmpty(Path.GetDirectoryName(item.originalPath))) {
                    dirPath = Path.GetDirectoryName(item.originalPath) + "/";
                }
                uploadFiles.FileList_.AddRange(PythonRepository.instance.loadProjectFiles(item.originalPath).ToFileNodeList(dirPath));
            }
        }
    }

    void PackGameboard(FileList uploadFiles, Gameboard.Gameboard gameboard) {
        FileNode tGb = new FileNode();
        tGb.PathName = GameboardRepository.GameBoardFileName;
        tGb.FileContents = gameboard.Serialize().ToByteString();
        uploadFiles.FileList_.Add(tGb);
    }

    // #TODO should have a utility method to load all files in one call
    void PackProject(FileList uploadFiles, Project project) {
        if(project != null) {
            uploadFiles.FileList_.AddRange(project.ToFileNodeList(""));
        }
    }

    void Reset() {
        periodItems.Clear();
    }

    public bool OnBeforeToggleCodeBinding(PeriodItem item, bool isOn) {
        if(isOn) {
            if(codeBindings.Count == gameboardItem.gameboard.robots.Count) {
                PopupManager.Notice("ui_max_robot_code_selected".Localize(codeBindings.Count));
                return false;
            }
            codeBindings.Add(item);
            // sort bindings according to position in the item list
            codeBindings = codeBindings.Select(x => new { Index = periodItems.IndexOf(x), Item = x })
                                       .OrderBy(x => x.Index)
                                       .Select(x => x.Item)
                                       .ToList();
        } else {
            codeBindings.Remove(item);
        }

        shouldModifyGameboard = true;
        return true;
    }

    public bool IsCodeSelected(PeriodItem item) {
        return codeBindings.Contains(item);
    }

    public bool HasGameboard() {
        return gameboardItem != null;
    }

    public void OnTogglePassMode(int type) {
        if(toggleSubmit.isOn) {
            passModeType = PassModeType.Submit;
        } else {
            passModeType = PassModeType.Play;
        }
    }
}
