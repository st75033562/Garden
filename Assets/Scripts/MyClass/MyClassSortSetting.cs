using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyClassSortSetting {

    public const string keyName = "my_class_sort";

    public static readonly UISortSettingDefault Default = new UISortSettingDefault {
        key = (int)MyClassController.SortType.Name,
        asc = true
    };
}
