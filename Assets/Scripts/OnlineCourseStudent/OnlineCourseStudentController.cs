using Google.Protobuf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class StOnlineCourseStep {
    public OnlineCourseStudentController.ShowType showType;
    public uint courseItemId;
    public Vector2 scrollPostion;
    public uint periodInfoId = 0; //选中的课时id
    public Vector2 scrollPeriodPostion;
    public bool OpenPre;
}

public class CourseInfoStu {
    public Course_Info courseInfo { get; set; }
    public List<St_Period_Info> periodInfos = new List<St_Period_Info>();
    private CourseInfoStu() {}
    public static CourseInfoStu Parse(Course_Info courseInfo) {
        var corseInfoStu = new CourseInfoStu();
        corseInfoStu.courseInfo = courseInfo;

        foreach(uint i in courseInfo.PeriodDisplayList) {
            corseInfoStu.periodInfos.Add(new St_Period_Info(courseInfo, courseInfo.PeriodList[i]));
        }

        return corseInfoStu;
    }

    public float GetProgress() {
        int CurPeriodLevel = 0;

        foreach(St_Period_Info info in periodInfos) {
            if(info.IsPass()) {
                CurPeriodLevel++;
            } else {
                break;
            }
        }

        float progressValue = 0;
        if(periodInfos.Count != 0) {
            progressValue = (float)CurPeriodLevel / periodInfos.Count;
        }

        return (float)System.Math.Round(progressValue, 2);
    }
}

public class OnlineCourseStudentController : SceneController {
    public ScrollLoopController scroll;
    public ScrollLoopController scrollTest;
    public StudentAddCourse studentAddCourse;
    public StudentPeriodUI studentPeriodUi;
    public GameObject addPanel;
    public Toggle togglePublish;
    public Toggle toggleTest;
    public UISortMenuWidget uiSortMenuWidget;

    private UISortSetting sortSetting;

    private List<CourseInfoStu> myCourseInfos = new List<CourseInfoStu>();
    private List<CourseInfoStu> myTestCourseInfos = new List<CourseInfoStu>();

    public enum SortType {
        Name,
        Progress
    }

    private static readonly string[] s_sortOptions = {
        "ui_single_pk_sort_name",
        "ui_rate_of_progress"
    };
    public StOnlineCourseStep stOnlineCourseStep { get; set; }
    public ShowType showType { get; set; }

    public List<CourseInfoStu> FormalCourses {
        get {
            return myCourseInfos;
        }
    }

    public List<CourseInfoStu> testCourses {
        get {
            return myTestCourseInfos;
        }
    }
    public enum ShowType {
        Formal,
        Test
    }

    public override void Init(object userData, bool isRestored) {
        base.Init(userData, isRestored);
        stOnlineCourseStep = userData as StOnlineCourseStep;
    }

    // Use this for initialization
    protected override void Start () {
        base.Start();
        sortSetting = (UISortSetting)UserManager.Instance.userSettings.
           Get(OnlineCSSortSetting.keyName, true);

        uiSortMenuWidget.SetOptions(s_sortOptions.Select(x => x.Localize()).ToArray());

        int maskId = PopupManager.ShowMask();
        CMD_Get_Course_List_r_Parameters courseListR = new CMD_Get_Course_List_r_Parameters();
        courseListR.ReqType = GetCourseListType.GetCourseMyself;
        SocketManager.instance.send(Command_ID.CmdGetCourseListR, courseListR.ToByteString(), (res, content) => {
            PopupManager.Close(maskId);
            if(res == Command_Result.CmdNoError) {
                CMD_Get_Course_List_a_Parameters courseA = CMD_Get_Course_List_a_Parameters.Parser.ParseFrom(content);
                for(int i = 0; i < courseA.CouseList.Count; i++) {
                    if(courseA.CouseList[i].CourseStatus == Course_Status.Publish) {
                        myCourseInfos.Add(CourseInfoStu.Parse(courseA.CouseList[i]));
                    }else if(courseA.CouseList[i].CourseStatus == Course_Status.Test) {
                        myTestCourseInfos.Add(CourseInfoStu.Parse(courseA.CouseList[i]));
                    }
                }

                scroll.context = this;

                scrollTest.context = this;

                SortItem();
                
                if(stOnlineCourseStep != null) {
                    showType = stOnlineCourseStep.showType;
                    stOnlineCourseStep.OpenPre = true;
                    if(showType == ShowType.Test) {
                        toggleTest.isOn = true;
                        togglePublish.isOn = false;
                        TestArea();
                        scrollTest.normalizedPosition = stOnlineCourseStep.scrollPostion;
                    } else {
                        scroll.normalizedPosition = stOnlineCourseStep.scrollPostion;
                    }

                    StartCoroutine(GetCourseItemCell());
                }
                AddPanelState();

            } else {
                Debug.LogError("CmdCreateCourseR:" + res);
            }
        });
    }

