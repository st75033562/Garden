using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SystemPoolSortSetting : MonoBehaviour {

    public const string keyName = "system_pool_sort";

    public static readonly UISortSettingDefault Default = new UISortSettingDefault {
        key = (int)UISystemTaskPool.SortType.CreateTime,
        asc = true
    };
}
