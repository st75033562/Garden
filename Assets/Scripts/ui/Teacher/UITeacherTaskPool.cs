using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public enum TaskOperationType {
    NONE,
    DELETE,
    PUBLISH,
    UPLOADSYS,
    SELECT
}

public enum TaskPoolOperation {
    Update,
    Add,
    Remove
}

public class UITeacherTaskPool : MonoBehaviour {
    [SerializeField]
    private ScrollableAreaController scrollControllers;
    [SerializeField]
    private UITeacherEditTask m_UIEdit;
    [SerializeField]
    private Button[] buttons;
    public GameObject upload2SysGo;
    public GameObject cancleGo;
    public Button[] disableBtns;
    public ButtonColorEffect backBtnColorEffect;
    public GameObject btnAddGo;
    public GameObject[] hideBtns;

    private List<TaskTemplate> taskPools;
    private TaskOperationType taskOperation;
    private TaskCategory curCategory = TaskCategory.ALL;
    private List<TaskTemplate> taskPoolsInShow;
    private Action<SelectPoolTaskInfo> selectTaskInfo;

    public TaskOperationType curOperation {
        get { return taskOperation; }
    }

    public bool isEditing
    {
        get { return TaskOperationType.NONE < taskOperation && taskOperation < TaskOperationType.SELECT; }
    }

    public TaskService service { get; set; }

    public UISortMenuWidget uiSortMenuWidget;

    private UISortSetting sortSetting;

    public enum SortType {
        Name,
        CreateTime,
        UpdateTime
    }
    private static readonly string[] s_sortOptions = {
        "ui_single_pk_sort_name",
        "ui_single_pk_sort_creation_time",
        "ui_modified_time"
    };
    void Awake() {
        if(uiSortMenuWidget != null) {
            sortSetting = (UISortSetting)UserManager.Instance.userSettings.
                        Get(TeacherPoolSortSetting.keyName, true);
            uiSortMenuWidget.SetOptions(s_sortOptions.Select(x => x.Localize()).ToArray());
        }
    }

    void OnEnable() {
        service = new TaskService();
        taskPools = NetManager.instance.taskPools;
        scrollControllers.context = this;

        RefreshListView();
        UpdateUI();
    }

    private void UpdateUI() {
        UpdateCategoryButtons();
        UpdateButtons();
    }

    void UpdateButtons() {
        bool isEmpty = scrollControllers.model.count == 0;
        if (btnAddGo)
        {
            btnAddGo.SetActive(isEmpty && taskOperation == TaskOperationType.NONE);
        }
        foreach(GameObject go in hideBtns) {
            go.SetActive(!isEmpty);
        }

        cancleGo.SetActive(isEditing);
        for(int i = 0; i < disableBtns.Length; ++i) {
            disableBtns[i].interactable = i == (int)taskOperation || !isEditing;
        }
        backBtnColorEffect.interactable = !isEditing;

        if(upload2SysGo) {
            upload2SysGo.SetActive(UserManager.Instance.IsAdmin);
        }
    }

    void UpdateCategoryButtons() {
        for(int i = 0; i < buttons.Length; i++) {
            buttons[i].interactable = (int)curCategory != i;
        }
    }

    public void ClickCreate() {
        PopupManager.EditorTask(UITeacherEditTask.WorkMode.Create_Pool_mode, teacherTaskPool: this, poolTaskUpdated: ChangeList);
    }

    public void ChangeList(TaskPoolOperation state, TaskTemplate template) {
        if(state == TaskPoolOperation.Update) {
            scrollControllers.updateCellData(template);
        } else if(state == TaskPoolOperation.Add) {
            taskPools.Add(template);
        } else {
            taskPools.Remove(template);
        }

        RefreshListView();
        UpdateUI();
    }

    public void ClickEditor(TaskTemplate template) {
        PopupManager.EditorTask(UITeacherEditTask.WorkMode.Edit_Pool_Mode, task: template, teacherTaskPool:this, poolTaskUpdated: ChangeList);
    }

    public void OnClick(int index) {
        curCategory = (TaskCategory)index;
        RefreshListView();
        UpdateUI();
    }

    private void RefreshListView() {
        switch(curCategory) {
            case TaskCategory.ALL:  //all
                Refresh(taskPools);
                break;
            case TaskCategory.BL1:
            case TaskCategory.BL2:
            case TaskCategory.BL3:
            case TaskCategory.BL5:
            case TaskCategory.BL4:
            case TaskCategory.OTHERS: {
                    var tasks = taskPools.FindAll(x => x.level == curCategory);
                    Refresh(tasks);
                    break;
                }
        }
    }