    IEnumerator GetCourseItemCell() {
        yield return new WaitForEndOfFrame();
        LinkedList<ScrollCell> courseCells = null;
        if(showType == ShowType.Test) {
            courseCells = scrollTest.GetCellsInUse();
        } else {
            courseCells = scroll.GetCellsInUse();
        }
        foreach (ScrollCell cell in courseCells)
        {
            CourseItem courseItem = cell.GetComponent<CourseItem>();
            if(courseItem.courseInfo.CourseId == stOnlineCourseStep.courseItemId) {
                courseItem.OnClickCell();
                break;
            }
        }
    }

    void AddPanelState() {
        if(showType == ShowType.Formal) {
            addPanel.SetActive(myCourseInfos.Count == 0);
        } else if(showType == ShowType.Test) {
            addPanel.SetActive(myTestCourseInfos.Count == 0);
        }
    }
    public void FormalArea() {
        showType = ShowType.Formal;
        scroll.gameObject.SetActive(true);
        scrollTest.gameObject.SetActive(false);
        AddPanelState();
    }

    public void TestArea() {
        showType = ShowType.Test;
        scroll.gameObject.SetActive(false);
        scrollTest.gameObject.SetActive(true);
        AddPanelState();
    }
    public void OnClickAdd() {
        studentAddCourse.gameObject.SetActive(true);
        studentAddCourse.ShowAll();
    }

    public void OnClickHome() {
        SceneDirector.Push("LessonCenter");
    }

    public void AddCourses(Course_Info courses) {
        if(showType == ShowType.Formal) {
            myCourseInfos.Add(CourseInfoStu.Parse(courses));
            scroll.refresh();
        } else {
            myTestCourseInfos.Add(CourseInfoStu.Parse(courses));
            scrollTest.refresh();
        }
        SortItem();
        AddPanelState();
    }

    void SortItem() {
        uiSortMenuWidget.onSortChanged.RemoveListener(OnSortChanged);
        uiSortMenuWidget.SetCurrentSort(sortSetting.sortKey, sortSetting.ascending);
        uiSortMenuWidget.onSortChanged.AddListener(OnSortChanged);

        var comparer = GetComparison(sortSetting.sortKey, sortSetting.ascending);
        if (comparer != null)
        {
            myCourseInfos.Sort(comparer);
            myTestCourseInfos.Sort(comparer);
        }

        scroll.initWithData(myCourseInfos);
        scrollTest.initWithData(myTestCourseInfos);
    }

    public void Refresh() {
        if(showType == ShowType.Formal) {
            scroll.refresh();
        } else {
            scrollTest.refresh();
        }
    }

    void OnSortChanged() {
        sortSetting.SetSortCriterion(uiSortMenuWidget.activeSortOption, uiSortMenuWidget.sortAsc);
        SortItem();
    }
    public void ClickCell(CourseItem courseItem , List<St_Period_Info> periodInfos) {
        if(stOnlineCourseStep == null) {
            stOnlineCourseStep = new StOnlineCourseStep();
        }
        Vector2 scrollNormalPostion;
        if(showType == ShowType.Formal) {
            scrollNormalPostion = scroll.normalizedPosition;
        } else {
            scrollNormalPostion = scrollTest.normalizedPosition;
        }
        stOnlineCourseStep.scrollPostion = scrollNormalPostion;
        stOnlineCourseStep.showType = showType;
        stOnlineCourseStep.courseItemId = courseItem.courseInfo.CourseId;

        studentPeriodUi.gameObject.SetActive(true);
       // studentPeriodUi.SetData(courseItem, periodInfos, stOnlineCourseStep);
    }

    static Comparison<CourseInfoStu> GetComparison(int type, bool asc) {
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
