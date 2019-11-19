using Google.Protobuf.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using g_WebRequestManager = Singleton<WebRequestManager>;

public class ProjectSynchronizer
{
    private static readonly A8.Logger s_logger = A8.Logger.GetLogger<ProjectSynchronizer>();

    public event Action onGetFileListFailed;
    public event Action onProgressChanged;
    public event Action onDownloadFinished;

    private readonly ProjectRepository m_projectRepository;
    private readonly GetCatalogType m_catalogType;
    private readonly List<FileNode> m_failedDownloads = new List<FileNode>();
    private RepeatedField<FileNode> serverFiles;

    private string rootPath;
    public ProjectSynchronizer(ProjectRepository projectRepository, GetCatalogType type)
    {
        if (projectRepository == null)
        {
            throw new ArgumentNullException("projectRepository");
        }
        m_projectRepository = projectRepository;
        m_catalogType = type;
    }

    public int totalFilesToSync
    {
        get;
        protected set;
    }

    public int successfulSyncs
    {
        get;
        protected set;
    }

    public bool finished
    {
        get { return totalFilesToSync == successfulSyncs; }
    }

    public void Synchronize()
    {
        totalFilesToSync = 0;
        successfulSyncs = 0;
        m_failedDownloads.Clear();

        var request = new ListFilesRequest();
        request.type = m_catalogType;
        request.blocking = false;
        request.defaultErrorHandling = false;
        request.Success(res => {
            rootPath = res.RootPath;
            serverFiles = res.FileList_;

            var filesToSync = new List<FileNode>();
            var localFiles = m_projectRepository.listDirectory("",
                FileListOptions.Raw | FileListOptions.Recursive | FileListOptions.FileOrDir);

            foreach (var projectNode in serverFiles)
            {
                var localFile = localFiles.Find((x) => { return x.ToString() == projectNode.PathName; });
                if (localFile == null)
                {
                    filesToSync.Add(projectNode);
                }
                else if ((FN_TYPE)projectNode.FnType == FN_TYPE.FnFile)
                {
                    byte[] data = m_projectRepository.loadFile("", localFile.ToString());
                    if (data == null || !Md5.CreateMD5Hash(data).ToLower().Equals(projectNode.FileMd5))
                    {
                        filesToSync.Add(projectNode);
                    }
                    else
                    {
                        localFiles.Remove(localFile);
                    }
                }
                else
                {
                    localFiles.Remove(localFile);
                }
            }

            foreach (var path in localFiles)
            {
                m_projectRepository.delete(path);
            }

            if (filesToSync.Count > 0)
            {
                Synchronize(filesToSync);
            }
            else
            {
                DownloadFinished();
            }
        })
        .Error(onGetFileListFailed)
        .Execute();
    }

    private void Synchronize(List<FileNode> files)
    {
        totalFilesToSync = 0;

        foreach (var proj in files)
        {
            if ((FN_TYPE)proj.FnType == FN_TYPE.FnFile)
            {
                totalFilesToSync++;
                Synchronize(proj);
            }
            else
            {
                m_projectRepository.createDirectory(proj.PathName, TimeUtils.FromEpochSeconds((long)proj.CreateTime));
            }
        }
        CheckFinished();
    }

    private void Synchronize(FileNode file)
    {
        var request = Downloads.Download(m_catalogType, rootPath + "/" + Uri.EscapeDataString(file.Base64PathName), UserManager.Instance.UserId);
        request.blocking = false;
        request.defaultErrorHandling = false;
        request.Success(data => {
                m_projectRepository.save(file.PathName, data);

                ++successfulSyncs;
                if (onProgressChanged != null)
                {
                    onProgressChanged();
                }
                CheckFinished();
            })
            .Error(() => {
                m_failedDownloads.Add(file);
                CheckFinished();
            })
            .Execute();
    }

    private void CheckFinished()
    {
        if (m_failedDownloads.Count + successfulSyncs == totalFilesToSync)
        {
            DownloadFinished();
        }
    }

    private void DownloadFinished()
    {
        foreach (var fileInfo in serverFiles)
        {
            try
            {
                m_projectRepository.setFileStats(fileInfo.PathName,
                    TimeUtils.FromEpochSeconds((long)fileInfo.CreateTime),
                    TimeUtils.FromEpochSeconds((long)fileInfo.UpdateTime));
            }
            catch (Exception e)
            {
                s_logger.LogException(e);
            }
        }

        if (onDownloadFinished != null)
        {
            onDownloadFinished();
        }
    }

    public void RetryFailedDownloads()
    {
        foreach (var proj in m_failedDownloads)
        {
            Synchronize(proj);
        }
        m_failedDownloads.Clear();
    }
}
