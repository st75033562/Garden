using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnlineCSSortSetting {

    public const string keyName = "online_cs_sort";

    public static readonly UISortSettingDefault Default = new UISortSettingDefault {
        key = (int)OnlineCourseStudentController.SortType.Name,
        asc = true
    };
}
