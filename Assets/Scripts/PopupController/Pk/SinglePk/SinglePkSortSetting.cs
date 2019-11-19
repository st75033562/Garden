public class SinglePkSortSetting
{
    public const string Key = "single_pk_sort";
    public static readonly UISortSettingDefault Default = new UISortSettingDefault {
        key = (int)GBSort_Type.StData,
    };

    public static UISortSetting Get(UserManager user)
    {
        return (UISortSetting)user.userSettings.Get(Key, true);
    }
}
