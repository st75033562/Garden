using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

using g_WebRequestManager = Singleton<WebRequestManager>;

public class CourseItem : ScrollCell {

    [SerializeField]
    private Text courseName;
    [SerializeField]
    private Text finishProgress;

    [SerializeField]
    private UIImageMedia imageMedia;

    private WebRequestData webReq;
    public Course_Info courseInfo { get; set; }
    private List<St_Period_Info> periodInfos;

    public override void configureCellData() {
        var courseInfoStu = (CourseInfoStu)DataObject;
        courseInfo = courseInfoStu.courseInfo;
        courseName.text = courseInfo.CourseName;

        imageMedia.SetImage(courseInfo.CourseCoverImageUrl);

        periodInfos = courseInfoStu.periodInfos;

        finishProgress.text = courseInfoStu.GetProgress() * 100 + "%";

    }

    public void OnClickCell() {
        (Context as BaseILCourseStu).ClickCell(this, periodInfos.ToList());
    }
}
