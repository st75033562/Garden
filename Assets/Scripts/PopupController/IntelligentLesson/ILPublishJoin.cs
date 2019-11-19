using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ILPublishJoin : BaseILCourseStu {
    public ScrollLoopController scroll;
    public GameObject editorGo;
    public GameObject publishGo;
    public GameObject testGo;

    protected override void OnEnable() {
        base.OnEnable();
        editorGo.SetActive(false);
        publishGo.SetActive(false);
        testGo.SetActive(false);
    }

    protected override void Start () {
        base.Start();
	}

    protected override void Refrsh() {
        base.Refrsh();
        var comparer = GetComparison(sortSetting.sortKey, sortSetting.ascending);
        if(comparer != null) {
            myCourseInfos.Sort(comparer);
        }
        scroll.context = this;
        scroll.initWithData(myCourseInfos);
        centerAddGo.SetActive(myCourseInfos.Count == 0);
    }

    public void OnClickAdd() {
        AddCourse(OnlineCourseStudentController.ShowType.Formal); 
    }
}
