using Google.Protobuf;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using g_WebRequestManager = Singleton<WebRequestManager>;
public class StudentCourseCell : ScrollCell {
    public Text courseName;
    public UIImageMedia imageMedia;
    public Text courseDescription;
    public Text teacherName;
    public Button btnBuy;
    public Text buyText;
    public Text coinText;

    private Course_Info courseInfo;

    public override void configureCellData() {
       
        courseInfo = (Course_Info)DataObject;
        List<CourseInfoStu> myJoinedCourses = null;
        StudentAddCourse studentAddCourse = Context as StudentAddCourse;
        if(studentAddCourse.showType == OnlineCourseStudentController.ShowType.Formal) {
            myJoinedCourses = BaseILCourseStu.myCourseInfos;
            coinText.transform.parent.gameObject.SetActive(true);
            coinText.text = courseInfo.CoursePrice.ToString();
        } else {
            myJoinedCourses = BaseILCourseStu.myTestCourseInfos;
            coinText.transform.parent.gameObject.SetActive(false);
        }

        var joined = myJoinedCourses.Find((x) => { return x.courseInfo.CourseId == courseInfo.CourseId; }) != null;
        if(joined) {
            btnBuy.interactable = false;

            if(studentAddCourse.showType == OnlineCourseStudentController.ShowType.Formal) 
                buyText.text = "ui_course_purchased".Localize();
            else
                buyText.text = "ui_added".Localize();
        } else {
            btnBuy.interactable = UserManager.Instance.UserId != courseInfo.CourseCreaterUserid;

            if(studentAddCourse.showType == OnlineCourseStudentController.ShowType.Formal)
                buyText.text = "ui_course_buy".Localize();
            else
                buyText.text = "ui_add".Localize();
           
        }

        courseName.text = courseInfo.CourseName;
        courseDescription.text = courseInfo.CourseDescription;
        teacherName.text = courseInfo.CourseCreaterNickname;
        imageMedia.SetImage(courseInfo.CourseCoverImageUrl);
    }

    public void OnClickBuy() {
        if((Context as StudentAddCourse).showType == OnlineCourseStudentController.ShowType.Test)
            PopupManager.SetPassword(
                "ui_input_the_password".Localize(),
                "",
                new SetPasswordData((str) => {
                    Buy(str);
                }));
        else
        {
            Buy();
        }
    }

    void Buy(string password = null) {
        CMD_Buy_Course_r_Parameters courseR = new CMD_Buy_Course_r_Parameters();
        courseR.CourseId = courseInfo.CourseId;
        if(password != null)
            courseR.CoursePassword = password;

        int maskId = PopupManager.ShowMask();
        SocketManager.instance.send(Command_ID.CmdBuyCourseR, courseR.ToByteString(), (res, content) => {
            PopupManager.Close(maskId);
            if(res == Command_Result.CmdCoursePasswordWrong) {
                PopupManager.Notice("ui_password_incorrect".Localize());
            }else if(res == Command_Result.CmdCoinNotEnough) {
                PopupManager.Notice("ui_not_enough_coin".Localize());
            } else {
                (Context as StudentAddCourse).AddMyCourse(courseInfo);
                btnBuy.interactable = false;
                if((Context as StudentAddCourse).showType == OnlineCourseStudentController.ShowType.Formal)
                    buyText.text = "ui_course_purchased".Localize();
                else
                    buyText.text = "ui_added".Localize();
            }
        });
    }
}
