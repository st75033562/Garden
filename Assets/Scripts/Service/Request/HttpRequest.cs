using Google.Protobuf;
using System;
using System.Collections.Generic;

public abstract class HttpRequest : IRequest
{
    private readonly Dictionary<string, string> m_headers = new Dictionary<string, string>();

    public HttpRequest()
        : this(WebRequestManager.Default)
    {
    }

    public HttpRequest(WebRequestManager requestManager)
    {
        if (requestManager == null)
        {
            throw new ArgumentNullException("requestManager");
        }
        this.requestManager = requestManager;
        defaultErrorHandling = true;
    }

    public WebRequestManager requestManager
    {
        get;
        private set;
    }

    protected WebRequestData webRequest { get; set; }

    public virtual void Abort()
    {
        if (webRequest != null)
        {
            webRequest.Abort();
            webRequest = null;
        }
    }

    public virtual void Execute()
    {
        if (webRequest != null)
        {
            throw new InvalidOperationException();
        }

        Validate();

        webRequest = new WebRequestData(requestManager);
        webRequest.blocking = blocking;
        webRequest.useDefaultErrorHandling = defaultErrorHandling;
        webRequest.defaultErrorPrompt = errorPrompt;
        webRequest.m_SuccessCallBack = (resp, userObj) => OnSuccess(resp);
        webRequest.m_failCallBack = OnError;
        webRequest.m_ProgressCallback = uploadProgressHandler;
        webRequest.SetPath(path);

        foreach (var header in m_headers)
        {
            webRequest.Header(header.Key, header.Value);
        }

        Init(webRequest);
        webRequest.Execute();
    }

    protected virtual void Validate() { }

    public bool blocking { get; set; }

    public HttpRequest Blocking(bool flag = true)
    {
        blocking = flag;
        return this;
    }

    public abstract string path { get; }

    public void SetHeader(string name, string value)
    {
        m_headers[name] = value;
    }

    public bool defaultErrorHandling { get; set; }

    public string errorPrompt { get; set; }

    public Action successHandler { get; set; }

    public HttpRequest Success(Action handler)
    {
        successHandler = handler;
        return this;
    }

    public Action errorHandler { get; set; }

    public HttpRequest Error(Action handler)
    {
        errorHandler = handler;
        return this;
    }

    public Action finalHandler { get; set; }

    public HttpRequest Finally(Action handler)
    {
        finalHandler = handler;
        return this;
    }

    public Action<float> uploadProgressHandler { get; set; }

    public HttpRequest UploadProgress(Action<float> handler)
    {
        uploadProgressHandler = handler;
        return this;
    }

    public object userData { get; set; }

    protected virtual void Init(WebRequestData request) { }

    protected virtual void OnSuccess(ResponseData response)
    {
        webRequest = null;

        if (successHandler != null)
        {
            successHandler();
        }

        if (finalHandler != null)
        {
            finalHandler();
        }
    }

    protected virtual void OnError(WebRequestData request)
    {
        webRequest = null;

        if (errorHandler != null)
        {
            errorHandler();
        }

        if (finalHandler != null)
        {
            finalHandler();
        }
    }
}

public abstract class HttpRequest<ResultT> : HttpRequest
{
    public Action<ResultT> successCallback { get; set; }

    public HttpRequest() { }

    public HttpRequest(WebRequestManager requestManager)
        : base(requestManager)
    { }

    public HttpRequest Success(Action<ResultT> callback)
    {
        successCallback = callback;
        return this;
    }

    public ResultT result { get; protected set; }

    public override void Execute()
    {
        base.Execute();

        result = default(ResultT);
    }

    protected void SetResult(ResultT result)
    {
        this.result = result;

        if (successCallback != null)
        {
            successCallback(result);
        }
    }
}

public class SimpleHttpRequest : HttpRequest
{
    private string m_path;

    public override string path { get { return m_path; } }

    public void SetPath(string path) { m_path = path; }

    public byte[] postData { get; set; }

    public byte[] responseData { get; set; }

    public SimpleHttpRequest Success(Action<byte[]> handler)
    {
        if (handler == null)
        {
            throw new ArgumentNullException("handler");
        }
        successHandler = () => handler(responseData);
        return this;
    }

    public override void Execute()
    {
        base.Execute();

        responseData = null;
    }

    protected override void Init(WebRequestData request)
    {
        base.Init(request);
        request.postData = postData;
    }

    protected override void OnSuccess(ResponseData response)
    {
        responseData = response.bytes;
        base.OnSuccess(response);
    }
}

public class SimpleHttpRequest<ResultT> : HttpRequest<ResultT>
    where ResultT : IMessage<ResultT>, new()
{
    private string m_path;

    public SimpleHttpRequest()
    {
    }

    public SimpleHttpRequest(WebRequestManager requestManager)
        : base(requestManager)
    {
    }

    public override string path { get { return m_path; } }

    public void SetPath(string path) { m_path = path; }

    public byte[] postData { get; set; }

    protected override void Init(WebRequestData request)
    {
        base.Init(request);
        request.postData = postData;
    }

    protected override void OnSuccess(ResponseData response)
    {
        SetResult(ProtobufUtils.Parse<ResultT>(response.bytes));
        base.OnSuccess(response);
    }
}