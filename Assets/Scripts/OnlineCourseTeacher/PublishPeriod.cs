using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PublishPeriod : PopupController {
    public ScrollLoopController scroll;
    public PublishPeriodItem publishPeriodItem;

    private List<Period_Info> periodInfos = new List<Period_Info>();
    private Course_Info courseInfo;
    protected override void Start() {
        base.Start();
        SetData((Course_Info)payload);
    }
    public void SetData(Course_Info courseInfo) {
        periodInfos.Clear();
        this.courseInfo = courseInfo;
        foreach (uint i in courseInfo.PeriodDisplayList)
        {
            periodInfos.Add(courseInfo.PeriodList[i]);
        }

        scroll.context = this;
        scroll.initWithData(periodInfos);
    }

    public void ClickCell(Period_Info periodInfo) {
        PublishPeriodItem.PayLoad payload = new PublishPeriodItem.PayLoad();
        payload.CourseId = courseInfo.CourseId;
        payload.periodInfo = periodInfo;
        PopupManager.PublishPeriodsItem(payload);
    }
}
