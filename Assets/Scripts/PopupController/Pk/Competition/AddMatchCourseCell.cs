using Google.Protobuf;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AddMatchCourseCell : ScrollCell
{
    public Text courseName;
    public UIImageMedia imageMedia;
    public Text courseDescription;
    public Text teacherName;
    public Button btnBuy;
    public Text buyText;
    public Text coinText;

    private Course_Info courseInfo;

    public override void configureCellData()
    {

        courseInfo = (Course_Info)DataObject;
        PopupAddMatchCourse addMatchCourse = Context as PopupAddMatchCourse;
        if (addMatchCourse.currentModel.hasCompetition(courseInfo.CourseId))
        {
            btnBuy.interactable = false;
            buyText.text = "ui_added".Localize();
        }
        else
        {
            btnBuy.interactable = true;
            buyText.text = "ui_add".Localize();
        }
        courseName.text = courseInfo.CourseName;
        courseDescription.text = courseInfo.CourseDescription;
        teacherName.text = courseInfo.CourseCreaterNickname;
        imageMedia.SetImage(courseInfo.CourseCoverImageUrl);
    }

    public void OnClickBuy()
    {
        PopupManager.SetPassword(
            "ui_input_the_password".Localize(),
            "",
            new SetPasswordData((str) => {
                Buy(str);
            }));
    }

    void Buy(string password = null)
    {
        CMD_Buy_Course_r_Parameters courseR = new CMD_Buy_Course_r_Parameters();
        courseR.CourseId = courseInfo.CourseId;
        if (password != null)
            courseR.CoursePassword = password;

        int maskId = PopupManager.ShowMask();
        SocketManager.instance.send(Command_ID.CmdBuyCourseR, courseR.ToByteString(), (res, content) => {
            PopupManager.Close(maskId);
            if (res == Command_Result.CmdCoursePasswordWrong)
            {
                PopupManager.Notice("ui_password_incorrect".Localize());
            }
            else if (res == Command_Result.CmdCoinNotEnough)
            {
                PopupManager.Notice("ui_not_enough_coin".Localize());
            }
            else
            {
                btnBuy.interactable = false;
                buyText.text = "ui_added".Localize();
            }
        });
    }
}
