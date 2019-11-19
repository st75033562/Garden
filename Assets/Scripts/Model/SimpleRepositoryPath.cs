public class SimpleRepositoryPath : BaseRepositoryPath
{
    public SimpleRepositoryPath(ProjectRepository repo, string path, bool isFile)
        : base(repo, path, isFile)
    {
    }

    public override bool isLogical
    {
        get { return true; }
    }

    public override IRepositoryPath logicalPath
    {
        get { return this; }
    }
}
