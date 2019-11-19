using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public struct FileData
{
    public string filename;
    public byte[] data;

    public FileData(string filename, byte[] data)
    {
        this.filename = filename;
        this.data = data;
    }
}

[Flags]
public enum FileListOptions
{
    Recursive = 1 << 0,
    Raw       = 1 << 1,
    File      = 1 << 2,
    Directory = 1 << 3,
    FileOrDir = File | Directory,
}

public enum FileNameValidationResult
{
    NoError,
    ReservedName,
    FileNameTooLong,
    InvalidPrefix,
    InvalidChar,
}

/// <summary>
/// A project is just a directory containing files
/// </summary>
public abstract class ProjectRepository
{
    public const int MaxPathLength = 255;
    public const int MaxDepth = 3;
    public const int MaxFileNameLength = (MaxPathLength / (MaxDepth + 1));

    public event Action<string> onCreatingDirectory;
    public event Action<string, byte[]> onSavingFile;
    public event Action<string> onDeletingProject;
    public event Action<string> onProjectDeleted;

    private string m_root;

    protected class FileStats
    {
        public DateTime creationTime;
        public DateTime writeTime;
    }

    // in-memory file stats
    // NOTE: only some file systems support creation time, so we cannot 
    //       rely on file systems to store the creation time.
    private readonly Dictionary<string, FileStats> m_fileStats = new Dictionary<string, FileStats>();

    public virtual void initialize(string rootName, uint userId)
    {
        m_root = getRoot(rootName, userId, false);
        FileUtils.createDirIfNotExist(m_root);
    }

    private static string getRoot(string rootName, uint userId, bool isTemporary)
    {
        rootName = isTemporary ? "." + rootName : rootName;
        return string.Format("{0}/{1}/{2}/", Application.persistentDataPath, userId, rootName);
    }

    public virtual void uninitialize()
    {
        m_fileStats.Clear();
    }

    /// <summary>
    /// return true if the directory exists
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public bool isDirectory(IRepositoryPath path)
    {
        if (path == null)
        {
            throw new ArgumentNullException("path");
        }
        if (!path.isDir)
        {
            throw new ArgumentException("path is not a dir");
        }
        return Directory.Exists(getAbsPath(path.ToString()));
    }