    public void OnClickBack() {
        curCategory = TaskCategory.ALL;
        gameObject.SetActive(false);
    }

    public void OnClickOperation(int type) {
        taskOperation = (TaskOperationType)type;
        scrollControllers.Refresh();
        UpdateUI();
    }

    void Refresh(List<TaskTemplate> templates) {
        taskPoolsInShow = templates;
        if(uiSortMenuWidget != null) {
            uiSortMenuWidget.onSortChanged.RemoveListener(OnSortChanged);
            uiSortMenuWidget.SetCurrentSort(sortSetting.sortKey, sortSetting.ascending);
            uiSortMenuWidget.onSortChanged.AddListener(OnSortChanged);
            var comparer = TaskTemplateComparison.Get((TaskTemplateSortType)sortSetting.sortKey, sortSetting.ascending);
            if(comparer != null) {
                templates.Sort(comparer);
            }
        }
        scrollControllers.InitializeWithData(templates);
    }

    void OnSortChanged() {
        sortSetting.SetSortCriterion(uiSortMenuWidget.activeSortOption, uiSortMenuWidget.sortAsc);
        Refresh(taskPoolsInShow);
    }

    
    public void SelectPoolCell(Action<SelectPoolTaskInfo> taskInfo) {
        taskOperation = TaskOperationType.SELECT;
        selectTaskInfo = taskInfo;
        gameObject.SetActive(true);
    }

    public void SelectCell(TaskInfo taskInfo ,string webPath) {
        if(selectTaskInfo != null) {
            gameObject.SetActive(false);
            if(taskInfo != null) {
                selectTaskInfo(new SelectPoolTaskInfo(taskInfo, webPath));
            }
            selectTaskInfo = null;
            taskOperation = TaskOperationType.NONE;
        }
    }

    public void OnClickRelease(TaskTemplate template) {
        if(UserManager.Instance.CurClass.GetTask(template.name) != null) {
            PopupManager.Notice("repat_name_need_change".Localize(), null);
            return;
        }

        confirmRelease(template);
    }

    public void confirmRelease(TaskTemplate template) {
        string path;
        if(Preference.scriptLanguage == ScriptLanguage.Visual) {
            path = HttpCommon.c_tasktemplateV3 + UserManager.Instance.UserId + "/" + template.id;
        } else {
            path = HttpCommon.c_tasktemplatePyV3 + UserManager.Instance.UserId + "/" + template.id;
        }

        service.PublishTaskToClass(path, UserManager.Instance.CurClass.m_ID,
            (res, taskInfo) => {
                if(res == Command_Result.CmdNoError) {
                    UserManager.Instance.CurClass.AddTask(taskInfo);
                    PopupManager.Notice("ui_published_sucess".Localize());
                } else {
                    PopupManager.Notice(res.Localize());
                }
            });
    }

    public void UpLoadToSystem(TaskTemplate template) {
        bool duplicate = NetManager.instance.SameNameInSysPool(template.name);
        if(duplicate) {
            PopupManager.Notice("repat_name_need_change".Localize(), null);
            return;
        } else {
            AddTemplateToSys(template);
        }
    }

    void AddTemplateToSys(TaskTemplate template) {
        var copyRequest = new CopyRequest();
        if(Preference.scriptLanguage == ScriptLanguage.Visual) {
            copyRequest.rootType = FileList_Root_Type.TaskSystemGraphy;
            copyRequest.projectSrc = HttpCommon.c_tasktemplateV3 + UserManager.Instance.UserId + "/" + template.id;
        } else {
            copyRequest.rootType = FileList_Root_Type.TaskSystemPython;
            copyRequest.projectSrc = HttpCommon.c_tasktemplatePyV3 + UserManager.Instance.UserId + "/" + template.id;
        }

        copyRequest.desTag = ((int)template.level).ToString();
        copyRequest.desName = template.name;

        copyRequest.Success((t) => {
            var sysTemplate = new TaskTemplate(template);
            sysTemplate.type = TaskTemplateType.System;

            NetManager.instance.CoverOrAddToSysPool(sysTemplate);
            PopupManager.Notice("ui_success_save_to_sys".Localize());
        })
            .Execute();
    }


}
