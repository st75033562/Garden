using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeacherPoolSortSetting {

    public const string keyName = "teacher_pool_sort";

    public static readonly UISortSettingDefault Default = new UISortSettingDefault {
        key = (int)UITeacherTaskPool.SortType.CreateTime,
        asc = true
    };
}