    // check if there exists a file in the given directory
    // name should be unprefixed
    public bool existsPath(string path, string name, FileListOptions opt = FileListOptions.FileOrDir)
    {
        opt &= FileListOptions.FileOrDir;
        var paths = listDirectory(path, opt);
        return paths.Any(x => x.name.ToLower().Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    // name should be unprefixed
    public bool hasProject(string path, string name)
    {
        if(Directory.Exists(getRootPath() + path)) {
            return existsPath(path, name, FileListOptions.File);
        } else {
            return false;
        }
    }

    protected bool saveFile(string projectPath, string filename, byte[] data)
    {
        if (projectPath == null)
        {
            throw new ArgumentNullException("projectPath");
        }

		string path = getAbsPath(projectPath + "/" + filename);
        return saveFileWithFullPath(path, data);
    }

    protected bool saveFileWithFullPath(string path, byte[] data)
    {
        try
        {
            FileUtils.createParentDirectory(path);
            File.WriteAllBytes(path, data ?? new byte[0]);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            return false;
        }
    }

    public PathInfo createDirectory(string path, DateTime dateTimeUtc)
    {
        string fullPath = getAbsPath(path);
        if (!Directory.Exists(fullPath))
        {
            if (onCreatingDirectory != null)
            {
                onCreatingDirectory(path);
            }
            Directory.CreateDirectory(fullPath);
        }
        var stats = getFileStats(path);
        stats.creationTime = stats.writeTime = dateTimeUtc;
        return new PathInfo(createDirPath(path), dateTimeUtc, dateTimeUtc);
    }

    public bool save(string path, IEnumerable<FileNode> files)
    {
        foreach (FileNode df in files)
        {
            string pathName = FileUtils.combine(path, df.PathName);
            if ((FN_TYPE)df.FnType == FN_TYPE.FnFile)
            {
                if (!save(pathName, df.FileContents.ToByteArray()))
                {
                    return false;
                }

                var stats = getFileStats(pathName);
                stats.creationTime = TimeUtils.FromEpochSeconds((long)df.CreateTime);
                stats.writeTime = TimeUtils.FromEpochSeconds((long)df.UpdateTime);

                onUpdatedFileStats(pathName, stats);
            }
            else
            {
                createDirectory(path, TimeUtils.FromEpochSeconds((long)df.CreateTime));
            }
        }
        
        return true;
    }

    
    public void saveSameNameNotice(string path, List<FileNode> files, Action<List<string>> done = null, int index = 0, List<string> downloadStr = null) {
        if(downloadStr == null) {
            downloadStr = new List<string>();
        }
        if(files.Count == index) {
            done(downloadStr);
            return;
        }
        FileNode df = files[index++];
        string pathName = FileUtils.combine(path, df.PathName);
        if((FN_TYPE)df.FnType == FN_TYPE.FnFile) {
            if(hasProject(FileUtils.combine(path, Path.GetDirectoryName(df.PathName)), Path.GetFileName(df.PathName))) {
                PopupManager.YesNo("local_down_notice".Localize(Path.GetFileName(df.PathName)), () => {
                    save(pathName, df);
                    downloadStr.Add(df.PathName);
                    saveSameNameNotice(path, files, done, index, downloadStr);
                },()=> {
                    saveSameNameNotice(path, files, done, index, downloadStr);
                });
            } else {
                downloadStr.Add(df.PathName);
                save(pathName, df);
                saveSameNameNotice(path, files, done, index, downloadStr);
            }
        } else {
            createDirectory(path, TimeUtils.FromEpochSeconds((long)df.CreateTime));
            saveSameNameNotice(path, files, done, index, downloadStr);
        }
    }

    bool save(string path, FileNode file) {
        if(!save(path, file.FileContents.ToByteArray())) {
            return false;
        }
        var stats = getFileStats(path);
        stats.creationTime = TimeUtils.FromEpochSeconds((long)file.CreateTime);
        stats.writeTime = TimeUtils.FromEpochSeconds((long)file.UpdateTime);

        onUpdatedFileStats(path, stats);
        return true;
    }

    protected virtual void onUpdatedFileStats(string path, FileStats stats) { }

    protected FileStats getFileStats(string path)
    {
        FileStats stats;
        if (!m_fileStats.TryGetValue(path, out stats))
        {
            stats = new FileStats();
            m_fileStats.Add(path, stats);
        }
        return stats;
    }

    private void removeDirectoryStats(string dirPath)
    {
        var paths = m_fileStats.Keys.Where(x => 
            x.StartsWith(dirPath, StringComparison.OrdinalIgnoreCase) &&
                (dirPath.Length == x.Length || x[dirPath.Length] == '/')
        ).ToArray();

        foreach (var path in paths)
        {
            m_fileStats.Remove(path);
        }
    }

    public bool save(string pathName, byte[] data)
    {
        if (onSavingFile != null)
        {
            onSavingFile(pathName, data);
        }
        return saveFileWithFullPath(getAbsPath(pathName), data);
    }

    /// <summary>
    /// delete the raw path
    /// </summary>
    public void deleteFile(string path)
    {
        var fullPath = getAbsPath(path);
        if (Directory.Exists(fullPath))
        {
            Directory.Delete(fullPath, true);

            removeDirectoryStats(path);
        }
        else
        {
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                m_fileStats.Remove(path);
            }
        }
    }

    /// <summary>
    /// delete the logical directory
    /// </summary>
    /// <returns></returns>
    public void deleteDirectory(string path)
    {
        var absPath = getAbsPath(path);
        if (Directory.Exists(absPath))
        {
            foreach (var entry in listDirectory(path, FileListOptions.Recursive | FileListOptions.FileOrDir))
            {
                if (entry.isFile)
                {
                    delete(entry.ToString());
                }
            }
            Directory.Delete(absPath, true);

            removeDirectoryStats(path);
        }
    }

    public void delete(IRepositoryPath path)
    {
        if (path == null)
        {
            throw new ArgumentNullException("path");
        }

        if (path.isLogical)
        {
            if (path.isDir)
            {
                deleteDirectory(path.ToString());
            }
            else
            {
                delete(path.ToString());
            }
        }
        else
        {
            deleteFile(path.ToString());
        }
    }

    /// <summary>
    /// delete the logical file, make sure that path is a logical path
    /// </summary>
    public bool delete(string path)
    {
		try
		{
            var fullPath = getAbsPath(path);
            if (Directory.Exists(fullPath))
            {
                if (onDeletingProject != null)
                {
                    onDeletingProject(path);
                }

                doDelete(fullPath);
                Directory.Delete(fullPath, true);

                removeDirectoryStats(path);

                if (onProjectDeleted != null)
                {
                    onProjectDeleted(path);
                }
                return true;
            }
            else if (File.Exists(fullPath))
            {
                if (onDeletingProject != null)
                {
                    onDeletingProject(path);
                }

                doDelete(fullPath);
                File.Delete(fullPath);

                m_fileStats.Remove(path);
                if (onProjectDeleted != null)
                {
                    onProjectDeleted(path);
                }
                return true;
            }
            else
            {
                return false;
            }
		}
		catch (IOException e)
		{
			Debug.LogException(e);
			return false;
		}
    }

    /// <summary>
    /// delete the project rooted at path
    /// </summary>
    /// <param name="path">project root</param>
    protected virtual void doDelete(string projectName) { }

    public byte[] loadFile(string projectName, string fileName)
    {
        return loadFile(projectName + "/" + fileName);
    }

    public byte[] loadFile(string path)
    {
        var absPath = getAbsPath(path);
        try
        {
            if (!File.Exists(absPath))
            {
                return null;
            }
            return File.ReadAllBytes(absPath);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            return null;
        }
    }

    public List<IRepositoryPath> listDirectory(string path, FileListOptions opts = FileListOptions.FileOrDir)
    {
        if (path == null)
        {
            throw new ArgumentNullException("path");
        }
        validateFileOptions(opts);

        var paths = new List<IRepositoryPath>();
        listDirectory(paths, FileUtils.ensureSlashIfNonEmpty(path), opts,
            (fileOrDirPath, fileInfo) => fileOrDirPath, false);
        return paths;
    }

    private static void validateFileOptions(FileListOptions opts)
    {
        if ((opts & FileListOptions.FileOrDir) == 0)
        {
            throw new ArgumentException("File or Directory is not specified");
        }
    }

    public List<PathInfo> listFileInfos(string path, FileListOptions opts = FileListOptions.FileOrDir)
    {
        if (path == null)
        {
            throw new ArgumentNullException("path");
        }
        validateFileOptions(opts);

        var paths = new List<PathInfo>();
        listDirectory(paths, FileUtils.ensureSlashIfNonEmpty(path), opts,
            (repoPath, stats) => {
                return new PathInfo(repoPath, stats.creationTime, stats.writeTime);
            }, true);
        return paths;
    }

    // get all entries in the given path, path must end with '/' except for the root
    private void listDirectory<T>(List<T> paths, string path, FileListOptions opts,
        Func<IRepositoryPath, FileStats, T> action, bool getInfo)
    {
        var dirInfo = new DirectoryInfo(getAbsPath(path));

        var includeFile = (opts & FileListOptions.File) != 0;
        if (includeFile)
        {
            foreach (FileInfo fi in dirInfo.GetFiles(searchPattern))
            {
                var filePath = FileUtils.combine(path, fi.Name);
                FileStats stats = null;
                if (getInfo)
                {
                    stats = getFileStats(filePath);
                }
                paths.Add(action(createFilePath(filePath), stats));
            }
        }

        var raw = (opts & FileListOptions.Raw) != 0;
        var recursive = (opts & FileListOptions.Recursive) != 0;
        var includeDir = (opts & FileListOptions.Directory) != 0;

        foreach (DirectoryInfo di in dirInfo.GetDirectories())
        {
            var dirPath = FileUtils.combine(path, di.Name);
            var isFile = !raw ? !isDirName(di.Name) : false;

            if (raw && includeDir ||
                !raw && (includeDir && !isFile || includeFile && isFile))
            {
                FileStats stats = null;
                if (getInfo)
                {
                    stats = getFileStats(dirPath);
                }
                paths.Add(action(createPath(dirPath, isFile), stats));
            }
            if (recursive && (!isFile || raw))
            {
                listDirectory(paths, dirPath + "/", opts, action, getInfo);
            }
        }
    }

    protected virtual string searchPattern
    {
        get { return "*"; }
    }

    public string getRootPath()
    {
        return m_root;
    }

    /// <summary>
    /// convert a relative path to absolute path
    /// </summary>
    /// <param name="path">relative path, should not be prefixed with '/'</param>
    public string getAbsPath(string path)
    {
        if (path == null)
        {
            throw new ArgumentNullException("path");
        }
        return getRootPath() + path;
    }

    public void setFileStats(string path, DateTime creationTimeUtc, DateTime writeTimeUtc)
    {
        var absPath = getAbsPath(path);
        if (Directory.Exists(absPath) || File.Exists(absPath))
        {
            var stats = getFileStats(path);
            stats.creationTime = creationTimeUtc;
            stats.writeTime = writeTimeUtc;
            onUpdatedFileStats(path, stats);
        }
        else
        {
            throw new IOException("invalid path " + path);
        }
    }

    public void setLastWriteTime(string path, DateTime dateTimeUtc)
    {
        var absPath = getAbsPath(path);
        if (Directory.Exists(absPath) || File.Exists(absPath))
        {
            var stats = getFileStats(path);
            stats.writeTime = dateTimeUtc;
            onUpdatedFileStats(path, stats);
        }
        else
        {
            throw new IOException("invalid path " + path);
        }
    }

    public virtual FileNameValidationResult validateFileName(string name)
    {
        if (FileUtils.fileNameContainsInvalidChars(name))
        {
            return FileNameValidationResult.InvalidChar;
        }
        if (FileUtils.isReservedFileName(name))
        {
            return FileNameValidationResult.ReservedName;
        }
        if (name.Length > MaxFileNameLength)
        {
            return FileNameValidationResult.FileNameTooLong;
        }
        return FileNameValidationResult.NoError;
    }

    public abstract IRepositoryPath createPath(string path, bool isFile);

    public abstract string makeDirName(string logicalName);

    public abstract bool isDirName(string rawName);

    public abstract string getLogicalDirName(string rawName);

    public IRepositoryPath createFilePath(string path)
    {
        return createPath(path, true);
    }

    public IRepositoryPath createDirPath(string path)
    {
        return createPath(path, false);
    }
}
