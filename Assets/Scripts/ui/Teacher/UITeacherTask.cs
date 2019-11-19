using UnityEngine;
using UnityEngine.UI;
using Google.Protobuf;
using System.Linq;
using System;
using System.Globalization;

public class UITeacherTask : MonoBehaviour
{
    public UITeacherEditTask m_UIEdit;
  //  public TeacherManager m_Manager;
    public ScrollableAreaController m_ScrollController;

    public Button[] disableBtns;
    public ButtonColorEffect btnBack;
    public Toggle[] disableToggles;
    public GameObject btnCancle;
    public Button btnDelete;
    public GameObject btnAdd;
    public UISortMenuWidget uiSortMenuWidget;

    private UISortSetting sortSetting;

    public enum SortType {
        Name,
        CreateTime
    }
    private static readonly string[] s_sortOptions = {
        "ui_single_pk_sort_name",
        "ui_single_pk_sort_creation_time"
    };
    void Awake() {
        sortSetting = (UISortSetting)UserManager.Instance.userSettings.
            Get(TeacherTaskSortSetting.keyName, true);
        uiSortMenuWidget.SetOptions(s_sortOptions.Select(x => x.Localize()).ToArray());
    }
    void OnEnable()
    {
        Refresh();
    }

    void Refresh()
    {
        uiSortMenuWidget.onSortChanged.RemoveListener(OnSortChanged);
        uiSortMenuWidget.SetCurrentSort(sortSetting.sortKey, sortSetting.ascending);
        uiSortMenuWidget.onSortChanged.AddListener(OnSortChanged);

        var curClass = UserManager.Instance.CurClass;
        var comparer = GetComparison(sortSetting.sortKey, sortSetting.ascending);
        if (comparer != null)
        {
            curClass.TaskList.Sort(comparer);
        }

        m_ScrollController.InitializeWithData(curClass.TaskList);

        btnAdd.SetActive(!btnCancle.activeSelf && curClass.TaskList.Count == 0);
        btnDelete.gameObject.SetActive(curClass.TaskList.Count != 0);
    }

    public bool isDeleting
    {
        get;
        private set;
    }

    public void CreateNewTask()
    {
        PopupManager.EditorTask(UITeacherEditTask.WorkMode.Create_Mode, Refresh);
    }

    public void DeleteTask(uint ID)
    {
        PopupManager.TwoBtnDialog("ui_confirm_delete".Localize(), "ui_cancel".Localize(), null,
           "ui_confirm".Localize(), ()=> {
            ConfirmDeleteTask(ID);
        });
    }

    void ConfirmDeleteTask(uint tID)
    {
        CMD_Del_Task_r_Parameters tDelTask = new CMD_Del_Task_r_Parameters();
        tDelTask.ClassId = UserManager.Instance.CurClass.m_ID;
        tDelTask.TaskId = tID;

        SocketManager.instance.send(Command_ID.CmdDelTaskR, tDelTask.ToByteString(), DeleteTaskCallBack);
    }

    private void DeleteTaskCallBack(Command_Result res, ByteString content)
    {
  //      m_Manager.CloseMask();
        if (res == Command_Result.CmdNoError)
        {
            var response = CMD_Del_Task_a_Parameters.Parser.ParseFrom(content);
            ClassInfo tCurClass = UserManager.Instance.GetClass(response.ClassId);
            tCurClass.DeleteTask(response.TaskId);

            Refresh();
        }
        else
        {
      //      m_Manager.RequestErrorCode(res);
        }
    }

    public void EditTask(uint ID)
    {
        PopupManager.EditorTask(UITeacherEditTask.WorkMode.Edit_Mode, Refresh, ID);
    }

    public void OnClickDel()
    {
        SetDeleting(true);
        btnDelete.interactable = true;
    }

    public void OnClickCancle()
    {
        SetDeleting(false);
    }

    void SetDeleting(bool isDeleting)
    {
        this.isDeleting = isDeleting;

        foreach (Button btn in disableBtns)
        {
            btn.interactable = !isDeleting;
        }
        foreach (Toggle toggle in disableToggles)
        {
            toggle.interactable = !isDeleting;
        }
        btnCancle.SetActive(isDeleting);
        btnBack.interactable = !isDeleting;

        Refresh();
    }

    void OnSortChanged() {
        sortSetting.SetSortCriterion(uiSortMenuWidget.activeSortOption, uiSortMenuWidget.sortAsc);
        Refresh();
    }

    static Comparison<TaskInfo> GetComparison(int type, bool asc) {
        Comparison<TaskInfo> comp = null;
        switch((SortType)type) {
            case SortType.CreateTime:
                comp = (x, y) => x.m_CreateTime.CompareTo(y.m_CreateTime);
                break;

            case SortType.Name:
                comp = (x, y) => string.Compare(x.m_Name, y.m_Name, StringComparison.CurrentCultureIgnoreCase);
                break;
        }
        return comp != null ? comp.Invert(!asc) : null;
    }

    public void OnClickOpenSort() {
        uiSortMenuWidget.gameObject.SetActive(true);
    }

    public void OnClickCloseSort() {
        uiSortMenuWidget.gameObject.SetActive(false);
    }
}
