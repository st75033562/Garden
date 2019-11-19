using System;
using UnityEngine;

/// <summary>
/// A simple wrapper around WWW and UnityWebRequest with timeout support
/// </summary>
public class TimedWebRequest : CustomYieldInstruction, IDisposable
{
    protected interface IRequest : IDisposable
    {
        bool isDone { get; }
        float progress { get; }
        string error { get; }
    }

    protected IRequest m_request;
    private int m_lastCheckTime;
    private float m_lastProgress;
    private int m_timeoutTime;
    private bool m_timedOut;

    static TimedWebRequest()
    {
        defaultTimeout = 30.0f;
    }

    public static float defaultTimeout
    {
        get;
        set;
    }

    protected void Init(IRequest request, float? timeout)
    {
        m_request = request;

        if (timeout == null)
        {
            timeout = defaultTimeout;
        }
        if (timeout.Value > 0)
        {
            m_timeoutTime = (int)(timeout.Value * 1000);
            m_lastProgress = m_request.progress;
            m_lastCheckTime = Environment.TickCount;
        }
    }

    public bool isDone
    {
        get
        {
            if (m_timeoutTime > 0 && m_request != null)
            {
                if (m_request.progress != m_lastProgress)
                {
                    m_lastProgress = m_request.progress;
                    m_lastCheckTime = Environment.TickCount;
                }

                if (!m_request.isDone)
                {
                    CheckTimeout();
                    return false;
                }
            }
            else if (m_request != null)
            {
                return m_request.isDone;
            }

            return true;
        }
    }

    private void CheckTimeout()
    {
        if (Environment.TickCount >= m_lastCheckTime + m_timeoutTime)
        {
            m_timedOut = true;
            Dispose();
        }
    }

    public bool timedOut
    {
        get { return m_timedOut; }
    }

    public string error
    {
        get
        {
            if (m_request != null)
            {
                return m_request.error;
            }
            return timedOut ? "timed out" : null;
        }
    }

    public float progress
    {
        get
        {
            return m_request != null ? m_request.progress : 1.0f;
        }
    }

    public override bool keepWaiting
    {
        get
        {
            return !isDone;
        }
    }

    public void Dispose()
    {
        if (m_request != null)
        {
            m_request.Dispose();
            m_request = null;
        }
    }
}