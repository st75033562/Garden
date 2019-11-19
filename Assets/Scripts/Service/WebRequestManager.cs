using UnityEngine;
using System;
using System.Collections.Generic;
using System.Net;

public class WebRequestData
{
    public enum Method
    {
        Get,
        Post
    }

	public int m_ID = -1;
	public Action<ResponseData, object> m_SuccessCallBack;
	public Action<WebRequestData> m_failCallBack;

    private bool m_useDefaultErrorHandling;
    private bool m_blocking;
    private readonly Dictionary<string, string> m_headers = new Dictionary<string, string>();
    private WebRequestManager m_requestManager;

    public Action<float> m_ProgressCallback;
	public object m_Param;

    public WebRequestData()
        : this(WebRequestManager.Default)
    {
    }

    public WebRequestData(WebRequestManager requestManager)
    {
        if (requestManager == null)
        {
            throw new ArgumentNullException("requestManager");
        }

        this.requestManager = requestManager;
        maxRetryCount = requestManager.maxRetryCount;
    }

    public Dictionary<string, string> headers { get { return m_headers; } }

    public void Header(string key, string value)
    {
        headers[key] = value;
    }

    /// <summary>
    /// the request method. the default method is GET
    /// </summary>
    public Method method
    {
        get { return postData != null ? Method.Post : Method.Get; }
    }

    public byte[] postData { get; set; }

    public string url { get; set; }

    public void SetPath(string path)
    {
        if (path == null)
        {
            throw new ArgumentNullException("path");
        }
        url = requestManager.UrlHost + path;
    }

    public int maxRetryCount { get; set; }

    public WebRequestManager requestManager
    {
        get { return m_requestManager; }
        set
        {
            m_requestManager = value ?? WebRequestManager.Default;
        }
    }

    public bool useDefaultErrorHandling
    {
        get { return m_useDefaultErrorHandling; }
        set
        {
            if (m_ID != -1)
            {
                throw new InvalidOperationException();
            }
            m_useDefaultErrorHandling = value;
        }
    }

    public string defaultErrorPrompt { get; set; }

    public bool blocking
    {
        get { return m_blocking; }
        set
        {
            if (m_ID != -1)
            {
                throw new InvalidOperationException();
            }
            m_blocking = value;
        }
    }

    public void Abort()
    {
        if (aborted) { return; }

        requestManager.RemoveRequest(this);
    }

    public void Execute()
    {
        requestManager.AddTask(this);
    }

    public ResponseData response { get; internal set; }

    public bool aborted { get; internal set; }

    internal void InternalAbort()
    {
        aborted = true;
        response = null;
        if (client != null)
        {
            client.Abort();
        }
    }

    // for internal use
    internal int attempCount = -1;
    internal bool autoRetry;
    internal IHttpClient client;
}

public interface IHttpAuthErrorHandler
{
    void Handle(Action resolved);
}

public interface IHttpTaskErrorDefaultHandler
{
    void Handle(WebRequestData task);
}

public interface IHttpClientFactory
{
    IHttpClient Create();
}

public class DefaultHttpClientFactory : IHttpClientFactory
{
    public IHttpClient Create()
    {
        return new HttpClient();
    }
}

public partial class WebRequestManager : MonoBehaviour
{
    private static readonly A8.Logger s_logger = A8.Logger.GetLogger<WebRequestManager>();

	const int c_MaxActionCount = 5;
	const int c_MaxTryCount = 2;

    private int m_NextTaskID = 0;

	private readonly object m_responseLock = new object();
	private readonly Queue<ResponseData> m_responses = new Queue<ResponseData>();

	private readonly List<WebRequestData> m_pendingRequests = new List<WebRequestData>();
	private readonly List<WebRequestData> m_executingRequests = new List<WebRequestData>();
    private readonly List<WebRequestData> m_authErrorTasks = new List<WebRequestData>();

    private bool m_authError;

