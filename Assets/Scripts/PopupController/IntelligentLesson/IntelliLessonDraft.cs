using Google.Protobuf;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntelliLessonDraft : BaseILCourese {
    public GameObject centerAddGo;
    protected override void OnEnable() {
        base.OnEnable();
        NodeTemplateCache.Instance.ShowBlockUI = true;
        addGo.SetActive(true);
        editorGo.SetActive(true);
        publishGo.SetActive(true);
        testGo.SetActive(true);
    }
    void Start() {
        Refresh(GetCourseListType.GetCourseDraft);
    }
    public void OnClickAdd() {
        if(gameObject.activeSelf) {
            OpenEditor(null);
        }
    }

    void OpenEditor(CourseInfo courseInfo) {
        PopupManager.ILEditor(courseInfo, (info) => {
            if(info != null) {
                AddData(info);
                OpenPeriod(info);
            } else {
                OpenPeriod(courseInfo);
            }
            RefreshView(true);
        });
    }

    void OpenPeriod(CourseInfo courseInfo) {
        PopupManager.ILPeriod(courseInfo);
    }

    protected override void CellCountChange(int count) {
        centerAddGo.SetActive(count == 0);
    }

    public override void OnClickCell(BaseCourseInfo baseCourseInfo) {
        var courseInfo = baseCourseInfo.courseInfo;
        if(courseInfo.operationType == CourseInfo.OperationType.Normal) {
            OpenPeriod(courseInfo);
        } else if(courseInfo.operationType == CourseInfo.OperationType.EDITOR) {
            OpenEditor(courseInfo);
        } else if(courseInfo.operationType == CourseInfo.OperationType.PUBLISH) {
            ClickPublish(courseInfo);
        } else if(courseInfo.operationType == CourseInfo.OperationType.TEST) {
            ClickTest(courseInfo);
        } else {
            base.OnClickCell(baseCourseInfo);
        }
    }

    public void OnClickPublish() {
        if(!gameObject.activeSelf) {
            return;
        }
        SetMode(CourseInfo.OperationType.PUBLISH);
    }

    public void OnClickTest() {
        if(!gameObject.activeSelf) {
            return;
        }
        SetMode(CourseInfo.OperationType.TEST);
    }

    public void OnClickEditor() {
        if(!gameObject.activeSelf) {
            return;
        }
        SetMode(CourseInfo.OperationType.EDITOR);
    }

    void ClickPublish(CourseInfo courseInfo) {
        if(NoEnoughPeriodNotice(courseInfo)) {
            return;
        }
        PopupManager.YesNo("ui_text_to_release".Localize(),
            () => {
                CMD_Set_Course_Status_r_Parameters setCourse = new CMD_Set_Course_Status_r_Parameters();
                setCourse.CourseId = courseInfo.proCourseInfo.CourseId;
                setCourse.CourseStatus = Course_Status.Publish;
                int maskId = PopupManager.ShowMask();
                SocketManager.instance.send(Command_ID.CmdSetCourseStatusR, setCourse.ToByteString(), (res, content) => {
                    PopupManager.Close(maskId);
                    if(res != Command_Result.CmdNoError) {
                        Debug.LogError("CmdCreateCourseR:" + res);
                    } 
                    PopupManager.Notice("ui_published_sucess".Localize());
                });
            });
    }

    bool NoEnoughPeriodNotice(CourseInfo courseInfo) {
        if(courseInfo.proCourseInfo == null || courseInfo.proCourseInfo.PeriodDisplayList.Count == 0) {
            PopupManager.Notice("ui_not_enough_period".Localize());
            return true;
        }
        return false;
    }

    void ClickTest(CourseInfo courseInfo) {
        if(NoEnoughPeriodNotice(courseInfo)){
            return;
        }
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
                    if(res == Command_Result.CmdNoError) {
                        PopupManager.Notice("ui_published_sucess".Localize());
                    } else {
                        PopupManager.Notice(res.Localize());
                    }
                });
            }));
    }
}
