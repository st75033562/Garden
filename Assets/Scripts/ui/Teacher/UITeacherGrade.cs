using Google.Protobuf;
using System;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UITeacherGrade : MonoBehaviour
{
	public GameObject m_GradeTemplate;
	public TeacherManager m_Manager;
    public ScrollableAreaController m_ScrollController;
    public UISortMenuWidget uiSortMenuWidget;

    private UISortSetting sortSetting;

    public enum SortType {
        CreateTime,
        Name,
        Students
    }
    private static readonly string[] s_sortOptions = {
        "ui_single_pk_sort_creation_time",
        "ui_single_pk_sort_name",
        "ui_sort_submissions"
    };
    public uint initialTaskId { get; set; }

    void Awake() {
        sortSetting = (UISortSetting)UserManager.Instance.userSettings.
            Get(TeacherGradeSortSetting.keyName, true);
        uiSortMenuWidget.SetOptions(s_sortOptions.Select(x => x.Localize()).ToArray());
    }

    void OnEnable()
    {
        Refresh();
        if (initialTaskId != 0)
        {
            SelectTask(initialTaskId);
            initialTaskId = 0;
        }
    }

    void Refresh() {
        uiSortMenuWidget.onSortChanged.RemoveListener(OnSortChanged);
        uiSortMenuWidget.SetCurrentSort(sortSetting.sortKey, sortSetting.ascending);
        uiSortMenuWidget.onSortChanged.AddListener(OnSortChanged);

        var comparer = GetComparison(sortSetting.sortKey, sortSetting.ascending);
        if (comparer != null)
        {
            UserManager.Instance.CurClass.TaskList.Sort(comparer);
        }
        m_ScrollController.InitializeWithData(UserManager.Instance.CurClass.TaskList);
    }

	public void SelectTask(uint taskID)
	{
		m_Manager.ShowMask();

		CMD_Get_Task_Info_r_Parameter tRequestTask = new CMD_Get_Task_Info_r_Parameter();
		tRequestTask.ClassId = UserManager.Instance.CurClass.m_ID;
		tRequestTask.TaskId = taskID;

		SocketManager.instance.send(Command_ID.CmdGetTaskInfoR, tRequestTask.ToByteString(), OnGetSubmits);
	}

	private void OnGetSubmits(Command_Result res, ByteString content)
	{
		m_Manager.CloseMask();
		if (res == Command_Result.CmdNoError)
		{
			var response = CMD_Get_Task_Info_a_Parameter.Parser.ParseFrom(content);

			ClassInfo classInfo = UserManager.Instance.CurClass;
            A8_Task_Info tTaskformServer = response.TaskList[0];
            TaskInfo tCurTask = classInfo.GetTask(tTaskformServer.TaskId);
            tCurTask.UpdateSubmit(tTaskformServer.TaskSubmitInfo);
            UserManager.Instance.CurTask = tCurTask;
		}
		else
		{
			m_Manager.RequestErrorCode(res);
		}
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

            case SortType.Students:
                comp = (x, y) => x.SubmitList.Count.CompareTo(y.SubmitList.Count);
                break;
        }
        return comp != null ? comp.Invert(!asc) : null;
    }
}
