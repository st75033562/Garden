using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeacherTaskSortSetting {

    public const string keyName = "teacher_task_sort";

    public static readonly UISortSettingDefault Default = new UISortSettingDefault {
        key = (int)UITeacherTask.SortType.CreateTime,
        asc = true
    };
}
