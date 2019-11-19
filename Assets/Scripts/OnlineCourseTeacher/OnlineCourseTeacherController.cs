using Google.Protobuf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CourseInfo{
    public enum OperationType {
        Normal,
        DELETE,
        PUBLISH,
        TEST,
        SELECT,
        EDITOR
    }

    public Course_Info proCourseInfo;
    public OperationType operationType { get; set; }

    private bool delMode;
    private uint coursePrice;
    private Action<uint> coursePriceListen;

    public void SetListen(Action<uint> coursePriceListen) {
        this.coursePriceListen = coursePriceListen;
    }
    public uint CoursePrice {
        get { return coursePrice; }
        set {
            coursePrice = value;
            proCourseInfo.CoursePrice = coursePrice;
            if(coursePriceListen != null)
                coursePriceListen(coursePrice);
        }
    }
    public CourseInfo ParseProtobuf(Course_Info info) {
        proCourseInfo = info;
        CoursePrice = info.CoursePrice;
        return this;
    }
}

public enum CourseListType {
    PublishAll,
    publishMy,
    Draft,
    Test
}
public class OnlineCourseTeacherController : MonoBehaviour {
    [SerializeField]
    private ScrollLoopController scroll;
    [SerializeField]
    private LessonEditor lessonEditor;
    [SerializeField]
    private EveryLessonManager everyLesson;
    [SerializeField]
    private AnchoredWidget[] anchoreWidgets;
    [SerializeField]
    private PublishPeriod publishPeriod;
    [SerializeField]
    private GameObject draftAddGo;
    public GameObject btnDel;
    public GameObject btnAdd;
    public Button[] disableModeBtns;
    public Toggle[] disableModeTogs;
    public ButtonColorEffect backColorEffect;
    public GameObject btnCancle;
    public GameObject[] operationBtns;
    public UISortMenuWidget uiSortMenuWidget;

    private List<CourseInfo> coursePublishList = new List<CourseInfo>();
    private List<CourseInfo> courseMyPublishList = new List<CourseInfo>();
    private List<CourseInfo> courseMyselfList = new List<CourseInfo>();
    private List<CourseInfo> courseTestList = new List<CourseInfo>();
    private UISortSetting sortSetting;
    private List<CourseInfo> CourseInfoInScroll;

    private static readonly string[] s_sortOptions = {
        "ui_single_pk_sort_creation_time",
        "ui_single_pk_sort_name"
    };

    public CourseListType currentCourseType { set; get; }

    public enum SortType {
        CreateTime,
        Name
    }
    // Use this for initialization
    void Start() {
        scroll.context = this;
        sortSetting = (UISortSetting)UserManager.Instance.userSettings.
            Get(OnlineCTSortSeeting.keyName, true);

        uiSortMenuWidget.SetOptions(s_sortOptions.Select(x => x.Localize()).ToArray());
        StartCoroutine(WaitFirstFrameOver());
        OnClickPublish();
    }
    IEnumerator WaitFirstFrameOver() {
        yield return null;
        AnchoredWidget.ForceImmediateLayout();
        foreach(AnchoredWidget anchoreWidget in anchoreWidgets) {
            Destroy(anchoreWidget);
        }
    }

    public void OnClickAddCourse() {
        everyLesson.gameObject.SetActive(false);
        lessonEditor.SetDataAndShow(null);
    }

    public void OnClickHome() {
        SceneDirector.Push("Lobby");
    }

    public void AddAndShowCourse(CourseInfo info , bool editor = false) {
        lessonEditor.gameObject.SetActive(false);
        if(!editor) {
            courseMyselfList.Add(info);
            InitScrollView(courseMyselfList, true);
            ActiveOPeration();
        }
            
        everyLesson.gameObject.SetActive(true);
        everyLesson.SetData(info);
    }

    public void OnClickBack() {
        SceneDirector.Push("LessonCenter");

    }

    public void OnClickPublish() {
        coursePublishList.Clear();
        int maskId = PopupManager.ShowMask();
        CMD_Get_Course_List_r_Parameters courseListR = new CMD_Get_Course_List_r_Parameters();
        courseListR.ReqType = GetCourseListType.GetCoursePublished;
        SocketManager.instance.send(Command_ID.CmdGetCourseListR, courseListR.ToByteString(), (res, content) => {
            PopupManager.Close(maskId);
            if(res == Command_Result.CmdNoError) {
                CMD_Get_Course_List_a_Parameters courseA = CMD_Get_Course_List_a_Parameters.Parser.ParseFrom(content);
                foreach (Course_Info info in courseA.CouseList) {
                    var courseInfo = new CourseInfo().ParseProtobuf(info);
                    coursePublishList.Add(courseInfo);
                    if(info.CourseCreaterUserid == UserManager.Instance.UserId) {
                        courseMyPublishList.Add(courseInfo);
                    }
                }
                SetTextStyle(CourseListType.PublishAll);
                ActiveOPeration();
                InitScrollView(coursePublishList, false);
            } else {
                Debug.LogError("CmdCreateCourseR:" + res);
            }  
        });
    }

