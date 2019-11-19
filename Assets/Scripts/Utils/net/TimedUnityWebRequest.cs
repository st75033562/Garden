using System;
using System.Net;
using UnityEngine.Networking;

public class TimedUnityWebRequest : TimedWebRequest
{
    private class WebRequest : IRequest
    {
        private UnityWebRequest m_request;

        public WebRequest(UnityWebRequest request)
        {
            m_request = request;
        }

        public bool isDone
        {
            get { return m_request.isDone; }
        }

        public float progress
        {
            get { return m_request.downloadProgress; }
        }

        public string error
        {
            get
            {
                if (m_request.error != null)
                {
                    return m_request.error;
                }

                if (m_request.responseCode >= 400)
                {
                    return ((HttpStatusCode)m_request.responseCode).ToString();
                }

                return null;
            }
        }

        public UnityWebRequest request
        {
            get { return m_request; }
        }

        public void Dispose()
        {
            m_request.Dispose();
            m_request = null;
        }
    }

    public TimedUnityWebRequest(UnityWebRequest webRequest, float? timeout = null)
    {
        if (webRequest == null)
        {
            throw new ArgumentNullException("webRequest");
        }

        webRequest.Send();
        Init(new WebRequest(webRequest), timeout);
    }

    /// <summary>
    /// return the underlying request, if times out or disposed, return null
    /// </summary>
    public UnityWebRequest rawRequest
    {
        get { return m_request != null ? (m_request as WebRequest).request : null; }
    }

    public static TimedUnityWebRequest Get(string url)
    {
        return new TimedUnityWebRequest(UnityWebRequest.Get(url));
    }
}
