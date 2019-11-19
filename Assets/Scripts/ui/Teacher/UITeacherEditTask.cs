using Gameboard;
using Google.Protobuf;
using Google.Protobuf.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using g_WebRequestManager = Singleton<WebRequestManager>;

public delegate void PoolTaskEditCallback(TaskPoolOperation op, TaskTemplate task);

public class UITeacherEditTask : PopupController {
    public class Payload {
        public WorkMode workMode;
        public Action refreshCallBack;
        public uint editorTaskId;
        public TaskTemplate task;
        public UITeacherTaskPool teacherTaskPool;
        public PoolTaskEditCallback poolTaskUpdated;
    }

    public Text m_TaskName;
    public InputField m_TaskNameInput;
    public Text m_Description;
    public InputField m_DescriptionInput;
    public GameObject m_ClassificationPanel;
    public ScrollLoopController attachmentScroll;
    public GameObject importBtn;
    public GameObject dropDownGo;
    public GameObject classifyTitle;
    public Toggle togMine;
    public GameObject publishBtn;
    public GameObject publishSysBtn;
    public Toggle prohibitSubmitTog;
    private bool initProhibitSumit;
    public enum WorkMode {
        Create_Mode,
        Edit_Mode,
        Preview_Mode,
        Preview_Pool_Mode,
        Create_Pool_mode,
        Edit_Pool_Mode,
        Edit_Sys_Pool
    }

    public WorkMode m_Mode { get; set; }
    public readonly List<AttachData> attachDatas = new List<AttachData>();
    private Action m_refreshCallBack;

    private TaskTemplate poolTaskCellData;
    private UITeacherTaskPool teacherTaskPool;

    public GameObject createOrEditorPanel;
    public GameObject previewAttrributePanel;
    public Button CreateEditorBtn;
    public GameObject gradeTask;
    public Text m_ClassificationText;
    public GameObject editorBtn;

    [SerializeField]
    private GameObject m_ClassificationMenu;

    uint m_TaskID;
    
    private Payload payLoadData;

    public int classificationId { get; set; }
    public PoolTaskEditCallback m_poolTaskUpdated { get; set; }
    public string descriptionInfo {
        get { return m_DescriptionInput.text; }
    }
    public ClassInfo curClass {
        get { return UserManager.Instance.CurClass; }
    }
    public uint editorTaskId {
        get { return payLoadData.editorTaskId; }
    }
    public TaskInfo curTaskInfo {
        get { return UserManager.Instance.CurClass.GetTask(m_TaskID); }
    }

    protected override void Start() {
        base.Start();

        payLoadData = (Payload)payload;
        m_Mode = payLoadData.workMode;
        m_refreshCallBack = payLoadData.refreshCallBack;
        m_poolTaskUpdated = payLoadData.poolTaskUpdated;
        teacherTaskPool = payLoadData.teacherTaskPool;
        poolTaskCellData = payLoadData.task;
        if(m_Mode == WorkMode.Create_Mode) {
            InitCreateClassTask();
        } else if(m_Mode == WorkMode.Edit_Mode) {
            InitClassTask(payLoadData.editorTaskId);
        } else if(m_Mode == WorkMode.Create_Pool_mode) {
            InitNewPoolTask(payLoadData.teacherTaskPool != null);
        } else if(m_Mode == WorkMode.Edit_Pool_Mode) {
            InitEditPoolTask(payLoadData.task);
        }
        initProhibitSumit = prohibitSubmitTog.isOn;
    }

    public void InitCreateClassTask() {
        m_Mode = WorkMode.Create_Mode;
        InitCreateMode();
    }
    public void InitNewPoolTask(bool teacherPool) {
        m_Mode = WorkMode.Create_Pool_mode;
        poolTaskCellData = new TaskTemplate() {
            type = teacherPool ? TaskTemplateType.User : TaskTemplateType.System
        };
        InitCreateMode();
    }

    void InitCreateMode() {
        attachDatas.Clear();
        SetOperationMode(WorkMode.Create_Mode);
        m_ClassificationPanel.SetActive(poolTaskCellData != null);
        m_TaskName.text = "";
        m_Description.text = "";
        m_TaskNameInput.text = "";
        m_DescriptionInput.text = "";
        InitAttachScroll(true);
        SetClassification(0);
    }

