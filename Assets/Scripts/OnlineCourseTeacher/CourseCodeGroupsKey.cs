public static class CourseCodeGroupsKey
{
    public const string Prefix = "period_gb_code_groups";

    public static string Create(uint courseId, uint periodId)
    {
        return Prefix + "." + courseId + "." + periodId;
    }
}
