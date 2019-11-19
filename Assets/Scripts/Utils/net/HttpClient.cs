//每次new一个对象建议执行一个get或者post方法，如果一个对象中多次get、post可能会造成调用执行Abort方法错误
//Editor中没做超时检测，主要是做了超时会导致unity只能运行一次
//调用Abort 可能会返回在error中 返回Abort
using System.Collections;
using System;
using System.Text;
using System.Net;
using System.Threading;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

public class ResponseData
{
    public byte[] bytes = new byte[0];
    public string error;
    public HttpStatusCode errorCode;
    public object parameter;
    public bool abort;
    public bool timeout;
    public Dictionary<string, string> headers = new Dictionary<string, string>();
}

public interface IHttpClient
{
    void Get(string url, object parameter, Action<ResponseData> action, Dictionary<string, string> dic);

    void Post(string url, byte[] bytes, object parameter, Action<ResponseData> action, Dictionary<string, string> dic);

    void Abort();

    // set is for internal use
    bool ProgressChanged { get; set; }

    float Progress { get; }
}

public class HttpClient : IHttpClient
{
    const int BUFFER_SIZE = 20480;
    const int DefaultTimeout = 100000; // 毫秒
    const bool keepAlive = false;
    const int WriteBlockSize = 1024 * 10;

    class RequestState
    {
        public byte[] BufferRead;
        public HttpWebRequest request;
        public HttpWebResponse response;
        public Stream streamResponse;
        public ResponseData responseData;
        public byte[] postData;
        public Action<ResponseData> action;
        public int alreadyReadLength;
    }

    private RequestState m_RequestState;
    private float m_Progress;
    private readonly object m_gate = new object();

    public void Get(string url, object parameter, Action<ResponseData> action, Dictionary<string, string> dic = null)
    {
        url = transcodingUrl(url);
        if (m_RequestState != null)
        {
            throw new InvalidOperationException();
        }

        HttpWebRequest myHttpWebRequest = (HttpWebRequest)WebRequest.Create(url);
        m_RequestState = InitRequestState(myHttpWebRequest, "GET", action , parameter, headers: dic);
        try
        {
            var result = (IAsyncResult)myHttpWebRequest.BeginGetResponse(new AsyncCallback(RespCallback), m_RequestState);
            if (!Application.isEditor)
            {
                ThreadPool.RegisterWaitForSingleObject(result.AsyncWaitHandle, new WaitOrTimerCallback(TimeoutCallback), m_RequestState, DefaultTimeout, true);
            }
        }
        catch (WebException e)
        {
            CallBackResult(m_RequestState, e);
        }
    }


    public void Post(string url, byte[] bytes, object parameter, Action<ResponseData> action, Dictionary<string, string> dic = null)
    {
        url = transcodingUrl(url);
        if (m_RequestState != null)
        {
            throw new InvalidOperationException();
        }

        HttpWebRequest myHttpWebRequest = (HttpWebRequest)WebRequest.Create(url);
        m_RequestState = InitRequestState(myHttpWebRequest, "POST", action, parameter, bytes, dic);
        try
        {
            var result = myHttpWebRequest.BeginGetRequestStream(new AsyncCallback(GetRequestStreamCallback), m_RequestState);
            if (!Application.isEditor)
            {
                ThreadPool.RegisterWaitForSingleObject(result.AsyncWaitHandle, new WaitOrTimerCallback(TimeoutCallback), m_RequestState, DefaultTimeout, true);
            }
        }
        catch (WebException e)
        {
            CallBackResult(m_RequestState , e);
        }
    }

    RequestState InitRequestState(HttpWebRequest webRequest, string method , Action<ResponseData> action, object parameter, byte[] bytes = null , Dictionary<string, string> headers = null)
    {
        webRequest.Method = method;
        webRequest.KeepAlive = keepAlive;

        RequestState myRequestState = new RequestState();
        myRequestState.request = webRequest;
        myRequestState.responseData = new ResponseData();
        myRequestState.responseData.parameter = parameter;
        myRequestState.postData = bytes;
        myRequestState.action = action;

        if (headers != null)
        {
            foreach (string key in headers.Keys)
            {
                webRequest.Headers.Add(key, headers[key]);
            }
        }
        return myRequestState;
    }

    private void GetRequestStreamCallback(IAsyncResult asynchronousResult)
    {
        RequestState myRequestState = (RequestState)asynchronousResult.AsyncState;
        try
        {
            Stream postStream = myRequestState.request.EndGetRequestStream(asynchronousResult);
            WriteWithProgress(postStream, myRequestState.postData);
            myRequestState.request.BeginGetResponse(new AsyncCallback(RespCallback), myRequestState);
        }
        catch (WebException e)
        {
            CallBackResult(myRequestState, e);
        }
        catch (Exception e)
        {
            myRequestState.responseData.error = e.ToString();
            CallBackResult(myRequestState);
        }
    }

