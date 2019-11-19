using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnlineCTSortSeeting {
    public const string keyName = "online_course_t";

    public static readonly UISortSettingDefault Default = new UISortSettingDefault {
        key = (int)OnlineCourseTeacherController.SortType.CreateTime,
        asc = true
    };
}

public class BankCTSortSeeting
{
    public const string keyName = "bank_projects";

    public static readonly UISortSettingDefault Default = new UISortSettingDefault
    {
        key = (int)GBBankShareBase.SortType.CreateTime,
        asc = true
    };
}

public class ExerciseTeaSetting {
    public const string keyName = "exercise_t";

    public static readonly UISortSettingDefault Default = new UISortSettingDefault {
        key = (int)BaseExercise.SortType.CreateTime,
        asc = true
    };
}

public class ExerciseTeaSettingStu {
    public const string keyName = "exercise_stu";

    public static readonly UISortSettingDefault Default = new UISortSettingDefault {
        key = (int)BaseExerciseStu.SortType.Name,
        asc = true
    };
}
