using System;
using System.IO;
using System.Linq;

public abstract class BaseRepositoryPath : IRepositoryPath
{
    private readonly string m_path;

    // TODO: need to validate validity of logical path
    protected BaseRepositoryPath(ProjectRepository repo, string path, bool isFile)
    {
        if (repo == null)
        {
            throw new ArgumentNullException("repo");
        }
        if (path == null)
        {
            throw new ArgumentNullException("path");
        }
#if UNITY_EDITOR
        if (path.Split('/').Any(x => FileUtils.fileNameContainsInvalidChars(x)))
        {
            throw new ArgumentException("invalid file name");
        }
#endif
        if (path.Length > ProjectRepository.MaxPathLength)
        {
            throw new ArgumentException("path is too long");
        }
        if (path.StartsWith("/"))
        {
            throw new ArgumentException("path should not start with /");
        }
        if (!isFile && path.EndsWith("/"))
        {
            path = path.Substring(0, path.Length - 1);
        }

        repository = repo;
        m_path = path;
        this.isFile = isFile;
    }

    public bool isDir
    {
        get { return !isFile; }
    }

    public bool isFile
    {
        get;
        private set;
    }

    public abstract bool isLogical
    {
        get;
    }

    public abstract IRepositoryPath logicalPath
    {
        get;
    }

    public string name
    {
        get { return repository.getLogicalDirName(rawName); }
    }

    public string rawName
    {
        get { return Path.GetFileName(m_path); }
    }

    public int depth
    {
        get { return m_path.Count(x => x == '/'); }
    }

    public IRepositoryPath AppendFile(string name)
    {
        validateAppendedName(name);
        return repository.createPath(FileUtils.combine(m_path, name), true);
    }

    public IRepositoryPath AppendLogicalDir(string name)
    {
        validateAppendedName(name);
        var dirName = repository.makeDirName(name);
        return repository.createPath(FileUtils.combine(m_path, dirName), false);
    }

    private void validateAppendedName(string name)
    {
        if (name == null)
        {
            throw new ArgumentNullException("name");
        }
        if (FileUtils.fileNameContainsInvalidChars(name))
        {
            throw new ArgumentException("invalid name");
        }
        if (!isDir)
        {
            throw new InvalidOperationException();
        }
    }

    public override int GetHashCode()
    {
        return m_path.GetHashCode();
    }

    public override string ToString()
    {
        return m_path;
    }

    public IRepositoryPath parent
    {
        get
        {
            var dir = m_path != "" ? Path.GetDirectoryName(m_path) : "";
            return repository.createPath(dir, false);
        }
    }

    public ProjectRepository repository
    {
        get;
        private set;
    }

    public int CompareTo(IRepositoryPath other)
    {
        if (ReferenceEquals(other, null))
        {
            return 1;
        }

        var rhs = other as BaseRepositoryPath;
        if (rhs == null)
        {
            throw new ArgumentException("invalid type");
        }

        if (rhs.repository != repository)
        {
            throw new ArgumentException("repository mismatch");
        }

        var res = isFile.CompareTo(rhs.isFile);
        if (res == 0)
        {
            res = string.Compare(m_path, rhs.m_path, StringComparison.CurrentCultureIgnoreCase);
        }
        return res;
    }

    public override bool Equals(object other)
    {
        if (ReferenceEquals(other, null))
        {
            return false;
        }

        var rhs = other as BaseRepositoryPath;
        if (rhs == null || repository != rhs.repository)
        {
            return false;
        }

        return isFile == rhs.isFile && m_path.Equals(rhs.m_path, StringComparison.OrdinalIgnoreCase);
    }

    public bool Equals(IRepositoryPath other)
    {
        return Equals((object)other);
    }
}
