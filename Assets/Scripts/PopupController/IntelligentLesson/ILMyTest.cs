using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ILMyTest : BaseILCourese {

    protected override void OnEnable() {
        base.OnEnable();
        addGo.SetActive(false);
        editorGo.SetActive(false);
        publishGo.SetActive(false);
        testGo.SetActive(false);
        Refresh(GetCourseListType.GetCourseInvitedMyself);
    }
}
