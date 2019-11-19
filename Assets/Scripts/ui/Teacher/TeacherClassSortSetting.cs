using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeacherClassSortSetting : MonoBehaviour {

    public const string keyName = "teacher_class_sort";

    public static readonly UISortSettingDefault Default = new UISortSettingDefault {
        key = (int)UITeacherMainView.SortType.CreateTime,
        asc = true
    };
}
