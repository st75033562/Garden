using System;

using System.IO;

public static class PythonConstants
{
    public const string Main = "main.py";
}

public class PythonRepository : ProjectRepository
{
    public static PythonRepository instance { get; set; }

    public PythonRepository() { }

    public void createFile(string projectname, string fileName)
    {
        saveFile(projectname, fileName, null);
    }

    protected override string searchPattern
    {
        get { return "*.py"; }
    }

    public FileCollection loadProjectFiles(string path)
    {
        var coll = new FileCollection();
        if (File.Exists(getRootPath() + path))
        {
            var data = loadFile(path);
            if (data != null)
            {
                var relativePath = Path.GetFileName(path);
                coll.Add(new FileData(relativePath, data));
            }
        }
        else
        {
            coll.name = Path.GetFileName(path);
            foreach (var filePath in listDirectory(path, FileListOptions.Recursive | FileListOptions.File))
            {
                var data = loadFile(filePath.ToString());
                if (data != null)
                {
                    var relativePath = filePath.ToString().Substring(path.Length + 1);
                    coll.Add(new FileData(relativePath, data));
                }
            }
        }

        return coll;
    }

    public override IRepositoryPath createPath(string path, bool isFile)
    {
        return new SimpleRepositoryPath(this, path, isFile);
    }

    public override string makeDirName(string logicalName)
    {
        if (string.IsNullOrEmpty(logicalName))
        {
            throw new ArgumentException("logicalName");
        }
        return logicalName;
    }

    public override bool isDirName(string rawName)
    {
        if (rawName == null)
        {
            throw new ArgumentNullException("rawName");
        }
        return true;
    }

    public override string getLogicalDirName(string rawName)
    {
        if (rawName == null)
        {
            throw new ArgumentNullException("rawName");
        }
        return rawName;
    }
}