    void SetOperationMode(WorkMode workMode) {
        prohibitSubmitTog.gameObject.SetActive(false);
        bool isEditor = (workMode == WorkMode.Edit_Mode || workMode == WorkMode.Create_Mode);

        m_TaskName.gameObject.SetActive(!isEditor);
        m_TaskNameInput.gameObject.SetActive(isEditor);
        classifyTitle.SetActive(workMode == WorkMode.Preview_Mode && m_Mode == WorkMode.Edit_Mode);
        importBtn.SetActive(m_Mode == WorkMode.Create_Mode && poolTaskCellData == null);
        dropDownGo.SetActive(isEditor);

        m_Description.gameObject.SetActive(!isEditor);
        m_DescriptionInput.gameObject.SetActive(isEditor);

        switch(workMode) {
            case WorkMode.Create_Mode:
            case WorkMode.Edit_Mode:
                createOrEditorPanel.SetActive(true);
                previewAttrributePanel.SetActive(false);
                prohibitSubmitTog.gameObject.SetActive(true && (m_Mode == WorkMode.Create_Mode || m_Mode == WorkMode.Edit_Mode));
                break;
            case WorkMode.Preview_Mode:
                createOrEditorPanel.SetActive(false);
                previewAttrributePanel.SetActive(true);
                if(m_Mode == WorkMode.Edit_Pool_Mode && poolTaskCellData.type == TaskTemplateType.User) {
                    publishBtn.SetActive(true);
                    publishSysBtn.SetActive(UserManager.Instance.IsAdmin);
                } else {
                    publishBtn.SetActive(false);
                    publishSysBtn.SetActive(false);
                }
                break;
        }
        CheckActiveSaveBtn();
    }

    public void CheckActiveSaveBtn() {
        CreateEditorBtn.interactable = taskNameInputText != "";
    }

    public void OnClickAddAttach() {
        PopupManager.AttachmentManager(attachDatas, (upload) => {
            upload();
        }, () => {
            InitAttachScroll(true);
            CheckActiveSaveBtn();
        }, gameboardCount:PopupAttachmentManager.MaxAttachCount);
    }

    public void ClickConfirm() {
        switch(m_Mode) {
        case WorkMode.Create_Mode:
                GetClassTaskMode().CreateTask(()=> {
                    m_refreshCallBack();
                    Close();
                });
            break;
        case WorkMode.Edit_Mode:
                GetClassTaskMode().EditConfirm(() => {
                    m_refreshCallBack();
                    Close();
                });
            break;
        case WorkMode.Create_Pool_mode:
        case WorkMode.Edit_Pool_Mode:
                GetClassTaskMode().UploadPoolTask(poolTaskCellData, () => {
                    Close();
                });
            break;
        }
    }



    public void InitClassTask(uint taskId) {
        SetOperationMode(WorkMode.Preview_Mode);
        m_TaskID = taskId;

        attachDatas.Clear();
        string webPath = TaskCommon.GetTaskPath(
                UserManager.Instance.CurClass.m_ID,
                taskId,
                UserManager.Instance.CurClass.teacherId);
        AddResources(curTaskInfo.attachs, webPath);
        InitAttachScroll(false);
        m_ClassificationPanel.SetActive(false);
        m_TaskName.text = curTaskInfo.m_Name;
        prohibitSubmitTog.isOn = !curTaskInfo.prohibitSubmit;
        m_Description.text = curTaskInfo.m_Description;
    }

    public void SwitchEditorClassTask() {
        m_TaskNameInput.text = m_TaskName.text;
        m_DescriptionInput.text = m_Description.text;
        SetOperationMode(WorkMode.Edit_Mode);
        InitAttachScroll(true);
    }

    public void InitEditPoolTask(TaskTemplate task) {
        m_Mode = WorkMode.Edit_Pool_Mode;
        SetOperationMode(WorkMode.Preview_Mode);
        m_ClassificationPanel.SetActive(true);
        SetClassification((int)poolTaskCellData.level);
        m_TaskName.text = poolTaskCellData.name;
        m_Description.text = poolTaskCellData.description;

        attachDatas.Clear();

        string webPath = TaskCommon.GetTemplate(
                UserManager.Instance.CurClass.teacherId,
                (uint)poolTaskCellData.level,
                poolTaskCellData.name,
                poolTaskCellData.type);

        AddResources(poolTaskCellData.attachs, webPath);
        InitAttachScroll(false);
        editorBtn.SetActive(UserManager.Instance.IsAdmin || poolTaskCellData.type == TaskTemplateType.User);
    }

    void InitAttachScroll(bool showAdd) {
        if(showAdd) {
            if(attachDatas.Count == 0 || attachDatas[attachDatas.Count - 1] != null) {
                attachDatas.Add(null);
            }
        } else {
            attachDatas.Remove(null);
        }
        var effectiveAttach = attachDatas.FindAll(x => { return x == null || x.state != AttachData.State.Delete; });
        attachmentScroll.initWithData(effectiveAttach.Select(x => {
            return new AddAttachmentCellData(x, effectiveAttach.Where(y => y != null));
        }).ToArray());
    }
    
    private void SetClassification(int index) {
        classificationId = index;
        if(index >= 0 && index < 5) {
            m_ClassificationText.text = "BL" + (index + 1);
        } else {
            m_ClassificationText.text = "task_pool_others".Localize();
        }
    }

