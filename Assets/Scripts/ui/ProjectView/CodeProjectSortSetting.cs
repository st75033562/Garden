public class CodeProjectSortSetting
{
    public const string Prefix = "code_project_sort";

    public static readonly UISortSettingDefault Default = new UISortSettingDefault {
        key = (int)PathInfoMember.CreateTime,
        asc = true
    };

    public static string GetKey(string path)
    {
        return Prefix + "." + path;
    }
}