    public static WebRequestManager Default
    {
        get { return Singleton<WebRequestManager>.instance; }
    }

    public string UrlHost { get; set; }

    public bool hasRequests
    {
        get { return m_pendingRequests.Count > 0 || m_executingRequests.Count > 0 || m_authErrorTasks.Count > 0; }
    }

    public IHttpAuthErrorHandler authErrorHandler { get; set; }

    public IHttpTaskErrorDefaultHandler taskErrorDefaultHandler { get; set; }

    public IHttpClientFactory httpClientFactory { get; set; }

    /// <summary>
    /// called before request is made
    /// </summary>
    public Action<WebRequestData> beforeRequest { get; set; }

    /// <summary>
    /// called when the request is aborted or completes without error
    /// </summary>
    public Action<WebRequestData> onRequestComplete { get; set; }

    /// <summary>
    /// the default max retry count
    /// </summary>
    public int maxRetryCount { get; set; }

    WebRequestManager()
    {
        httpClientFactory = new DefaultHttpClientFactory();
        maxRetryCount = c_MaxTryCount;
    }

	void Update()
	{
        lock (m_responseLock)
        {
            while (m_responses.Count > 0)
            {
                ResponseData data = m_responses.Dequeue();
                DealWithHttpClientBack(data);
            }
        }

        // do not process pending requests until auth error is resolved
        while (m_pendingRequests.Count > 0 && !m_authError)
        {
            var request = m_pendingRequests[0];
            if (request.aborted)
            {
                m_pendingRequests.RemoveAt(0);
                NotifyRequestComplete(request);
                continue;
            }

            if (m_executingRequests.Count == c_MaxActionCount)
            {
                break;
            }

            // only fire event when this is the first attempt or manual
            if ((request.attempCount == -1 || !request.autoRetry) && beforeRequest != null)
            {
                beforeRequest(request);
            }

            request.autoRetry = false;
            if (request.attempCount == -1)
            {
                request.attempCount = 1;
            }
            else
            {
                ++request.attempCount;
            }

            request.response = null;

            var client = httpClientFactory.Create();
            request.client = client;
			m_executingRequests.Add(request);

			if (request.method == WebRequestData.Method.Get)
			{
                client.Get(request.url, request, AddResponse, request.headers);
			}
			else
			{
                client.Post(request.url, request.postData, request, AddResponse, request.headers);
			}
			m_pendingRequests.RemoveAt(0);
        }

        for (int i = 0; i < m_executingRequests.Count; ++i)
        {
            var workItem  = m_executingRequests[i];
            if (workItem.m_ProgressCallback != null && 
                workItem.client.ProgressChanged && 
                workItem.client.Progress >= 0.0f)
            {
                workItem.client.ProgressChanged = false;
                workItem.m_ProgressCallback(workItem.client.Progress);
            }
        }
	}

	private void AddResponse(ResponseData data)
	{
		lock (m_responseLock)
		{
			m_responses.Enqueue(data);
		}
	}

