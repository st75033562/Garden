using System;
using UnityEngine;

public class TimedWWWRequest : TimedWebRequest
{
    private class WWWRequest : IRequest
    {
        private WWW m_www;

        public WWWRequest(WWW www)
        {
            m_www = www;
        }

        public bool isDone
        {
            get { return m_www.isDone; }
        }

        public float progress
        {
            get { return m_www.progress; }
        }

        public string error
        {
            get { return m_www.error; }
        }

        public WWW request
        {
            get { return m_www; }
        }

        public void Dispose()
        {
            m_www.Dispose();
            m_www = null;
        }
    }

    /// <summary>
    /// create a WWW which will be Disposed after timeout seconds elapsed if no progress is made
    /// </summary>
    /// <param name="timeout">if &le; 0, no timeout</param>
    /// <returns></returns>
    public TimedWWWRequest(string url, float? timeout = null)
        : this(new WWW(url), timeout)
    {
    }

    public TimedWWWRequest(WWW www, float? timeout = null)
    {
        if (www == null)
        {
            throw new ArgumentNullException("www");
        }

        Init(new WWWRequest(www), timeout);
    }

    /// <summary>
    /// return the underlying www request, if times out or disposed, return null
    /// </summary>
    public WWW rawRequest
    {
        get { return m_request != null ? (m_request as WWWRequest).request : null; }
    }

}
