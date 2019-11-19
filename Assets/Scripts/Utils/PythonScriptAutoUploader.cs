using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class PythonScriptAutoUploader : IDisposable
{
    public event Action<string> onFileUploaded;

    private static readonly A8.Logger s_logger = A8.Logger.GetLogger<PythonScriptAutoUploader>();

    private FileSystemWatcher m_watcher = new FileSystemWatcher();

    private HttpRequest m_uploadRequest;
    private string m_curUploadingPath;

    private readonly List<FileSystemEventArgs> m_events = new List<FileSystemEventArgs>();
    // path to hash
    // hash is null when path points to a directory
    private readonly Dictionary<string, string> m_fileCache = new Dictionary<string, string>();

    public static readonly PythonScriptAutoUploader instance = new PythonScriptAutoUploader();

    private PythonScriptAutoUploader()
    {
        m_watcher.Changed += OnFileSystemEvent;
        m_watcher.Created += OnFileSystemEvent;
        m_watcher.NotifyFilter = NotifyFilters.LastWrite;
        m_watcher.IncludeSubdirectories = true;
    }

    public bool isUploading
    {
        get { return m_events.Count > 0; }
    }

    // call this before deleting a local path
    public void PathRemoved(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            throw new ArgumentException("path");
        }

        CancelUpload(path);
        foreach (var subPath in m_fileCache.Keys.Where(x => FileUtils.isParentPath(path, x)).ToList())
        {
            m_fileCache.Remove(subPath);
        }
    }

    public void DirectoryCreated(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            throw new ArgumentNullException("path");
        }

        CancelUpload(path);
        m_fileCache[path] = null;
    }

    // call this before creating or making changes to a file
    public void FileChanged(string path, byte[] data)
    {
        if (string.IsNullOrEmpty(path))
        {
            throw new ArgumentException("path");
        }

        if (data == null)
        {
            throw new ArgumentNullException("data");
        }

        CancelUpload(path);
        m_fileCache[path] = Md5.CreateMD5Hash(data);
    }

    public void CancelUpload(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            throw new ArgumentException("path");
        }

        if (m_curUploadingPath == path)
        {
            m_uploadRequest.Abort();
            m_uploadRequest = null;
            m_curUploadingPath = null;
        }
        m_events.RemoveAll(x => GetRelativePath(x.FullPath) == path);
    }

    public void Start()
    {
        if (!m_watcher.EnableRaisingEvents)
        {
            m_watcher.Path = PythonRepository.instance.getRootPath();
            m_watcher.EnableRaisingEvents = true;
            BuildCache();

            PythonRepository.instance.onCreatingDirectory += DirectoryCreated;
            PythonRepository.instance.onDeletingProject += PathRemoved;
            PythonRepository.instance.onSavingFile += FileChanged;
        }
    }

    private void BuildCache()
    {
        m_fileCache.Clear();
        var paths = PythonRepository.instance.listDirectory("", FileListOptions.FileOrDir | FileListOptions.Recursive);
        foreach (var path in paths)
        {
            string hash = null;
            if (path.isFile)
            {
                var fullPath = PythonRepository.instance.getAbsPath(path.ToString());
                try
                {
                    hash = Md5.HashFile(fullPath);
                }
                catch (IOException e)
                {
                    s_logger.LogError(e);
                    continue;
                }
            }

            m_fileCache.Add(path.ToString(), hash);
        }
    }

    public void Stop()
    {
        m_watcher.EnableRaisingEvents = false;
        if (m_uploadRequest != null)
        {
            m_uploadRequest.Abort();
            m_uploadRequest = null;
        }
        m_events.Clear();
        m_curUploadingPath = null;

        PythonRepository.instance.onCreatingDirectory -= DirectoryCreated;
        PythonRepository.instance.onDeletingProject -= PathRemoved;
        PythonRepository.instance.onSavingFile -= FileChanged;
    }

    public void Dispose()
    {
        if (m_watcher != null)
        {
            Stop();
            m_watcher.Changed -= OnFileSystemEvent;
            m_watcher.Created -= OnFileSystemEvent;
            m_watcher.Dispose();
        }
    }

    private void OnFileSystemEvent(object sender, FileSystemEventArgs args)
    {
        if (args.ChangeType != WatcherChangeTypes.Created &&
            args.ChangeType != WatcherChangeTypes.Changed)
        {
            return;
        }

        CallbackQueue.instance.Enqueue(() => {
            m_events.Add(args);
            UploadFile();
        });
    }

    private void UploadFile()
    {
        if (m_uploadRequest == null && m_events.Count > 0)
        {
            var curEvent = m_events[0];
            m_events.RemoveAt(0);

            if (curEvent.ChangeType == WatcherChangeTypes.Created)
            {
                m_uploadRequest = ProcessPathCreated(curEvent.FullPath);
            }
            else if (curEvent.ChangeType == WatcherChangeTypes.Changed)
            {
                m_uploadRequest = ProcessPathChanged(curEvent.FullPath);
            }

            if (m_uploadRequest != null)
            {
                m_uploadRequest.blocking = false;
                m_uploadRequest.errorPrompt = "ui_python_background_uploading_failed".Localize();
                m_uploadRequest.Finally(() => {
                    m_uploadRequest = null;
                    m_curUploadingPath = null;
                    UploadFile();
                })
                .Execute();
            }
            else
            {
                UploadFile();
            }
        }
    }

    private static string GetRelativePath(string path)
    {
        var relativePath = path.Substring(PythonRepository.instance.getRootPath().Length);
        return relativePath.Replace("\\", "/");
    }

    private UploadFileRequest ProcessPathCreated(string path)
    {
        var relPath = GetRelativePath(path);
        if (m_fileCache.ContainsKey(relPath))
        {
            s_logger.Log("{0} already cached, ignore Created event", relPath);
            return null;
        }

        if (File.Exists(path))
        {
            var data = PythonRepository.instance.loadFile(relPath);
            if (data != null)
            {
                return UploadFileData(relPath, data);
            }
            else
            {
                s_logger.Log("Failed to load {0}", relPath);
            }
        }
        else if (Directory.Exists(path))
        {
            s_logger.Log("creating folder at " + relPath);

            var request = Uploads.CreateFolder(relPath);
            request.type = GetCatalogType.PYTHON;
            request.Success(() => {
                s_logger.Log("created " + relPath);
                m_fileCache.Add(relPath, null);

                PythonRepository.instance.setFileStats(relPath, request.creationTime, request.creationTime);
                if (onFileUploaded != null)
                {
                    onFileUploaded(relPath);
                }
            });
            return request;
        }
        return null;
    }

    private UploadFileRequest UploadFileData(string relPath, byte[] data)
    {
        s_logger.Log("uploading {0}", relPath);

        var request = new UploadFileRequest();
        request.type = GetCatalogType.PYTHON;
        request.AddFile(relPath, data);
        request.Success(() => {
            s_logger.Log("uploaded {0}", relPath);

            m_fileCache[relPath] = Md5.Hash(data);

            var fileNode = request.files.GetFile(relPath);
            PythonRepository.instance.setFileStats(fileNode.PathName,
                TimeUtils.FromEpochSeconds((long)fileNode.CreateTime),
                TimeUtils.FromEpochSeconds((long)fileNode.UpdateTime));

            if (onFileUploaded != null)
            {
                onFileUploaded(relPath);
            }
        });
        return request;
    }

    private HttpRequest ProcessPathChanged(string path)
    {
        var relPath = GetRelativePath(path);
        if (File.Exists(path) && m_fileCache.ContainsKey(relPath))
        {
            var oldHash = m_fileCache[relPath];
            if (oldHash == null)
            {
                return null;
            }

            var data = PythonRepository.instance.loadFile(relPath);
            if (data != null)
            {
                var newHash = Md5.Hash(data);
                if (newHash != oldHash)
                {
                    return UploadFileData(relPath, data);
                }
                else
                {
                    s_logger.Log("ignore Changed event, content not changed " + relPath);
                }
            }
            else
            {
                s_logger.LogError("failed to load {0}", relPath);
            }
        }

        return null;
    }
}
