using Google.Protobuf;
using Google.Protobuf.Collections;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

using g_WebRequestManager = Singleton<WebRequestManager>;

public class PeriodItem {
    public enum State {
        NEW,
        CHANGED,
        EXISTING,
        DELETE
    }
    public State state;
    public Gameboard.Gameboard gameboard;
    public string originalPath;
    public Period_Item_Info periodItem;

    public static PeriodItem New(Period_Item_Info item)
    {
        return new PeriodItem {
            periodItem = item
        };
    }

    public static PeriodItem NewGameboard(string path, string name, Gameboard.Gameboard gameBoard)
    {
        return new PeriodItem {
            periodItem = new Period_Item_Info {
                eItemType = Period_Item_Type.ItemGb,
                ItemUrl = gameBoard.name,
                ItemName = name,
            },
            gameboard = gameBoard,
            originalPath = path,
        };
    }

    public static PeriodItem NewProject(string name, string path)
    {
        return new PeriodItem {
            periodItem = new Period_Item_Info {
                eItemType = Period_Item_Type.ItemProject,
                ItemName = name,
            },
            originalPath = path,
        };
    }
}

public enum PeriodOperation {
    NONE,
    DELETE
}
public class EveryLessonManager : MonoBehaviour {
    [SerializeField]
    private ScrollLoopController scroll;
    [SerializeField]
    private GameObject mainPanel;
    [SerializeField]
    private GameObject createEveryLessonGo;
    [SerializeField]
    private GameObject everyLessonContentGo;
    [SerializeField]
    private InputField periodName;
    [SerializeField]
    private InputField periodDescription;
    [SerializeField]
    private GameObject createGo;
    [SerializeField]
    private GameObject contentGo;
    [SerializeField]
    private SelectResType selectResType;
    [SerializeField]
    private GameObject periodItemGo;
    [SerializeField]
    private Transform periodItemParentGo;
    [SerializeField]
    private InputField threeStarText;
    [SerializeField]
    private InputField twoStarText;
    [SerializeField]
    private InputField passText;
    [SerializeField]
    private GameObject conditionGo;
    [SerializeField]
    private Text titleName;
    [SerializeField]
    private Text titleName1;
    [SerializeField]
    private Button btnNext;
    [SerializeField]
    private Button gameboardResButton;
    [SerializeField]
    private InputField inputPrice;

    public Button editPeriodNextButton;

    public GameObject CourseCreateGo;
    public Button[] disableModeBtns;
    public ButtonColorEffect btnBack;
    public GameObject btnCancle;
    public GameObject btnDel;

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
    void Awake()
    {
        periodName.onValueChanged.AddListener(delegate { UpdateEditPeriodNextButton(); });
    }

