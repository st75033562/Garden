using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using g_WebRequestManager = Singleton<WebRequestManager>;
public class CourseCell : BaseCourseInfo {

    public override void configureCellData() {
        base.configureCellData();
       
    }

    public void OnClickCell() {
        var controller = (base.Context as OnlineCourseTeacherController);
        if(controller.currentCourseType == CourseListType.PublishAll || controller.currentCourseType == CourseListType.Test
            || controller.currentCourseType == CourseListType.publishMy) {
            if(courseInfo.operationType == CourseInfo.OperationType.DELETE) {

            } else {
                controller.ClickPublishInfo(courseInfo);
            }
        } else if(controller.currentCourseType == CourseListType.Draft) {
            if(courseInfo.operationType == CourseInfo.OperationType.DELETE) {
            
            } else if(courseInfo.operationType == CourseInfo.OperationType.PUBLISH) {
                controller.ClickPublish(courseInfo);
            } else if(courseInfo.operationType == CourseInfo.OperationType.TEST) {
                controller.ClickTest(courseInfo);
            } else if(courseInfo.operationType == CourseInfo.OperationType.Normal) {
                controller.ClickOpen(courseInfo);
            } else if(courseInfo.operationType == CourseInfo.OperationType.EDITOR) {
                controller.ClickEditor(courseInfo);
            }
        }
    }

    public void OnClickLook() {
        (Context as OnlineCourseTeacherController).ClickLook(courseInfo);
    }

    public void OnClickLookPassword() {
        (Context as OnlineCourseTeacherController).ClickLookPassword(courseInfo);
    }

    void OnDisable() {
        if(courseInfo != null) {
            courseInfo.SetListen(null);
        }
    }

    

    
}