    void ActiveOPeration() {
        bool isEmptyCourse = true;
        draftAddGo.SetActive(false);
        if(currentCourseType == CourseListType.Draft) {
            isEmptyCourse = courseMyselfList != null && courseMyselfList.Count == 0;
            ActiveOperationCell(!isEmptyCourse);
            btnAdd.SetActive(true);
            draftAddGo.SetActive(isEmptyCourse);
        } else if(currentCourseType == CourseListType.Test) {
            isEmptyCourse = courseTestList != null && courseTestList.Count == 0;
            ActiveOperationCell(false);
            if(!isEmptyCourse) {
                btnDel.SetActive(true);
            }
            btnAdd.SetActive(false);
        } else if(currentCourseType == CourseListType.PublishAll) {
            isEmptyCourse = coursePublishList != null && coursePublishList.Count == 0;
            ActiveOperationCell(false);
            if(!isEmptyCourse && UserManager.Instance.IsAdmin) {
                btnDel.SetActive(true);
            }
            btnAdd.SetActive(false);
        } else if(currentCourseType == CourseListType.publishMy) {
            isEmptyCourse = coursePublishList != null && coursePublishList.Count == 0;
            ActiveOperationCell(false);
            if(!isEmptyCourse) {
                btnDel.SetActive(true);
            }
            btnAdd.SetActive(false);
        }
    }

    void ActiveOperationCell(bool Active) {
        foreach(GameObject go in operationBtns) {
            go.SetActive(Active);
        }
    }

    public void OnClickMyself() {
        courseMyselfList.Clear();
        int maskId = PopupManager.ShowMask();
        CMD_Get_Course_List_r_Parameters courseListR = new CMD_Get_Course_List_r_Parameters();
        courseListR.ReqType = GetCourseListType.GetCourseMyself;
        SocketManager.instance.send(Command_ID.CmdGetCourseListR, courseListR.ToByteString(), (res, content) => {
            PopupManager.Close(maskId);
            if(res == Command_Result.CmdNoError) {
                CMD_Get_Course_List_a_Parameters courseA = CMD_Get_Course_List_a_Parameters.Parser.ParseFrom(content);
                for(int i = 0; i < courseA.CouseList.Count; i++) {
                    courseMyselfList.Add(new CourseInfo().ParseProtobuf(courseA.CouseList[i]));
                }
                SetTextStyle(CourseListType.Draft);
                ActiveOPeration();
                InitScrollView(courseMyselfList, false);
            } else {
                Debug.LogError("CmdCreateCourseR:" + res);
            }
        });

        
    }

    public void OnClickTest() {
        courseTestList.Clear();
        int maskId = PopupManager.ShowMask();
        CMD_Get_Course_List_r_Parameters courseListR = new CMD_Get_Course_List_r_Parameters();
        courseListR.ReqType = GetCourseListType.GetCourseTest;
        SocketManager.instance.send(Command_ID.CmdGetCourseListR, courseListR.ToByteString(), (res, content) => {
            PopupManager.Close(maskId);
            if(res == Command_Result.CmdNoError) {
                CMD_Get_Course_List_a_Parameters courseA = CMD_Get_Course_List_a_Parameters.Parser.ParseFrom(content);
                foreach(Course_Info info in courseA.CouseList) {
                    courseTestList.Add(new CourseInfo().ParseProtobuf(info));
                }
                SetTextStyle(CourseListType.Test);
                ActiveOPeration();
                InitScrollView(courseTestList, false);
            } else {
                Debug.LogError("CmdCreateCourseR:" + res);
            }
        });
    }

