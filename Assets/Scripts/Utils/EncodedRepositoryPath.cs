using System;
using System.Linq;

public class EncodedRepositoryPath : BaseRepositoryPath
{
    private IRepositoryPath m_logicalPath;

    public EncodedRepositoryPath(ProjectRepository repo, string path, bool isFile)
        : base(repo, path, isFile)
    {
    }

    public override bool isLogical
    {
        get { return logicalPath == this; }
    }

    public override IRepositoryPath logicalPath
    {
        get
        {
            if (m_logicalPath == null)
            {
                var components = ToString().Split('/');
                var fileIndex = Array.FindIndex(components, x => !repository.isDirName(x));
                var logicalCompNum = fileIndex != -1 ? fileIndex + 1 : components.Length;
                if (logicalCompNum != components.Length)
                {
                    m_logicalPath = repository.createPath(string.Join("/", components.Take(logicalCompNum).ToArray()),
                                                          fileIndex != -1);
                }
                else
                {
                    m_logicalPath = this;
                }
            }
            return m_logicalPath;
        }
    }
}
