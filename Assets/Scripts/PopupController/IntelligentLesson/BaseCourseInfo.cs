using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BaseCourseInfo : ScrollCell {
    public Text nameText;
    public Text priceText;
    public Text useCount;
    public UIImageMedia imageMedia;
    public GameObject selectGo;
    public GameObject countBgMask;
    public GameObject coinGo;


    public CourseInfo courseInfo { set; get; }

    public override void configureCellData() {
        courseInfo = (CourseInfo)DataObject;
        courseInfo.SetListen(PriceChange);
        nameText.text = courseInfo.proCourseInfo.CourseName;
        if(useCount != null) {
            useCount.text = "ui_text_num_people".Localize() + ":" + courseInfo.proCourseInfo.CourseAttendUserCount;
        }
        
        PriceChange(courseInfo.CoursePrice);

        imageMedia.SetImage(courseInfo.proCourseInfo.CourseCoverImageUrl);

        ShowSelectPanel(courseInfo.operationType != CourseInfo.OperationType.Normal);

        coinGo.SetActive(((BaseILCourese)Context).courseType != GetCourseListType.GetCourseInvitedMyself);
    }

    void PriceChange(uint price) {
        if(price == 0) {
            priceText.text = "0";
        } else {
            priceText.text = price.ToString();
        }
    }

    public void ShowSelectPanel(bool state) {
        if(gameObject.activeSelf) {
            selectGo.SetActive(state);
            if(countBgMask != null) {
                countBgMask.SetActive(state);
            }
        }
    }
}