    private void UpdateEditPeriodNextButton()
    {
        editPeriodNextButton.interactable = periodName.text != "";
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
        bool isNone = (periodOperation == PeriodOperation.NONE);
        foreach (Button btn in disableModeBtns)
        {
            btn.interactable = isNone;
        }
        btnCancle.SetActive(!isNone);
        btnBack.interactable = isNone;
        disableModeBtns[type].interactable = true;
        foreach (Period_Info info in periodInfos)
        {
            info.periodOperation = periodOperation;
        }
        foreach (ScrollCell cell in scroll.GetCellsInUse())
        {
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
        btnDel.SetActive(periodInfos.Count != 0);
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
            if (res != Command_Result.CmdNoError)
            {
                PopupManager.Notice(res.Localize());
            }
        });
    }

    public void ClickAdd() {
        editingPeriodInfo = null;
        periodName.text = "";
        periodDescription.text = "";
        UpdateEditPeriodNextButton();
        createEveryLessonGo.SetActive(true);
    }

    public void ClickShowCreateLesson(Period_Info period) {
        editingPeriodInfo = period;
        periodName.text = period.PeriodName;
        periodDescription.text = period.PeriodDescription;
        UpdateEditPeriodNextButton();
        createEveryLessonGo.SetActive(true);
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
        createEveryLessonGo.SetActive(false);
        everyLessonContentGo.SetActive(true);

        contentGo.SetActive(course_Info.PeriodDisplayList.Count > 0);
        titleName1.text = periodName.text;

        ClearCodeBindings();

        if(editingPeriodInfo != null) {
            var gbItemInfo = editingPeriodInfo.GetGameboard();
            if (gbItemInfo != null)
            {
                DownloadGB(gbItemInfo.GbInfo.ProjPath);
            }
            else
            {
                InitPeriodItems(null);
            }
        }
        UpdateButtonState();
    }

    void ClearCodeBindings()
    {
        // initialize for code binding
        gameboardItem = null;
        codeBindings.Clear();
        shouldModifyGameboard = false;
    }

    private void InitPeriodItems(Gameboard.Gameboard gameboard)
    {
        for(int i = 0; i < editingPeriodInfo.PeriodItemDisplayList.Count; i++) {
            PeriodItem item = new PeriodItem();
            item.state = PeriodItem.State.EXISTING;
            item.periodItem = editingPeriodInfo.PeriodItems[editingPeriodInfo.PeriodItemDisplayList[i]];
            periodItems.Add(item);

            if (gameboard != null)
            {
                if (item.periodItem.eItemType == Period_Item_Type.ItemGb)
                {
                    item.gameboard = gameboard;
                    gameboardItem = item;
                }
                else if (item.periodItem.eItemType == Period_Item_Type.ItemProject)
                {
                    var path = Gameboard.ProjectUrl.ToRemote(item.periodItem.ItemId.ToString());
                    var groups = gameboard.GetCodeGroups(ScriptLanguage.Visual);
                    if (groups.GetGroup(path) != null)
                    {
                        codeBindings.Add(item);
                    }
                }
            }
        }

        foreach (var item in periodItems)
        {
            InstantiatePeriodItem(item);
        }
    }

    private void DownloadGB(string path)
    {
        var request = new ProjectDownloadRequest();
        request.basePath = path;
        request.preview = true;
        request.blocking = true;
        request.Success(dir => {
                var gameboard = dir.GetGameboard();
                if (gameboard == null)
                {
                    Debug.LogError("invalid gameboard");
                }
                InitPeriodItems(gameboard);
            })
            .Error(() => InitPeriodItems(null))
            .Finally(UpdateButtonState)
            .Execute();
    }

    public void OnClickAddRes() {
        selectResType.ListenResData(SelectResBack);
        selectResType.gameObject.SetActive(true);
    }

    public void OnClickAddGameBoard() {
        selectResType.Close();

        PopupGameBoardSelect.ConfigureParameter configure = new PopupGameBoardSelect.ConfigureParameter();
        configure.visibleType = PopupGameBoardSelect.VisibleType.All;
        configure.selectCallBack = (path) => {
           // var gameboard = GameboardRepository.instance.getGameboard(path.ToString());
           // if (gameboard != null)
           // {
           //     gameboard.ClearCodeGroups();
           ////     var item = PeriodItem.NewGameboard(path, gameboard);
           //     AddPeriodItem(item);

           //     gameboardItem = item;
           // }
        };
        PopupManager.GameBoardSelect(configure);
    }

    public void OnClickAddProject() {
        selectResType.Close();
        PopupManager.ProjectView((fileOrDirPath)=> {
          //  AddPeriodItem(PeriodItem.NewProject(fileOrDirPath));
        } ,showDeleteBtn:false, showAddCell:false);
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
        if(data.resType == ResType.Course && !Utils.IsValidUrl(data.name)) {
            periodInfo.ItemUrl = Singleton<WebRequestManager>.instance.GetMediaPath(data.name, false);
        } else {
            periodInfo.ItemUrl = data.name;
        }
        AddPeriodItem(PeriodItem.New(periodInfo));
    }

    void AddPeriodItem(PeriodItem contentItem)
    {
        periodItems.Add(contentItem);

        contentGo.SetActive(true);
        createGo.SetActive(false);

        InstantiatePeriodItem(contentItem);
        UpdateButtonState();
    }

    void InstantiatePeriodItem(PeriodItem pct) {
        Instantiate(periodItemGo, periodItemParentGo.transform);
    }

    public void DeleteItem(LessonContentItem item) {
        if(editingPeriodInfo != null) {
            PeriodItem pct = item.GetContent();
            Period_Item_Info periodItemInfo;
            editingPeriodInfo.PeriodItems.TryGetValue(pct.periodItem.ItemId , out periodItemInfo);
            if(periodItemInfo != null) {
                CMD_Del_Perioditem_r_Parameters delPeriodItem = new CMD_Del_Perioditem_r_Parameters();
                delPeriodItem.CourseId = course_Info.CourseId;
                delPeriodItem.PeriodId = editingPeriodInfo.PeriodId;
                delPeriodItem.PerioditemId = periodItemInfo.ItemId;
                int maskId = PopupManager.ShowMask();
                SocketManager.instance.send(Command_ID.CmdDelPerioditemR, delPeriodItem.ToByteString(), (res, content) => {
                    PopupManager.Close(maskId);
                    if(res == Command_Result.CmdNoError) {
                        editingPeriodInfo.PeriodItems.Remove(pct.periodItem.ItemId);
                        editingPeriodInfo.PeriodItemDisplayList.Remove(pct.periodItem.ItemId);

                        DeleteItemInternal(item);
                    } else {
                        PopupManager.Notice(res.Localize());
                    }
                });
            } else {
                DeleteItemInternal(item);
            }
        } else {
            DeleteItemInternal(item);
        }
    }

    void DeleteItemInternal(LessonContentItem item)
    {
        var pct = item.GetContent();
        if (pct.periodItem.eItemType == Period_Item_Type.ItemGb)
        {
            ClearCodeBindings();
        }
        else if (pct.periodItem.eItemType == Period_Item_Type.ItemProject)
        {
            codeBindings.Remove(pct);
            // no need to upload for a new gameboard
            if (gameboardItem != null &&
                gameboardItem.state != PeriodItem.State.NEW &&
                IsCodeSelected(pct))
            {
                UploadGameboardCodeGroups();
            }
        }

        periodItems.Remove(pct);
        UpdateButtonState();
        Destroy(item.gameObject);
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
        if(selectResType.gameObject.activeSelf) {
            selectResType.Close();
            return;
        }
        
        mainPanel.SetActive(true);
        createEveryLessonGo.SetActive(true);
        everyLessonContentGo.SetActive(false);
        Reset();
    }

    public void OnClickNextCondition() {
        foreach(PeriodItem content in periodItems) {
            if(string.IsNullOrEmpty(content.periodItem.ItemName)) {
                string noticeStr = "";
                if((Period_Item_Type)content.periodItem.ItemType == Period_Item_Type.ItemDoc) {
                    noticeStr = string.Format("ui_text_name_no_empty".Localize(), "PDF");
                } else if((Period_Item_Type)content.periodItem.ItemType == Period_Item_Type.ItemImage) {
                    noticeStr = string.Format("ui_text_name_no_empty".Localize(), "ui_task_type_image".Localize());
                } else if((Period_Item_Type)content.periodItem.ItemType == Period_Item_Type.ItemVideo) {
                    noticeStr = string.Format("ui_text_name_no_empty".Localize(), "ui_task_type_video".Localize());
                } else if((Period_Item_Type)content.periodItem.ItemType == Period_Item_Type.ItemProject) {
                    noticeStr = string.Format("ui_text_name_no_empty".Localize(), "ui_program".Localize());
                } else {
                    noticeStr = string.Format("ui_text_name_no_empty".Localize(), "ui_text_gameboard".Localize());
                }

                PopupManager.Notice(noticeStr);
                return;
            }
            if((Period_Item_Type)content.periodItem.ItemType == Period_Item_Type.ItemGb) {
                string validateResult = ProjectNameValidator.Validate(GameboardRepository.instance, content.periodItem.ItemName);
                if(validateResult != null) {
                    PopupManager.Notice(validateResult);
                    return;
                }
            }
        }

        if(editingPeriodInfo != null && editingPeriodInfo.PeriodFinsishCon != null) {
            threeStarText.text = editingPeriodInfo.PeriodFinsishCon.ThreestarScore.ToString();
            twoStarText.text = editingPeriodInfo.PeriodFinsishCon.DoublestarScore.ToString();
            passText.text = editingPeriodInfo.PeriodFinsishCon.PassScore.ToString();
        } else {
            threeStarText.text = "";
            twoStarText.text = "";
            passText.text = "";
        }

        conditionGo.SetActive(true);
    }

    public void OnClickFinish() {
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

    void UploadingFinished()
    {
        if (shouldModifyGameboard)
        {
            UploadGameboardCodeGroups();
        }
        else
        {
            CloseAndShowPeriodList();
        }
    }

    void UploadGameboardCodeGroups()
    {
        // reset gameboard code binding info
        gameboardItem.gameboard.ClearCodeGroups();
        var groups = gameboardItem.gameboard.GetCodeGroups(ScriptLanguage.Visual);
        for (int i = 0; i < codeBindings.Count; ++i)
        {
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

    void CloseAndShowPeriodList()
    {
        everyLessonContentGo.SetActive(false);
        conditionGo.SetActive(false);
        Reset();
        SynchroShowPeriodInfo();
        scroll.refresh();
    }

    void UploadChangedItem(PeriodItem content, Action done = null)
    {
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
                if (done != null)
                {
                    done();
                }
            } else {
                PopupManager.Notice(res.Localize());
            }
        });
    }

    void PackProject(FileList uploadFiles, PeriodItem item)
    {
        if(item.gameboard != null) {
            PackGameboard(uploadFiles, item.gameboard);
            if (item.state == PeriodItem.State.NEW)
            {
                PackProject(uploadFiles, GameboardRepository.instance.loadCodeProject(item.originalPath));
            }
        } else if(item.periodItem.eItemType == Period_Item_Type.ItemProject &&
                  item.state == PeriodItem.State.NEW) {
            PackProject(uploadFiles, CodeProjectRepository.instance.loadCodeProject(item.originalPath));
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
        if (project != null) {
            uploadFiles.FileList_.AddRange(project.ToFileNodeList(""));
        }
    }

    public void OnClickConditonBack() {
        conditionGo.SetActive(false);
    }

    void Reset() {
        periodItems.Clear();
        UpdateButtonState();

        for(int i = 0; i < periodItemParentGo.childCount; i++) {
            Destroy(periodItemParentGo.GetChild(i).gameObject);
        }
    }

    void UpdateButtonState() {
        var hasItems = periodItems.Count != 0;
        btnNext.gameObject.SetActive(hasItems);
        createGo.SetActive(!hasItems);

        var hasGb = periodItems.Any(x => x.periodItem.eItemType == Period_Item_Type.ItemGb);
        btnNext.interactable = hasGb;
        gameboardResButton.interactable = !hasGb;
    }

    public bool OnBeforeToggleCodeBinding(PeriodItem item, bool isOn)
    {
        if (isOn)
        {
            if (codeBindings.Count == gameboardItem.gameboard.robots.Count)
            {
                PopupManager.Notice("ui_max_robot_code_selected".Localize(codeBindings.Count));
                return false;
            }
            codeBindings.Add(item);
            // sort bindings according to position in the item list
            codeBindings = codeBindings.Select(x => new { Index = periodItems.IndexOf(x), Item = x })
                                       .OrderBy(x => x.Index)
                                       .Select(x => x.Item)
                                       .ToList();
        }
        else
        {
            codeBindings.Remove(item);
        }

        shouldModifyGameboard = true;
        return true;
    }

    public bool IsCodeSelected(PeriodItem item)
    {
        return codeBindings.Contains(item);
    }

    public bool HasGameboard()
    {
        return gameboardItem != null;
    }
}