	void DealWithHttpClientBack(ResponseData response)
	{
		var request = (WebRequestData)response.parameter;
        m_executingRequests.Remove(request);
        request.response = response;

		if (string.IsNullOrEmpty(response.error))
		{
            NotifyRequestComplete(request);

			if (request.m_SuccessCallBack != null)
			{
                if (request.m_SuccessCallBack.IsSafeToInvoke())
                {
                    request.m_SuccessCallBack(response, request.m_Param);
                }
                else
                {
                    s_logger.Log("ignore success callback, mono object destroyed");
                }
			}

		}
        else if (request.aborted)
        {
            NotifyRequestComplete(request);
        }
		else
		{
            s_logger.LogError("request {0} error\n{1}", request.url, response.error);

            if (response.errorCode == HttpStatusCode.Unauthorized)
            {
                // do not count against the max try if unauthorized
                --request.attempCount;
                request.response = null;

                if (authErrorHandler != null)
                {
                    request.autoRetry = true;
                    m_authErrorTasks.Add(request);

                    if (!m_authError)
                    {
                        m_authError = true;

                        authErrorHandler.Handle(() =>
                        {
                            m_authError = false;
                            m_pendingRequests.AddRange(m_authErrorTasks);
                            m_authErrorTasks.Clear();
                        });
                    }
                }
                else
                {
                    NotifyRequestFail(request);
                }
            }
            else if (response.timeout || response.errorCode != HttpStatusCode.BadRequest)
            {
                if (request.attempCount < request.maxRetryCount)
                {
                    request.autoRetry = true;
                    m_pendingRequests.Add(request);
                }
                else if (request.useDefaultErrorHandling)
                {
                    if (taskErrorDefaultHandler != null)
                    {
                        NotifyRequestComplete(request);
                        taskErrorDefaultHandler.Handle(request);
                    }
                    else
                    {
                        NotifyRequestFail(request);
                    }
                }
                else
                {
                    NotifyRequestFail(request);
                }
            }
            else
            {
                NotifyRequestFail(request);
            }
		}
	}

    public void NotifyRequestFail(WebRequestData request)
    {
        NotifyRequestComplete(request);

        if (request.m_failCallBack != null)
        {
            if (request.m_failCallBack.IsSafeToInvoke())
            {
                request.m_failCallBack(request);
            }
            else
            {
                s_logger.Log("ignore fail callback, mono object destroyed");
            }
        }
    }

	public int AddTask(WebRequestData data)
	{
        if (data.m_ID != -1)
        {
            throw new InvalidOperationException("request already queued");
        }

        data.response = null;
        data.requestManager = this;
        data.aborted = false;
        data.autoRetry = false;

        if (data.maxRetryCount == -1)
        {
            data.maxRetryCount = maxRetryCount;
        }

		data.m_ID = m_NextTaskID++;
		if (m_NextTaskID < 0)
		{
			m_NextTaskID = 0;
		}
		m_pendingRequests.Add(data);
		return data.m_ID;
	}

    public void RemoveRequest(int taskId)
    {
        if (RemoveExecutingRequest(taskId))
        {
            return;
        }

        if (RemovePendingRequest(taskId))
        {
            return;
        }

        RemoveRequest(m_authErrorTasks, taskId);
    }

    public void RemoveRequest(WebRequestData task)
    {
        RemoveRequest(task.m_ID);
    }

    private bool RemoveRequest(List<WebRequestData> requests, int id)
    {
        for (int i = 0; i < requests.Count; ++i)
        {
            var request = requests[i];
            if (request.m_ID == id)
            {
                request.InternalAbort();
                requests.RemoveAt(i);
                NotifyRequestComplete(request);

                return true;
            }
        }
        return false;
    }

    private bool RemovePendingRequest(int taskId)
    {
        return RemoveRequest(m_pendingRequests, taskId);
    }

    private bool RemoveExecutingRequest(int id)
    {
        return RemoveRequest(m_executingRequests, id);
    }

	public void RemoveAllRequests()
	{
        RemoveAll(m_pendingRequests);
        RemoveAll(m_executingRequests);
        RemoveAll(m_authErrorTasks);
	}

    private void RemoveAll(List<WebRequestData> requests)
    {
        foreach (var request in requests)
        {
            request.InternalAbort();
            NotifyRequestComplete(request);
        }
		requests.Clear();
    }

    private void NotifyRequestComplete(WebRequestData request)
    {
        if (request.m_ID != -1)
        {
            request.m_ID = -1;

            // only when the request was really executed
            if (request.attempCount >= 0)
            {
                if (onRequestComplete != null)
                {
                    onRequestComplete(request);
                }
            }
        }
    }

    public void Reset()
    {
        RemoveAllRequests();
        m_authError = false;
    }

	void OnDestroy()
	{
        RemoveAllRequests();
	}
}
