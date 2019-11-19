using LitJson;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Networking;

public class AndroidUpdateService : MonoBehaviour
{
    /// <summary>
    /// The base url for android packages
    /// </summary>
    public string baseUrl;

    public event Action<string> onCheckVersionCompleted;
    public event Action onDownloadCancelled;
    public event Action<string> onDownloadError;
    public event Action<float> onDownloadProgress;

    public enum State
    {
        Idle,
        CheckingVersion,
        Downloading,
    }

    private State m_state = State.Idle;
    private bool m_cancelled;
    private FileDownloader m_fileDownloader;
    private object m_cancelLock = new object();

    private class DownloadState
    {
        public int totalSize;

        public void Reset()
        {
            totalSize = 0;
        }
    }

    public class VersionInfo
    {
        public string version;
        public string apkPath;
        public string hash;
        public string downloadUrl;
    }

    private VersionInfo m_versionInfo;

    void Start()
    {
        RemoveOldApks();
    }

    private void RemoveOldApks()
    {
        var currentVersionNum = new System.Version(Application.version);
        try
        {
            string path = GetUpdateDirectory();
            foreach (var file in Directory.GetFiles(path, "*.apk"))
            {
                var m = Regex.Match(file, @"^.*(\d+\.\d+\.\d+).*$");
                if (m.Success)
                {
                    var version = new System.Version(m.Groups[1].Value);
                    if (version <= currentVersionNum)
                    {
                        string apkPath = Path.Combine(path, file);
                        File.Delete(apkPath);

                        Debug.Log("removed " + apkPath);
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning(e.Message);
        }
    }

    public void CheckForNewVersion()
    {
        if (m_state != State.Idle)
        {
            Debug.Log("cannot start version checking");
            return;
        }

        StartCoroutine(CheckVersion());
    }

    IEnumerator CheckVersion()
    {
        m_state = State.CheckingVersion;

        var timedRequest = new TimedUnityWebRequest(UnityWebRequest.Get(baseUrl + "/version.txt"));
        yield return timedRequest;

        string error = null;
        try
        {
            if (!string.IsNullOrEmpty(timedRequest.error))
            {
                Debug.LogError("version check error: " + timedRequest.error);
                error = timedRequest.error;
                yield break;
            }

            m_versionInfo  = JsonMapper.ToObject<VersionInfo>(timedRequest.rawRequest.downloadHandler.text);

            Debug.Log("latest version: " + latestVersion);
        }
        catch (Exception e)
        {
            error = e.Message;
        }
        finally
        {
            m_state = State.Idle;
            if (onCheckVersionCompleted != null)
            {
                onCheckVersionCompleted(error);
            }
            timedRequest.Dispose();
        }
    }

    /// <summary>
    /// download and install the latest version, should be called after version checking succeeds
    /// </summary>
    public void InstallLatestVersion()
    {
        if (latestVersion == null)
        {
            Debug.LogWarning("check for new version first");
            return;
        }

        if (m_state != State.Idle)
        {
            Debug.Log("cannot start downloading");
            return;
        }

        m_state = State.Downloading;
        m_cancelled = false;

        try
        {
            PrepareDownload(GetUpdateDirectory());
        }
        catch (Exception e)
        {
            m_state = State.Idle;

            Debug.LogException(e);
            if (onDownloadError != null)
            {
                onDownloadError(e.Message);
            }
        }
    }

    string GetUpdateDirectory()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        using (var envClass = new AndroidJavaClass("android.os.Environment"))
        {
            var state = envClass.CallStatic<string>("getExternalStorageState");
            if (state != "mounted")
            {
                throw new IOException("external storage not mounted");
            }

            using (var activity = Utils.GetUnityActivity())
            using (var fileDir = activity.Call<AndroidJavaObject>("getExternalFilesDir", null))
            {
                fileDir.Call<bool>("mkdirs");
                return fileDir.Call<string>("getPath") + "/.update";
            }
        }
#else
        return Application.persistentDataPath + "/.update";
#endif
    }

    void PrepareDownload(string downloadDir)
    {
        if (!Directory.Exists(downloadDir))
        {
            Directory.CreateDirectory(downloadDir);
        }

        string apkName = Path.GetFileName(m_versionInfo.apkPath);
        DownloadState downloadState = null;
        var apkPath = downloadDir + "/" + apkName;
        var stateFilePath = apkPath + ".cfg";

        // get the download state
        if (File.Exists(apkPath))
        {
            if (File.Exists(stateFilePath))
            {
                try
                {
                    downloadState = JsonMapper.ToObject<DownloadState>(File.ReadAllText(stateFilePath));
                }
                catch (Exception e)
                {
                    Debug.LogWarning("failed to deserialize download state, restart download: " + e.Message);

                    File.Delete(stateFilePath);
                    File.Delete(apkPath);
                }
            }
        }

        ThreadPool.QueueUserWorkItem(delegate {
            ResumeDownload(apkPath, stateFilePath, downloadState);
        });
    }

    void ResumeDownload(string apkPath, string stateFilePath, DownloadState state)
    {
        Debug.Log("downloading to " + apkPath);

        try
        {
            long resumePosition = 0;
            // validate the size of downloaded file
            if (state != null)
            {
                var apkInfo = new FileInfo(apkPath);
                if (apkInfo.Length == state.totalSize)
                {
                    if (Md5.HashFile(apkPath).Equals(m_versionInfo.hash, StringComparison.InvariantCultureIgnoreCase))
                    {
                        try
                        {
                            if (onDownloadProgress != null)
                            {
                                onDownloadProgress(1.0f);
                            }
                        }
                        catch (Exception e)
                        {
                            Debug.LogException(e);
                        }

                        Debug.Log("hash match, use downloaded apk");

                        m_state = State.Idle;
                        InstallApk(apkPath);
                        return;
                    }
                    else
                    {
                        Debug.LogError("hash not match, restart download");
                    }
                }
                else if (apkInfo.Length > state.totalSize)
                {
                    Debug.LogError("size too large, restart download");
                }
                else
                {
                    resumePosition = apkInfo.Length;
                }
            }

            if (m_cancelled)
            {
                m_state = State.Idle;
                FireCancelledCallback();
                return;
            }

            var downloadUrl = string.Format("{0}/{1}", baseUrl, m_versionInfo.apkPath);
            var downloader = new FileDownloader();

            downloader.DownloadStarted += (sender, e) => {
                if (m_cancelled)
                {
                    downloader.Cancel();
                    return;
                }

                if (state == null)
                {
                    state = new DownloadState {
                        totalSize = (int)e.TotalBytesToReceive
                    };
                    File.WriteAllText(stateFilePath, JsonMapper.ToJson(state));
                }
                Debug.Log("download started, resumed: " + e.Resumed);
            };

            downloader.DownloadFileCompleted += (sender, e) => {
                m_state = State.Idle;
                lock (m_cancelLock)
                {
                    m_fileDownloader = null;
                }

                if (e.Cancelled)
                {
                    FireCancelledCallback();
                }
                else if (e.Error == null)
                {
                    try
                    {
                        if (Md5.HashFile(apkPath) != m_versionInfo.hash)
                        {
                            Debug.LogError("file has been downloaded but hash does not match");

                            File.Delete(apkPath);
                            File.Delete(stateFilePath);

                            FireErrorCallback("file corrupted");
                        }
                        else
                        {
                            InstallApk(apkPath);
                        }
                    }
                    catch (Exception ex)
                    {
                        FireErrorCallback(ex.Message);
                    }
                }
                else
                {
                    FireErrorCallback(e.Error.Message);
                }
            };

            float lastProgress = 0.0f;
            downloader.DownloadProgressChanged += (sender, evtArgs) => {
                if (lastProgress != evtArgs.Progress)
                {
                    CallbackQueue.instance.Enqueue(() => {
                        if (onDownloadProgress != null)
                        {
                            onDownloadProgress(evtArgs.Progress);
                        }
                    });
                }
            };

            lock (m_cancelLock)
            {
                if (m_cancelled)
                {
                    m_state = State.Idle;
                    FireCancelledCallback();
                }
                else
                {
                    m_fileDownloader = downloader;
                    downloader.Start(downloadUrl, apkPath, (int)resumePosition);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);

            m_state = State.Idle;
            FireErrorCallback(e.Message);
        }
    }

    void FireCancelledCallback()
    {
        CallbackQueue.instance.Enqueue(() => {
            if (onDownloadCancelled != null)
            {
                onDownloadCancelled();
            }
        });
    }

    void FireErrorCallback(string error)
    {
        CallbackQueue.instance.Enqueue(() => {
            if (onDownloadError != null)
            {
                onDownloadError(error);
            }
        });
    }

    void InstallApk(string apkPath)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        CallbackQueue.instance.Enqueue(() => {
            try
            {
                using (var intent = new AndroidJavaObject("android.content.Intent", "android.intent.action.VIEW"))
                using (var uriClass = new AndroidJavaClass("android.net.Uri"))
                using (var uri = uriClass.CallStatic<AndroidJavaObject>("parse", "file://" + apkPath))
                using (var activity = Utils.GetUnityActivity())
                {
                    intent.Call<AndroidJavaObject>("setDataAndType", uri, "application/vnd.android.package-archive");
                    // FLAG_ACTIVITY_NEW_TASK
                    intent.Call<AndroidJavaObject>("setFlags", 0x10000000);
                    activity.Call("startActivity", intent);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        });
#endif
    }

    public void CancelDownload()
    {
        if (m_state == State.Downloading && !m_cancelled)
        {
            m_cancelled = true;

            lock (m_cancelLock)
            {
                if (m_fileDownloader != null)
                {
                    m_fileDownloader.Cancel();
                    m_fileDownloader = null;
                }
            }
        }
    }

    /// <summary>
    /// return the latest version number, only available after version checking finishes
    /// </summary>
    public string latestVersion
    {
        get { return m_versionInfo != null ? m_versionInfo.version : null; }
    }

    public string downloadPath {
        get { return m_versionInfo != null ? m_versionInfo.downloadUrl : null; }
    }

    /// <summary>
    /// return true if update is available. Always returns false before version checking finishes.
    /// </summary>
    public bool hasUpdate
    {
        get
        {
            if (m_versionInfo != null)
            {
                return new System.Version(m_versionInfo.version) > new System.Version(Application.version);
            }
            return false;
        }
    }
}
