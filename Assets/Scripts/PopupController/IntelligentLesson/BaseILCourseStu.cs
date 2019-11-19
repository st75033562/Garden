using Google.Protobuf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BaseILCourseStu : MonoBehaviour {
    public GameObject addGo;
    public GameObject delGo;
    public UISortMenuWidget uiSortMenuWidget;
    public GameObject centerAddGo;

    public static List<CourseInfoStu> myCourseInfos;
    public static List<CourseInfoStu> myTestCourseInfos;

    enum SortType {
        Name,
        Progress
    }

    private static readonly string[] s_sortOptions = {
        "ui_single_pk_sort_name",
        "ui_rate_of_progress"
    };
    protected UISortSetting sortSetting;

    protected virtual void Start () {
        if(myCourseInfos == null) {
            myCourseInfos = new List<CourseInfoStu>();
            myTestCourseInfos = new List<CourseInfoStu>();
            InitCourseInfo(GetCourseListType.GetCoursePublishedJoined);
        } else {
            Refrsh();
        }
    }

    protected virtual void OnEnable() {
        addGo.SetActive(true);
        delGo.SetActive(false);
        sortSetting = (UISortSetting)UserManager.Instance.userSettings.
           Get(OnlineCSSortSetting.keyName, true);
        uiSortMenuWidget.SetOptions(s_sortOptions.Select(x => x.Localize()).ToArray());
        SetSortMenu();
    }

    protected virtual void OnDisable()
    {
        uiSortMenuWidget.onSortChanged.RemoveListener(OnSortChanged);
    }

    void InitCourseInfo(GetCourseListType type) {
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
                if(type == GetCourseListType.GetCoursePublishedJoined) {
                    for(int i = 0; i < courseA.CouseList.Count; i++) {
                        myCourseInfos.Add(CourseInfoStu.Parse(courseA.CouseList[i]));
                    }
                    InitCourseInfo(GetCourseListType.GetCourseInvitedJoined);
                } else {
                    for(int i = 0; i < courseA.CouseList.Count; i++) {
                        myTestCourseInfos.Add(CourseInfoStu.Parse(courseA.CouseList[i]));
                    }
                }
                Refrsh();
            } else {
                Debug.LogError("CmdCreateCourseR:" + res);
            }
        });
    }
    protected virtual void Refrsh() {
        SetSortMenu();
    }

    void SetSortMenu() {
        uiSortMenuWidget.onSortChanged.RemoveListener(OnSortChanged);
        uiSortMenuWidget.SetCurrentSort(sortSetting.sortKey, sortSetting.ascending);
        uiSortMenuWidget.onSortChanged.AddListener(OnSortChanged);
    }

    public void ClickCell(CourseItem courseItem, List<St_Period_Info> periodInfos) {
        PopupManager.ILPeriodStu(courseItem.courseInfo.CourseName, periodInfos, ()=> {
            Refrsh();
        });
    }

    protected void AddCourse(OnlineCourseStudentController.ShowType showType) {
        if(gameObject.activeInHierarchy) {
            PopupManager.PopupAddCourse(showType, (info) => {
                if(showType == OnlineCourseStudentController.ShowType.Test) {
                    myTestCourseInfos.Add(CourseInfoStu.Parse(info));
                } else {
                    myCourseInfos.Add(CourseInfoStu.Parse(info));
                }
               
                Refrsh();
            });
        }
    }
    void OnDestroy() {
        myCourseInfos = null;
        myTestCourseInfos = null;
    }

    void OnSortChanged() {
        sortSetting.SetSortCriterion(uiSortMenuWidget.activeSortOption, uiSortMenuWidget.sortAsc);
        Refrsh();
    }

    protected static Comparison<CourseInfoStu> GetComparison(int type, bool asc) {
        Comparison<CourseInfoStu> comp = null;
        switch((SortType)type) {
            case SortType.Name:
                comp = (x, y) => string.Compare(x.courseInfo.CourseName, y.courseInfo.CourseName, StringComparison.CurrentCultureIgnoreCase);
                break;
            case SortType.Progress:
                comp = (x, y) => x.GetProgress().CompareTo(y.GetProgress());
                break;
        }
        return comp != null ? comp.Invert(!asc) : null;
    }
}
