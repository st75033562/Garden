using Google.Protobuf;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;
using System;
using System.Globalization;

public enum TeacherMainOperationType {
    NONE,
    DELETE,
    EDIT,
}

public class UITeacherMainView : MonoBehaviour {
    public GameObject[] relyOnDataBtns;

    public GameObject m_NewPage;
	public Text m_CreateNewClassBtnText;
    public ScrollLoopController m_ScrollController;
    private UISortSetting sortSetting;
    public UISortMenuWidget uiSortMenuWidget;

    private static readonly string[] s_sortOptions = {
        "ui_single_pk_sort_creation_time",
        "ui_single_pk_sort_name",
        "ui_sort_students"
    };
    private int getClassInfoPopupId;
    private bool thisEnable;
    public enum SortType {
        CreateTime,
        Name,
        Students
    }

    public ScriptLanguage classType {
        get {
            return Preference.scriptLanguage;
        }
        private set { }
    }

    public TeacherMainOperationType operationType { get; private set; }

    void OnEnable() {
        thisEnable = true;
        var classList = FilterClass(Preference.scriptLanguage);
        bool classIsEmpty = classList.Count == 0;
        m_NewPage.SetActive(classIsEmpty && operationType == TeacherMainOperationType.NONE);
        RelyOnDataBtns(classList.Count);
        sortSetting = (UISortSetting)UserManager.Instance.userSettings.
           Get(TeacherClassSortSetting.keyName, true);
        uiSortMenuWidget.SetOptions(s_sortOptions.Select(x => x.Localize()).ToArray());
        SetSortMenu();
    }

    void OnDisable() {
        thisEnable = false;
        uiSortMenuWidget.onSortChanged.RemoveListener(OnSortChanged);
    }

    void RelyOnDataBtns(int dataCount) {
        foreach(GameObject go in relyOnDataBtns) {
            go.SetActive(dataCount != 0);
        }
    }
    // Use this for initialization
    void Start ()
	{
		m_CreateNewClassBtnText.text = "class_new".Localize();
        Refresh();
    }

    List<ClassInfo> FilterClass(ScriptLanguage classType) {
        return UserManager.Instance.ClassList.FindAll((x) => x.languageType == classType && x.m_ClassStatus == ClassInfo.Status.Create_Status);
    }
    void SetSortMenu() {
        uiSortMenuWidget.onSortChanged.RemoveListener(OnSortChanged);
        uiSortMenuWidget.SetCurrentSort(sortSetting.sortKey, sortSetting.ascending);
        uiSortMenuWidget.onSortChanged.AddListener(OnSortChanged);
    }

    public void Refresh()
	{
        if(!thisEnable) {
            return;
        }
        var classList = FilterClass(Preference.scriptLanguage);

        SetSortMenu();

        var comparer = GetClassComparison(sortSetting.sortKey, sortSetting.ascending);
        if (comparer != null)
        {
            classList.Sort(comparer);
        }

        m_ScrollController.initWithData(classList);

        bool classIsEmpty = classList.Count == 0;
        m_NewPage.SetActive(classIsEmpty && operationType == TeacherMainOperationType.NONE);
        RelyOnDataBtns(classList.Count);
    }

	public void ClickCreateClass()
	{
        if(gameObject.activeInHierarchy) {
            PopupManager.EditorClass(0, classType, UITeacherEditClass.WorkMode.CreateNew_Mode, Refresh);
        }
    }

	public void SelectClass(uint id)
	{
        getClassInfoPopupId = PopupManager.ShowMask();
		CMD_Get_Classinfo_r_Parameters tGetClass = new CMD_Get_Classinfo_r_Parameters();
		tGetClass.ReqClassId = id;
        SocketManager.instance.send(Command_ID.CmdGetClassinfoR, tGetClass.ToByteString(), SelectClassCallBack);
	}

