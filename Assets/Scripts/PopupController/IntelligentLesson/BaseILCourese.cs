using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Google.Protobuf;
using System.Linq;
using System;

public class BaseILCourese : MonoBehaviour {
    public ScrollLoopController scroll;
    public UISortMenuWidget uiSortMenuWidget;
    public GameObject addGo;
    public GameObject delGo;
    public GameObject editorGo;
    public GameObject publishGo;
    public GameObject testGo;

    private List<CourseInfo> courseList = new List<CourseInfo>();
    private static readonly string[] s_sortOptions = {
        "ui_single_pk_sort_creation_time",
        "ui_single_pk_sort_name"
    };
    private UISortSetting sortSetting;
    public enum SortType {
        CreateTime,
        Name
    }

    public GetCourseListType courseType;

    protected virtual void OnEnable() {
        sortSetting = (UISortSetting)UserManager.Instance.userSettings.
           Get(OnlineCTSortSeeting.keyName, true);
        delGo.SetActive(true);
        uiSortMenuWidget.SetOptions(s_sortOptions.Select(x => x.Localize()).ToArray());
        SetSortMenu();
    }
    void SetSortMenu() {
        uiSortMenuWidget.onSortChanged.RemoveListener(OnSortChanged);
        uiSortMenuWidget.SetCurrentSort(sortSetting.sortKey, sortSetting.ascending);
        uiSortMenuWidget.onSortChanged.AddListener(OnSortChanged);
    }
    protected virtual void OnDisable()
    {
        uiSortMenuWidget.onSortChanged.RemoveListener(OnSortChanged);
    }

    protected void Refresh(GetCourseListType type) {
        courseType = type;
        courseList.Clear();
        int maskId = PopupManager.ShowMask();
        CMD_Get_Course_List_r_Parameters courseListR = new CMD_Get_Course_List_r_Parameters();
        courseListR.ReqType = type;
        if(Preference.scriptLanguage == ScriptLanguage.Visual) {
            courseListR.ReqProjectLanguageType = Project_Language_Type.ProjectLanguageGraphy;
        } else {
            courseListR.ReqProjectLanguageType = Project_Language_Type.ProjectLanguagePython;
        }
        SocketManager.instance.send(Command_ID.CmdGetCourseListR, courseListR.ToByteString(), (res, content) => {
            PopupManager.Close(maskId);
            if(res == Command_Result.CmdNoError) {
                CMD_Get_Course_List_a_Parameters courseA = CMD_Get_Course_List_a_Parameters.Parser.ParseFrom(content);
                foreach(Course_Info info in courseA.CouseList) {
                    courseList.Add(new CourseInfo().ParseProtobuf(info));
                }
                RefreshView(true);
            } else {
                Debug.LogError("CmdCreateCourseR:" + res);
            }
        });
    }

    protected virtual void CellCountChange(int count) {}

    public virtual void OnClickCell(BaseCourseInfo baseCourseInfo) {
        var courseInfo = baseCourseInfo.courseInfo;
        if(courseInfo.operationType == CourseInfo.OperationType.DELETE) {
            ClickDel(courseInfo);
        } else if(courseInfo.operationType == CourseInfo.OperationType.Normal) {
            PopupManager.PublishPeriods(courseInfo.proCourseInfo);
        }
    }

    void ClickDel(CourseInfo courseInfo) {
        PopupManager.YesNo("ui_course_confirm_delete".Localize(),
        () => {
            var del = new CMD_Del_Course_r_Parameters();
            del.CourseId = courseInfo.proCourseInfo.CourseId;
            int maskId = PopupManager.ShowMask();
            SocketManager.instance.send(Command_ID.CmdDelCourseR, del.ToByteString(), (res, content) => {
                PopupManager.Close(maskId);
                if(res == Command_Result.CmdNoError) {
                    courseList.Remove(courseInfo);
                    RefreshView(true);
                } else {
                    Debug.LogError("CmdCreateCourseR:" + res);
                }
            });
        });
    }
    static Comparison<CourseInfo> GetComparison(int type, bool asc) {
        Comparison<CourseInfo> comp = null;
        switch((SortType)type) {
            case SortType.CreateTime:
                comp = (x, y) => x.proCourseInfo.CoursePublishTime.CompareTo(y.proCourseInfo.CoursePublishTime);
                break;

            case SortType.Name:
                comp = (x, y) => string.Compare(x.proCourseInfo.CourseName, y.proCourseInfo.CourseName, StringComparison.CurrentCultureIgnoreCase);
                break;
        }
        return comp != null ? comp.Invert(!asc) : null;
    }
    public void AddData(CourseInfo courseInfo) {
        courseList.Add(courseInfo);
    }

    public void RefreshView(bool keepPostion) {
        SetSortMenu();

        var comparer = GetComparison(sortSetting.sortKey, sortSetting.ascending);
        if(comparer != null) {
            courseList.Sort(comparer);
        }
        CellCountChange(courseList.Count);
        scroll.context = this;
        scroll.initWithData(courseList);
    }
    void OnSortChanged() {
        sortSetting.SetSortCriterion(uiSortMenuWidget.activeSortOption, uiSortMenuWidget.sortAsc);
        RefreshView(true);
    }

    public void OnClickDel() {
        if(!gameObject.activeSelf) {
            return;
        }
        SetMode(CourseInfo.OperationType.DELETE);
    }

    protected void SetMode(CourseInfo.OperationType type) {
        foreach(ScrollCell cell in scroll.GetCellsInUse()) {
            cell.GetComponent<BaseCourseInfo>().ShowSelectPanel(true);
        }
        foreach(CourseInfo info in courseList) {
            info.operationType = type;
        }
    }

    public void OnClickCancle() {
        if(!gameObject.activeSelf) {
            return;
        }
        foreach(ScrollCell cell in scroll.GetCellsInUse()) {
            cell.GetComponent<BaseCourseInfo>().ShowSelectPanel(false);
        }
        foreach(CourseInfo info in courseList) {
            info.operationType = CourseInfo.OperationType.Normal;
        }
    }
}
