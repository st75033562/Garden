public class PKSortSetting
{
    public const string Key = "pk_sort";
    public const string AnswerPrefix = "pk_ans_sort";

    public static readonly UISortSettingDefault Default 
        = new UISortSettingDefault { key = (int)PKSort_Type.PkStData };

    public static readonly UISortSettingDefault AnswerDefault 
        = new UISortSettingDefault { key = (int)PkAnswersView.SortKey.CreationTime };

    public static UISortSetting Get(UserManager user)
    {
        return (UISortSetting)user.userSettings.Get(Key, true);
    }

    public static UISortSetting Get(UserManager user, int pkId)
    {
        var settingKey = AnswerPrefix + "." + pkId;
        return (UISortSetting)user.userSettings.Get(settingKey, true);
    }
}