    void AddResources(K8_Attach_Info attachs, string webPath) {
        if(attachs == null) {
            return;
        }
        attachDatas.AddRange(K8AttachAndAttachSwitch.ToAttach(attachs, webPath));
        var gamboardAttach = attachDatas.Find((x) => { return x != null && x.type == AttachData.Type.Gameboard && x.state != AttachData.State.Delete; });
        if(gamboardAttach != null) {
            var gameboard = gamboardAttach.gameboard;
            if(gameboard != null) {
                var groups = gameboard.GetCodeGroups(Preference.scriptLanguage);
                foreach(var g in groups) {
                    int relationId = 0;
                    if(int.TryParse(ProjectUrl.GetPath(g.projectPath), out relationId)) {
                        var pro = attachDatas.Find((x) => { return x != null && x.id == relationId && x.state != AttachData.State.Delete; });
                        if(pro != null) {
                            pro.isRelation = true;
                        }
                    }
                }
            }
        }
    }

    public void OnClickImportPool() {
        PopupManager.SelectPoolTask((selectedTaskInfo) => {
            m_TaskNameInput.text = selectedTaskInfo.taskInfo.m_Name;
            m_DescriptionInput.text = selectedTaskInfo.taskInfo.m_Description;
            attachDatas.Clear();
            AddResources(selectedTaskInfo.taskInfo.attachs, selectedTaskInfo.webPath);
            InitAttachScroll(true);
            CheckActiveSaveBtn();
        });
    }

    public string taskNameInputText{
        get { return m_TaskNameInput.text.TrimEnd(); }
    }

    public void OnClickPreView(){
        var project = new Project(poolTaskCellData.codeProject);
        SceneDirector.Push("Main", CodeSceneArgs.FromTempCode(project));
    }

    public void OnClickClassification(int index)
    {
        m_ClassificationMenu.SetActive(false);
        SetClassification(index);
    }
    public void OnClickGrade() {
        CMD_Get_Task_Info_r_Parameter tRequestTask = new CMD_Get_Task_Info_r_Parameter();
        tRequestTask.ClassId = UserManager.Instance.CurClass.m_ID;
        tRequestTask.TaskId = m_TaskID;
        int popupId = PopupManager.ShowMask();
        SocketManager.instance.send(Command_ID.CmdGetTaskInfoR, tRequestTask.ToByteString(), (res, content)=> {
            PopupManager.Close(popupId);
            if(res == Command_Result.CmdNoError) {
                var response = CMD_Get_Task_Info_a_Parameter.Parser.ParseFrom(content);

                ClassInfo classInfo = UserManager.Instance.CurClass;
                A8_Task_Info tTaskformServer = response.TaskList[0];
                TaskInfo tCurTask = classInfo.GetTask(tTaskformServer.TaskId);
                tCurTask.UpdateSubmit(tTaskformServer.TaskSubmitInfo);
                UserManager.Instance.CurTask = tCurTask;

                gradeTask.SetActive(true);
            } else {
                PopupManager.Notice(res.Localize());
            }
        });
    }
    public void OnClickMine() {
        if(gradeTask.activeSelf) {
            gradeTask.GetComponent<UITeacherGradeTask>().ClickReturn();
        }
    }
    public void OnClickPublish() {
        teacherTaskPool.OnClickRelease(poolTaskCellData);
    }
    public void OnClickPublishSys() {
        teacherTaskPool.UpLoadToSystem(poolTaskCellData);
    }
    ClassTaskModeTea GetClassTaskMode() {
        ClassTaskModeTea classTaskMode = null;
        if(Preference.scriptLanguage == ScriptLanguage.Visual) {
            var gameboardRes = attachDatas.Find(x => { return x != null && x.type == AttachData.Type.Gameboard && 
                x.state != AttachData.State.Delete; });
            if(gameboardRes == null) {
                classTaskMode = new ClassTaskGraphMode(this);
            } else {
                classTaskMode = new ClassTaskGbMode(this);
            }
        } else {
            if(classTaskMode == null) {
                classTaskMode = new ClassTaskPythonMode(this);
            }
        }
        return classTaskMode;
    }

    public void OnClickClose() {
        if(!createOrEditorPanel.activeSelf || (m_Mode == WorkMode.Create_Mode || m_Mode == WorkMode.Create_Pool_mode)) {
            OnCloseButton();
            return;
        }

        if(string.IsNullOrEmpty(taskNameInputText)) {
            PopupManager.Notice("ui_video_empty_title_warning".Localize());
            return;
        }

        var editorCell = attachDatas.Find((x)=> { return x != null && x.state != AttachData.State.Initial; });
        if(editorCell != null || initProhibitSumit != prohibitSubmitTog.isOn) {
            PopupManager.TwoBtnDialog("text_save_editor_tak".Localize(), "text_donot_save".Localize(), () => {
                OnCloseButton();
            }, "ui_wheel_balance_save".Localize(), () => {
                ClickConfirm();
            });
        } else {
            OnCloseButton();
        }
    }
}
