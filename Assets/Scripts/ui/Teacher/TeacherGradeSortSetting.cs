using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeacherGradeSortSetting {

    public const string keyName = "teacher_grade_sort";

    public static readonly UISortSettingDefault Default = new UISortSettingDefault {
        key = (int)UITeacherGrade.SortType.CreateTime,
        asc = true
    };
}
