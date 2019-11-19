using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Google.Protobuf;

using g_WebRequestManager = Singleton<WebRequestManager>;
using UnityEngine.UI;
using System.Linq;

public enum SysPoolOpertionType {
    NONE,
    DELETE,
    PUBLISH
}

public class UISystemTaskPool : MonoBehaviour {
    [SerializeField]
    private ScrollableAreaController scrollControllers;
    [SerializeField]
    private UITeacherEditTask m_UIEdit;
    [SerializeField]
    private Button[] buttons;
    public GameObject delGo;
    public GameObject addGo;
    public GameObject cancleGo;
    public Button[] disableBtns;
    public ButtonColorEffect backBtnColorEffect;
    public GameObject btnAddGo;
    public GameObject btnPublishGo;

    private List<TaskTemplate> taskPools;
    private SysPoolOpertionType sysOperation;
    private TaskCategory curCategory = TaskCategory.ALL;

    public UISortMenuWidget uiSortMenuWidget;

    private UISortSetting sortSetting;
    private List<TaskTemplate> taskPoolsInShow;

    public enum SortType {
        Name,
        CreateTime,
        UpdateTime
    }
    private static readonly string[] s_sortOptionsAdmin = {
        "ui_single_pk_sort_name",
        "ui_single_pk_sort_creation_time",
        "ui_modified_time",
    };

    private static readonly string[] s_sortOptions = {
        "ui_single_pk_sort_name",
        "ui_single_pk_sort_creation_time",
    };

    public SysPoolOpertionType operationType
    {
        get { return sysOperation; }
    }

    public TaskService service { get; set; }

    void Awake() {
        sortSetting = (UISortSetting)UserManager.Instance.userSettings.
            Get(SystemPoolSortSetting.keyName, true);
        if(UserManager.Instance.IsAdmin) {
            uiSortMenuWidget.SetOptions(s_sortOptionsAdmin.Select(x => x.Localize()).ToArray());
        } else {
            uiSortMenuWidget.SetOptions(s_sortOptions.Select(x => x.Localize()).ToArray());
        }
    }
    void OnEnable()
    {
        service = new TaskService();
        taskPools = NetManager.instance.sysTaskPools;
        scrollControllers.context = this;
        addGo.SetActive(UserManager.Instance.IsAdmin);

        RefreshListView();
        UpdateUI();
    }

    void UpdateUI()
    {
        UpdateCategoryButtons();
        UpdateButtons();
    }

    void UpdateCategoryButtons()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].interactable = (int)curCategory != i;
        }
    }

    void UpdateButtons() {
        bool isEmpty = scrollControllers.model.count == 0;
        btnAddGo.SetActive(isEmpty && sysOperation == SysPoolOpertionType.NONE && UserManager.Instance.IsAdmin);
        btnPublishGo.SetActive(!isEmpty);
        delGo.SetActive(!isEmpty && UserManager.Instance.IsAdmin);

        var isEditing = (SysPoolOpertionType)sysOperation != SysPoolOpertionType.NONE;
        cancleGo.SetActive(isEditing);
        for (int i = 0; i < disableBtns.Length; ++i)
        {
            disableBtns[i].interactable = i == (int)sysOperation || !isEditing;
        }
        backBtnColorEffect.interactable = !isEditing;
    }

    public void ClickCreate()
    {
        PopupManager.EditorTask(UITeacherEditTask.WorkMode.Create_Pool_mode, poolTaskUpdated: ChangeList);
      //  m_UIEdit.InitNewPoolTask(false, ChangeList);
      //  StackUIBase.Push(m_UIEdit, false);
    }

    public void ClickEditor(TaskTemplate template)
    {
       // StackUIBase.Push(m_UIEdit, false);
      //  m_UIEdit.InitEditPoolTask(template, ChangeList);
        PopupManager.EditorTask(UITeacherEditTask.WorkMode.Edit_Pool_Mode,task: template, poolTaskUpdated: ChangeList);
    }

    public void ChangeList(TaskPoolOperation state, TaskTemplate taskData)
    {
        if (state == TaskPoolOperation.Update)
        {
            scrollControllers.updateCellData(taskData);
            return;
        }
        else if (state == TaskPoolOperation.Add)
        {
            taskPools.Add(taskData);
        }
        else
        {
            taskPools.Remove(taskData);
        }

        RefreshListView();
        UpdateUI();
    }

    public void OnClick(int index)
    {
        curCategory = (TaskCategory)index;
        RefreshListView();
        UpdateUI();
    }

    private void RefreshListView()
    {
        switch (curCategory)
        {
        case TaskCategory.ALL:  //all
            Refresh(taskPools);
            break;
        case TaskCategory.BL1:
        case TaskCategory.BL2:
        case TaskCategory.BL3:
        case TaskCategory.BL4:
        case TaskCategory.BL5:
        case TaskCategory.OTHERS:
            {
                var tasks = taskPools.FindAll(x => x.level == curCategory);
                Refresh(tasks);
                break;
            }
        }
    }

    public void OnClickClose() {
        curCategory = TaskCategory.ALL;
        gameObject.SetActive(false);
    }

    public void OnClickOperation(int type) {
        sysOperation = (SysPoolOpertionType)type;
        scrollControllers.Refresh();
        UpdateUI();
    }

    void Refresh(List<TaskTemplate> templates) {
        taskPoolsInShow = templates;
        uiSortMenuWidget.onSortChanged.RemoveListener(OnSortChanged);
        uiSortMenuWidget.SetCurrentSort(sortSetting.sortKey, sortSetting.ascending);
        uiSortMenuWidget.onSortChanged.AddListener(OnSortChanged);

        var comparer = TaskTemplateComparison.Get((TaskTemplateSortType)sortSetting.sortKey, sortSetting.ascending);
        if (comparer != null)
        {
            templates.Sort(comparer);
        }

        scrollControllers.InitializeWithData(templates);
    }

    void OnSortChanged() {
        sortSetting.SetSortCriterion(uiSortMenuWidget.activeSortOption, uiSortMenuWidget.sortAsc);
        Refresh(taskPoolsInShow);
    }
}
