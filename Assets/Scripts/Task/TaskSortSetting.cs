using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskSortSetting {

    public const string keyName = "my_task_sort";

    public static readonly UISortSettingDefault Default = new UISortSettingDefault {
        key = (int)PopupStudentTasks.SortType.CreateTime,
        asc = true
    };
}