    private void WriteWithProgress(Stream stream, byte[] data)
    {
        // at most 100 progress reports
        int blockSize = Mathf.Max(WriteBlockSize, data.Length / 100);
        int offset = 0;
        SetProgress(0);
        while (offset < data.Length)
        {
            int writeSize = Mathf.Min(data.Length - offset, blockSize);
            stream.Write(data, offset, writeSize);

            offset += writeSize;
            SetProgress((float)offset / data.Length);
        }
        stream.Close();
    }

    private void TimeoutCallback(object state, bool timedOut)
    {
        RequestState request = state as RequestState;
        if (timedOut)
        {
            request.responseData.error = "time out";
            request.responseData.timeout = true;
            request.request.Abort();
        }
    }

    private void RespCallback(IAsyncResult asynchronousResult)
    {
        RequestState myRequestState = (RequestState)asynchronousResult.AsyncState;
        try
        {
            HttpWebRequest myHttpWebRequest = myRequestState.request;
            myRequestState.response = (HttpWebResponse)myHttpWebRequest.EndGetResponse(asynchronousResult);

            if (myRequestState.response.ContentLength == -1)
            {
                myRequestState.BufferRead = new byte[BUFFER_SIZE];
                m_Progress = -1;
            }
            else
            {
                myRequestState.BufferRead = new byte[myRequestState.response.ContentLength];
            }
            
            foreach (string key in myRequestState.response.Headers.Keys)
            {
                myRequestState.responseData.headers.Add(key , myRequestState.response.Headers[key]);
            }

            Stream responseStream = myRequestState.response.GetResponseStream();
            myRequestState.streamResponse = responseStream;
            
            responseStream.BeginRead(myRequestState.BufferRead, 0, myRequestState.BufferRead.Length, new AsyncCallback(ReadCallBack), myRequestState);
        }
        catch (WebException e)
        {
            CallBackResult(myRequestState , e);
        }
        catch (Exception e)
        {
            myRequestState.responseData.error = e.ToString();
            CallBackResult(myRequestState);
        }
    }

    private void ReadCallBack(IAsyncResult asyncResult)
    {
        RequestState myRequestState = (RequestState)asyncResult.AsyncState;
        try
        {
            Stream responseStream = myRequestState.streamResponse;
            int read = responseStream.EndRead(asyncResult);
            if (read > 0)
            {
                if (myRequestState.response.ContentLength == -1)
                {
                    int originalLength = myRequestState.responseData.bytes.Length;
                    byte[] buf = new byte[read + originalLength];
                    Array.Copy(myRequestState.responseData.bytes, 0, buf, 0, originalLength);
                    Array.Copy(myRequestState.BufferRead, 0, buf, originalLength, read);

                    myRequestState.responseData.bytes = buf;
                    responseStream.BeginRead(myRequestState.BufferRead, 0, BUFFER_SIZE, new AsyncCallback(ReadCallBack), myRequestState);
                }
                else
                {
                    myRequestState.alreadyReadLength += read;

                    m_Progress = (float)myRequestState.alreadyReadLength / myRequestState.BufferRead.Length;
                    ProgressChanged = true;

                    myRequestState.responseData.bytes = myRequestState.BufferRead;
                    responseStream.BeginRead(myRequestState.BufferRead, myRequestState.alreadyReadLength,
                        (myRequestState.BufferRead.Length - myRequestState.alreadyReadLength), new AsyncCallback(ReadCallBack), myRequestState);
                }                
                return;
            }
            else
            {
                responseStream.Close();
                CallBackResult(myRequestState);
            }
        }
        catch (Exception e)
        {
            myRequestState.responseData.error = e.ToString();
            CallBackResult(myRequestState);
        }
    }

    private void CallBackResult(RequestState requestState, WebException e = null)
    {
        lock (m_gate)
        {
            if (requestState.responseData.abort)
            {
                return;
            }

            if (e != null && e.Status != WebExceptionStatus.RequestCanceled) //not Abort
            {
                if (e.Response != null)
                {
                    requestState.responseData.errorCode = ((HttpWebResponse)e.Response).StatusCode;
                }
                requestState.responseData.error = e.ToString();
            }
            if (requestState.response != null)
            {
                requestState.response.Close();
            }
        }

        if (requestState.action != null)
        {
            requestState.action(requestState.responseData);
        }
    }
    
    public void Abort()
    {
        if (m_RequestState != null)
        {
            lock (m_gate)
            {
                m_RequestState.responseData.error = "Abort";
                m_RequestState.responseData.abort = true;
                m_RequestState.request.Abort();
                if (m_RequestState.response != null)
                {
                    m_RequestState.response.Close();
                }
            }
        }
    }

    public bool ProgressChanged
    {
        get;
        set;
    }

    public float Progress
    {
        get { return m_Progress; }
    }

    private void SetProgress(float progress)
    {
        m_Progress = progress;
        ProgressChanged = true;
    }

    string transcodingUrl(string url)
    {
        return url.Replace("+", "%2B");
    }
}
