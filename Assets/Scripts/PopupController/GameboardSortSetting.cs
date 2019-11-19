public static class GameboardSortSetting
{
    public const string Prefix = "gb_sort";

    public static readonly UISortSettingDefault Default = new UISortSettingDefault {
        key = (int)PathInfoMember.CreateTime,
        asc = true
    };

    public static string GetKey(string path)
    {
        return Prefix + "." + path;
    }
}
