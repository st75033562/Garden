using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using g_WebRequestManager = Singleton<WebRequestManager>;

public class TaskInfoCellData : TaskInfo{
    public PopupStudentTasks taskSceneController;
    public byte[] bytes;
    public byte[] gradeImageBytes;
	public byte[] m_CommentCode;
	public byte[] m_CommentMessage;
}

public class PopupStudentTasks : PopupController {
    [SerializeField]
    private ScrollableAreaController scrollController;
    [SerializeField]
    private GameObject preClickButMark;
    [SerializeField]
    private Text textTitle;
    [SerializeField]
    private TaskCellDetail taskCellDetail;
    public UISortMenuWidget uiSortMenuWidget;
    private UISortSetting sortSetting;

    private List<TaskInfoCellData> listTask = new List<TaskInfoCellData> ();
    private IKickNotificationEvent kickNotificationEvent;

    public enum SortType {
        CreateTime,
        Name,
    }

    private static readonly string[] s_sortOptions = {
        "ui_single_pk_sort_creation_time",
        "ui_single_pk_sort_name"
    };

    // Use this for initialization
    protected override void Start () {
        base.Start();

        kickNotificationEvent = (IKickNotificationEvent)payload;
        Debug.Assert(kickNotificationEvent != null);
        kickNotificationEvent.onDidConfirmNotification += OnDidConfirmKickNotification;

        sortSetting = (UISortSetting)UserManager.Instance.userSettings.
           Get(TaskSortSetting.keyName, true);
        uiSortMenuWidget.SetOptions(s_sortOptions.Select(x => x.Localize()).ToArray());

        listTask.Clear ();
        textTitle.text = "" + UserManager.Instance.CurClass.m_Name;
        CMD_Get_Task_Info_r_Parameter task_r = new CMD_Get_Task_Info_r_Parameter ();
        task_r.ClassId = UserManager.Instance.CurClass.m_ID;

        int popupId = PopupManager.ShowMask();
        SocketManager.instance.send(Command_ID.CmdGetTaskInfoR, task_r.ToByteString(), (res, content) => {
            if(res == Command_Result.CmdNoError) {
                PopupManager.Close(popupId);
                CMD_Get_Task_Info_a_Parameter task_a = CMD_Get_Task_Info_a_Parameter.Parser.ParseFrom(content);
                foreach(A8_Task_Info taskInfo in task_a.TaskList) {
                    TaskInfoCellData taskData = new TaskInfoCellData();
                    taskData.taskSceneController = this;
                    taskData.SetValue(taskInfo);
                    listTask.Add(taskData);
                }
                scrollController.context = this;
                SortShow();
            } else {
                PopupManager.Notice(res.Localize());
            }
        });
    }

    void SortShow() {
        uiSortMenuWidget.onSortChanged.RemoveListener(OnSortChanged);
        uiSortMenuWidget.SetCurrentSort(sortSetting.sortKey, sortSetting.ascending);
        uiSortMenuWidget.onSortChanged.AddListener(OnSortChanged);

        var comparer = GetComparison(sortSetting.sortKey, sortSetting.ascending);
        if(comparer != null) {
            listTask.Sort(comparer);
        }

        scrollController.InitializeWithData(listTask);
    }

    void OnSortChanged() {
        sortSetting.SetSortCriterion(uiSortMenuWidget.activeSortOption, uiSortMenuWidget.sortAsc);
        SortShow();
    }

    static Comparison<TaskInfoCellData> GetComparison(int type, bool asc) {
        switch((SortType)type) {
            case SortType.CreateTime:
                return (x, y) => {
                    var res = x.m_CreateTime.CompareTo(y.m_CreateTime); ;
                    return asc ? res : -res;
                };
            case SortType.Name:
                return (x, y) => {
                    var res = string.Compare(x.m_Name, y.m_Name, StringComparison.CurrentCultureIgnoreCase);
                    return asc ? res : -res;
                };
        }
        return null;
    }

    public void NetFail (WebRequestData data)
    {
        PopupManager.Notice("net_error_try_again".Localize(),()=> {
            g_WebRequestManager.instance.AddTask(data);
        });
    }

    public void OnClickTask (GameObject go) {
        showButtonMark (go);
    }

    void showButtonMark (GameObject go) {
        if(go == preClickButMark)
            return;
        go.SetActive (true);
        preClickButMark.SetActive (false);
        preClickButMark = go;
    }

    public void ShowTaskDetail (TaskInfoCellData data , TaskCell taskCell) {
        taskCellDetail.gameObject.SetActive(true);
        taskCellDetail.InitData (data , taskCell);
    }

    protected override void OnDestroy()
	{
        base.OnDestroy();

		UserManager.Instance.CurClass = null;
        kickNotificationEvent.onDidConfirmNotification -= OnDidConfirmKickNotification;
	}

    public void OnDidConfirmKickNotification(KickNotification notification)
    {
        Assert.IsNotNull(UserManager.Instance.CurClass);

        if (UserManager.Instance.CurClass.m_ID == notification.classId)
        {
            Close();
        }
    }
}
