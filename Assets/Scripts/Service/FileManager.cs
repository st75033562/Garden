using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
#if UNITY_5_4_OR_NEWER
using UnityEngine.Networking;
#else
using UnityEngine.Experimental.Networking;
#endif

public delegate void TextureLoadingComplete(Texture tex, string error);
public delegate void DataLoadingComplete(byte[] data, string error);
public delegate void DataSavingComplete(string error);

public class FileManager : MonoBehaviour, IService
{
    public class FileRequest : CustomYieldInstruction
    {
        internal UnityEngine.Networking.UnityWebRequest webRequest;
        private bool m_aborted;

        internal virtual void onRequestCompleted() { }

        public bool isDone
        {
            get
            {
                if (m_aborted) { return true; }
                if (webRequest != null) { return webRequest.isDone; }
                return false;
            }
        }

        public bool isError
        {
            get
            {
                if (m_aborted) { return true; }
                if (webRequest != null) { return webRequest.isError; }
                return false;
            }
        }

        public string error
        {
            get
            {
                if (m_aborted) { return "aborted"; }
                if (webRequest != null) { return webRequest.error; }
                return null;
            }
        }

        public bool isAborted
        {
            get { return m_aborted; }
        }

        public void abort()
        {
            m_aborted = true;
            if (webRequest != null && !m_aborted)
            {
                webRequest.Abort();
                webRequest.Dispose();
            }
        }

        public override bool keepWaiting
        {
            get { return !isDone; }
        }
    }

    private class TextureRequest : FileRequest
    {
        public TextureLoadingComplete onLoaded;

        public TextureRequest(string url)
        {
            webRequest = UnityEngine.Networking.UnityWebRequest.GetTexture(url);
        }

        internal override void onRequestCompleted()
        {
            if (onLoaded != null)
            {
                UnityEngine.Networking.DownloadHandlerTexture handler = webRequest.downloadHandler as UnityEngine.Networking.DownloadHandlerTexture;
                onLoaded(!isError ? handler.texture : null, error);
            }
        }
    }

    private class DataRequest : FileRequest
    {
        public DataLoadingComplete onLoaded;

        public DataRequest(string url)
        {
            webRequest = UnityEngine.Networking.UnityWebRequest.Get(url);
        }

        internal override void onRequestCompleted()
        {
            if (onLoaded != null)
            {
                onLoaded(!isError ? webRequest.downloadHandler.data : null, error);
            }
        }
    }

    // To workaround bug of high cpu usage in editor mode
#if UNITY_EDITOR
    private const int MaxConcurrentRequests = 1;
#else
    private const int MaxConcurrentRequests = int.MaxValue;
#endif

    private List<FileRequest> m_loadingRequests = new List<FileRequest>();
    private List<FileRequest> m_savingRequests = new List<FileRequest>();

    private Queue<FileRequest> m_pendingLoadingRequests;

    public void init()
    {
    }

    // gracefully shutdown the manager
    public Coroutine shutdown()
    {
        return StartCoroutine(shutdownImpl());
    }

    private IEnumerator shutdownImpl()
    {
        // wait for all loading requests
        for (int i = 0; i < m_loadingRequests.Count; ++i)
        {
            var request = m_loadingRequests[i];
            while (!request.isDone)
            {
                yield return null;
            }
            request.webRequest.Dispose();
        }
        m_loadingRequests.Clear();

        for (int i = 0; i < m_savingRequests.Count; ++i)
        {
            var request = m_savingRequests[i];
            while (!request.isDone)
            {
                yield return null;
            }
            request.webRequest.Dispose();
        }
        m_savingRequests.Clear();

        //Debug.Log("file manager shutdown");
    }

    // reset the manager, cancel all pending operations
    public void reset()
    {
        for (int i = 0; i < m_loadingRequests.Count; ++i)
        {
            m_loadingRequests[i].abort();
        }
        m_loadingRequests.Clear();

        for (int i = 0; i < m_savingRequests.Count; ++i)
        {
            m_savingRequests[i].abort();
        }
        m_savingRequests.Clear();
        StopAllCoroutines();
    }

    public bool isLoading(string path)
    {
        return getLoadingRequest(path) != null;
    }

    public FileRequest loadTextureAsync(string path, TextureLoadingComplete done = null)
    {
        string filePath = getFilePath(path);
        var request = new TextureRequest(filePath);
        enqueueLoadingRequest(request);
        request.onLoaded = done;
        return request;
    }

    private void enqueueLoadingRequest(FileRequest request)
    {
        if (currentRequestCount == MaxConcurrentRequests)
        {
            if (m_pendingLoadingRequests == null)
            {
                m_pendingLoadingRequests = new Queue<FileRequest>();
            }
            m_pendingLoadingRequests.Enqueue(request);
        }
        else
        {
            request.webRequest.Send();
            m_loadingRequests.Add(request);
        }
    }

    private int currentRequestCount
    {
        get { return m_loadingRequests.Count + m_savingRequests.Count; }
    }

    public FileRequest loadDataAsync(string path, DataLoadingComplete done = null)
    {
        string filePath = getFilePath(path);
        var request = new DataRequest(filePath);
        enqueueLoadingRequest(request);
        request.onLoaded = done;
        return request;
    }

    private static string getFilePath(string path)
    {
        return "file://" + Application.persistentDataPath + "/" + path;
    }

    private FileRequest getLoadingRequest(string path)
    {
        return m_loadingRequests.Find(x => x.webRequest.url == path);
    }

    public FileRequest saveAsync(string path, byte[] data, object userData, DataSavingComplete done = null)
    {
        FileRequest req = new FileRequest();
        StartCoroutine(saveImpl(req, path, data, userData, done));
        return req;
    }

    private IEnumerator saveImpl(FileRequest req, string path, byte[] data, object userData, DataSavingComplete done)
    {
        yield return null;

        while (currentRequestCount == MaxConcurrentRequests)
        {
            yield return null;
        }

        if (req.isAborted)
        {
            yield break;
        }

        string error = createDirectory(Application.persistentDataPath + "/" + path);
        if (!string.IsNullOrEmpty(error))
        {
            if (done != null)
            {
                done(error);
            }
            yield break;
        }

        var request = UnityEngine.Networking.UnityWebRequest.Put(getFilePath(path), data);
        req.webRequest = request;
        m_savingRequests.Add(req);

        yield return request.Send();

        try
        {
            if (done != null)
            {
                done(request.error);
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }

        request.Dispose();
        m_savingRequests.Remove(req);
    }

    private static string createDirectory(string path)
    {
        // create the folder if not exists
        var dir = Path.GetDirectoryName(path);
        if (dir != string.Empty && !Directory.Exists(dir))
        {
            try
            {
                Directory.CreateDirectory(dir);
                return null;
            }
            catch (IOException e)
            {
                Debug.LogException(e);
                return e.Message;
            }
        }

        return null;
    }

    private void Update()
    {
        for (int i = m_loadingRequests.Count - 1; i >= 0; --i)
        {
            var request = m_loadingRequests[i];
            if (request.webRequest.isDone)
            {
                if (!request.isAborted)
                {
                    try
                    {
                        request.onRequestCompleted();
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                    request.webRequest.Dispose();
                }
                m_loadingRequests.RemoveAt(i);
            }
        }

        while (currentRequestCount < MaxConcurrentRequests && 
            m_pendingLoadingRequests != null && m_pendingLoadingRequests.Count > 0)
        {
            var request = m_pendingLoadingRequests.Dequeue();
            request.webRequest.Send();
            m_loadingRequests.Add(request);
        }
    }
}
