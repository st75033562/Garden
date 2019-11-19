public static class CompetitionCodeGroupsKey
{
    public const string Prefix = "competition_gb_code_groups";

    public static string Create(CompetitionProblem problem)
    {
        return Prefix + "." + problem.competition.id + "." + problem.id;
    }
}