    public void OnClickMyPublish() {
        courseMyPublishList.Clear();
        int maskId = PopupManager.ShowMask();
        CMD_Get_Course_List_r_Parameters courseListR = new CMD_Get_Course_List_r_Parameters();
        courseListR.ReqType = GetCourseListType.GetCourseMyselfPublished;
        SocketManager.instance.send(Command_ID.CmdGetCourseListR, courseListR.ToByteString(), (res, content) => {
            PopupManager.Close(maskId);
            if(res == Command_Result.CmdNoError) {
                CMD_Get_Course_List_a_Parameters courseA = CMD_Get_Course_List_a_Parameters.Parser.ParseFrom(content);
                foreach(Course_Info info in courseA.CouseList) {
                    var courseInfo = new CourseInfo().ParseProtobuf(info);
                    courseMyPublishList.Add(courseInfo);
                }
                SetTextStyle(CourseListType.publishMy);
                ActiveOPeration();
                InitScrollView(courseMyPublishList, false);
            } else {
                Debug.LogError("CmdCreateCourseR:" + res);
            }
        });
        
    }

    void SetTextStyle(CourseListType type) {
        currentCourseType = type;
    }

    bool debugDel;
    public void ClickPublishInfo(CourseInfo courseInfo) {
        if(debugDel) {
            ClickDel(courseInfo);
            debugDel = false;
            return;
        }
        publishPeriod.SetData(courseInfo.proCourseInfo);
        publishPeriod.gameObject.SetActive(true);
    }

    public void OnClickDebugDel() {
        SetMode(null, CourseInfo.OperationType.DELETE, false);
    }
    public void ClickOpen(CourseInfo courseInfo) {
        AddAndShowCourse(courseInfo , true);
    }

    public void ClickEditor(CourseInfo courseInfo) {
        everyLesson.gameObject.SetActive(false);
        lessonEditor.SetDataAndShow(courseInfo.proCourseInfo);
    }

    public void Refesh() {
        scroll.refresh();
    }

    public void ClickDel(CourseInfo courseInfo) {
        PopupManager.YesNo("ui_course_confirm_delete".Localize(),
            () => {
                var del = new CMD_Del_Course_r_Parameters();
                del.CourseId = courseInfo.proCourseInfo.CourseId;
                int maskId = PopupManager.ShowMask();
                SocketManager.instance.send(Command_ID.CmdDelCourseR, del.ToByteString(), (res, content) => {
                    PopupManager.Close(maskId);
                    if (res == Command_Result.CmdNoError) {
                        if (currentCourseType == CourseListType.Draft) {
                            courseMyselfList.Remove(courseInfo);
                            InitScrollView(courseMyselfList, true);
                        } else if (currentCourseType == CourseListType.Test) {
                            courseTestList.Remove(courseInfo);
                            InitScrollView(courseTestList, true);
                        } else if (currentCourseType == CourseListType.PublishAll) {
                            coursePublishList.Remove(courseInfo);
                            InitScrollView(coursePublishList, true);
                        } else if(currentCourseType == CourseListType.publishMy) {
                            courseMyPublishList.Remove(courseInfo);
                            InitScrollView(courseMyPublishList, true);
                        }
                        ActiveOPeration();
                    } else {
                        Debug.LogError("CmdCreateCourseR:" + res);
                    }
                });
            });
    }

    public void ClickPublish(CourseInfo courseInfo) {
        PopupManager.YesNo("ui_text_to_release".Localize(),
            () => {
                CMD_Set_Course_Status_r_Parameters setCourse = new CMD_Set_Course_Status_r_Parameters();
                setCourse.CourseId = courseInfo.proCourseInfo.CourseId;
                setCourse.CourseStatus = Course_Status.Publish;
                int maskId = PopupManager.ShowMask();
                SocketManager.instance.send(Command_ID.CmdSetCourseStatusR, setCourse.ToByteString(), (res, content) => {
                    PopupManager.Close(maskId);
                    if(res == Command_Result.CmdNoError) {
                        CMD_Set_Course_Status_a_Parameters course_status_a = CMD_Set_Course_Status_a_Parameters.Parser.ParseFrom(content);
                        var couseInfo = new CourseInfo().ParseProtobuf(course_status_a.CourseInfo);
                        coursePublishList.Add(couseInfo);
                        courseMyPublishList.Add(couseInfo);
                    } else {
                        Debug.LogError("CmdCreateCourseR:" + res);
                    }

                    PopupManager.Notice("ui_published_sucess".Localize());
                });
        });
    }