	private void SelectClassCallBack(Command_Result res, ByteString content)
	{
		if (res == Command_Result.CmdNoError)
		{
			CMD_Get_Classinfo_a_Parameters tClassInfo = CMD_Get_Classinfo_a_Parameters.Parser.ParseFrom(content);
            A8_Class_Info tCurClass = tClassInfo.ClassInfoList[0];
            UserManager.Instance.CurClass = UserManager.Instance.GetClass(tCurClass.ClassId);
            UserManager.Instance.CurClass.UpdateInfo(tCurClass);
            RequestTask(tCurClass.ClassId);
		}
		else
		{
            PopupManager.Close(getClassInfoPopupId);
            PopupManager.Notice(res.Localize());
		}
	}

	public void RequestTask(uint classID)
	{
		CMD_Get_Task_Info_r_Parameter tRequestTask = new CMD_Get_Task_Info_r_Parameter();
		tRequestTask.ClassId = classID;
		tRequestTask.TaskId = 0;
        SocketManager.instance.send(Command_ID.CmdGetTaskInfoR, tRequestTask.ToByteString(), RequestAllTaskCallBack);
	}

	private void RequestAllTaskCallBack(Command_Result res, ByteString content)
	{
        PopupManager.Close(getClassInfoPopupId);
        
        if (res == Command_Result.CmdNoError)
		{
			var tTaskInfo = CMD_Get_Task_Info_a_Parameter.Parser.ParseFrom(content);

			ClassInfo tCurClass = UserManager.Instance.GetClass(tTaskInfo.ClassId);
            tCurClass.SetTasks(tTaskInfo.TaskList.Select(x => {
                var task = new TaskInfo();
                task.SetValue(x);
                return task;
            }));
            PopupManager.ClassInfo(()=> {
                Refresh();
            });
        }
		else
		{
            PopupManager.Notice(res.Localize());
        }
	}

	public void EditClass(uint id)
	{
        PopupManager.EditorClass(id, classType, UITeacherEditClass.WorkMode.ChangeInfo_Mode, Refresh);
    }

	public void DeleteClass(uint id)
	{
		ClassInfo tClass = UserManager.Instance.GetClass(id);
		string tNotice = "class_delete_prompt".Localize(tClass.m_Name);
        PopupManager.YesNo(tNotice, ()=> {
            ConfirmDeleteClass(id);
        });
    }

	public void ConfirmDeleteClass(uint tID)
	{
		CMD_Del_Classs_r_Parameters tDelete = new CMD_Del_Classs_r_Parameters();
		tDelete.DelClassId = tID;
        int popId = PopupManager.ShowMask();
        SocketManager.instance.send(Command_ID.CmdDelClassR, tDelete.ToByteString(), (res, content) => {
            PopupManager.Close(popId);
            if(res == Command_Result.CmdNoError){
                    var response = CMD_Del_Classs_a_Parameters.Parser.ParseFrom(content);
                    UserManager.Instance.DeleteClass(response.DelClassId);
                    Refresh();
            }else{
                PopupManager.Notice(res.Localize());
            }
        });
    }

    public void OnClickOperation(int index) {
        if(gameObject.activeInHierarchy) {
            operationType = (TeacherMainOperationType)index;
            Refresh();
        }
    }
    void OnSortChanged() {
        sortSetting.SetSortCriterion(uiSortMenuWidget.activeSortOption, uiSortMenuWidget.sortAsc);
        Refresh();
    }

    static Comparison<ClassInfo> GetClassComparison(int type, bool asc) {
        Comparison<ClassInfo> comp = null;
        switch((SortType)type) {
            case SortType.CreateTime:
                comp = (x, y) => x.m_createTime.CompareTo(y.m_createTime);
                break;

            case SortType.Students:
                comp = (x, y) => {
                    if(x.studentsInfos.Count == y.studentsInfos.Count) {
                        return string.Compare(x.m_Name, y.m_Name, StringComparison.CurrentCultureIgnoreCase);
                    }
                    return x.studentsInfos.Count.CompareTo(y.studentsInfos.Count); };
                break;

            case SortType.Name:
                comp = (x, y) => string.Compare(x.m_Name, y.m_Name, StringComparison.CurrentCultureIgnoreCase);
                break;
        }
        return comp != null ? comp.Invert(!asc) : null;
    }
}