    public void ClickTest(CourseInfo courseInfo) {
        PopupManager.SetPassword(
            "ui_text_to_test_section".Localize(),
            "ui_set_password".Localize(),
            new SetPasswordData((str) => {
                CMD_Set_Course_Status_r_Parameters setCourse = new CMD_Set_Course_Status_r_Parameters();
                setCourse.CourseId = courseInfo.proCourseInfo.CourseId;
                setCourse.CourseStatus = Course_Status.Test;
                setCourse.CoursePassword = str;
                int maskId = PopupManager.ShowMask();
                SocketManager.instance.send(Command_ID.CmdSetCourseStatusR, setCourse.ToByteString(), (res, content) => {
                    PopupManager.Close(maskId);
                    if (res == Command_Result.CmdNoError)
                    {
                        CMD_Set_Course_Status_a_Parameters course_status_a = CMD_Set_Course_Status_a_Parameters.Parser.ParseFrom(content);
                        if (courseTestList == null)
                            courseTestList = new List<CourseInfo>();
                        courseTestList.Add(new CourseInfo().ParseProtobuf(course_status_a.CourseInfo));

                        PopupManager.Notice("ui_published_sucess".Localize());
                    }
                    else
                    {
                        PopupManager.Notice(res.Localize());
                    }
                });
            }));
    }

    public void ClickLook(CourseInfo courseInfo) {
        ClickPublishInfo(courseInfo);
    }

    public void ClickLookPassword(CourseInfo courseInfo) {
        int maskId = PopupManager.ShowMask();
        var password_r = new CMD_Get_Course_Password_r_Parameters();
        password_r.CourseId = courseInfo.proCourseInfo.CourseId;
        SocketManager.instance.send(Command_ID.CmdGetCoursePasswordR, password_r.ToByteString(), (res, content) => {
            PopupManager.Close(maskId);
            if(res == Command_Result.CmdNoError) {
                var password_a = CMD_Get_Course_Password_a_Parameters.Parser.ParseFrom(content);
                PopupManager.SetPassword("ui_minicourse_view_password".Localize(), "", new SetPasswordData(null, password_a.CoursePassword));
            } else {
                PopupManager.Notice(res.Localize());
            }
        });
    }

    public void OnClickDelMode(Button btn) {
        SetMode(btn, CourseInfo.OperationType.DELETE, false);
    }

    public void OnClickEditorMode(Button btn) {
        SetMode(btn, CourseInfo.OperationType.EDITOR, false);
    }
    public void OnClickPublishMode(Button btn) {
        SetMode(btn, CourseInfo.OperationType.PUBLISH, false);
    }

    public void OnClickTestMode(Button btn) {
        SetMode(btn, CourseInfo.OperationType.TEST, false);
    }

    void SetMode(Button btn, CourseInfo.OperationType type, bool isCancle) {
        btnCancle.SetActive(true);
        foreach (Button button in disableModeBtns)
        {
            if(isCancle)
                button.interactable = true;
            else
                button.interactable = button == btn;
        }
        foreach (Toggle tog in disableModeTogs)
        {
            if(isCancle)
                tog.interactable = true;
            else
                tog.interactable = false;
        }
        foreach (CourseInfo info in courseMyselfList)
        {
            info.operationType = type;
        }
        foreach(CourseInfo info in courseTestList) {
            info.operationType = type;
        }
        foreach(CourseInfo courseInfo in coursePublishList) {
            courseInfo.operationType = type;
        }
        foreach(CourseInfo courseInfo in courseMyPublishList) {
            courseInfo.operationType = type;
        }
        var cells = scroll.GetCellsInUse();
        foreach (var cell in cells)
        {
            var courseCell = cell.gameObject.GetComponent<CourseCell>();
            courseCell.ShowSelectPanel(!isCancle);
        }

        backColorEffect.interactable = isCancle;
    }

    public void OnClickCancle() {
        SetMode(null, CourseInfo.OperationType.Normal, true);
        btnCancle.SetActive(false);
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
    
    void InitScrollView(List<CourseInfo> list, bool keepPostion) {
        uiSortMenuWidget.onSortChanged.RemoveListener(OnSortChanged);
        uiSortMenuWidget.SetCurrentSort(sortSetting.sortKey, sortSetting.ascending);
        uiSortMenuWidget.onSortChanged.AddListener(OnSortChanged);

        var comparer = GetComparison(sortSetting.sortKey, sortSetting.ascending);
        if (comparer != null)
        {
            list.Sort(comparer);
        }

        scroll.initWithData(list, keepPostion);
        CourseInfoInScroll = list;
    }

    void OnSortChanged() {
        sortSetting.SetSortCriterion(uiSortMenuWidget.activeSortOption, uiSortMenuWidget.sortAsc);
        InitScrollView(CourseInfoInScroll, true);
    }
}
