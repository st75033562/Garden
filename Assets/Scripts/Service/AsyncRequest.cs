using System;
using System.Collections;
using UnityEngine;

public delegate void AsyncRequestCompleted(IAsyncRequest request);
public delegate void AsyncRequestCompleted<in T>(IAsyncRequest<T> request);

public class AsyncRequestException : Exception
{
    public AsyncRequestException() { }

    public AsyncRequestException(string message)
        : base(message)
    {
    }
}

// TODO: add support for cancellation
public interface IAsyncRequest : IEnumerator
{
    event AsyncRequestCompleted onCompleted;

    /// <summary>
    /// result of the request, null if the request has no return value or was cancelled
    /// </summary>
    /// <exception cref="AsyncRequestException">if isCompleted is false</exception>
    object result { get; }

    /// <summary>
    /// return true if the request either has completed successfully or was cancelled
    /// </summary>
    bool isCompleted { get; }
}

public interface IAsyncRequest<out T> : IAsyncRequest
{
    new event AsyncRequestCompleted<T> onCompleted;

    /// <summary>
    /// default(T) if the request was cancelled
    /// </summary>
    new T result { get; }
}

public abstract class AsyncRequest<T> : IAsyncRequest<T>
{
    private AsyncRequestCompleted<T> m_onCompleted;
    private bool m_isCompleted;

    public event AsyncRequestCompleted<T> onCompleted
    {
        add
        {
            if (value == null) { return; }

            if (m_isCompleted)
            {
                value(this);
            }
            else
            {
                m_onCompleted += value;
            }
        }

        remove
        {
            m_onCompleted -= value;
        }
    }

    event AsyncRequestCompleted IAsyncRequest.onCompleted
    {
        add { m_onCompleted += new AsyncRequestCompleted<T>(value); }
        remove { m_onCompleted -= new AsyncRequestCompleted<T>(value); }
    }

    public T result
    {
        get
        {
            if (!isCompleted)
            {
                throw new AsyncRequestException("the request has not completed yet");
            }
            return GetResult();
        }
    }

    protected abstract T GetResult();

    object IAsyncRequest.result { get { return result; } }

    public virtual void Dispose()
    {
        SetCompleted();
    }

    public bool isCompleted
    {
        get { return m_isCompleted; }
    }

    protected void SetCompleted()
    {
        if (!m_isCompleted)
        {
            m_isCompleted = true;

            if (m_onCompleted != null)
            {
                m_onCompleted(this);
            }
        }
    }

    public object Current
    {
        get { return null; }
    }

    public bool MoveNext()
    {
        return !m_isCompleted;
    }

    public void Reset()
    {
        throw new NotSupportedException();
    }
}

public static class AsyncRequestExtensions
{
    public static IAsyncRequest<T> OnCompleted<T>(this IAsyncRequest<T> request, AsyncRequestCompleted<T> handler)
    {
        if (request == null)
        {
            throw new ArgumentNullException("request");
        }
        if (handler == null)
        {
            throw new ArgumentNullException("handler");
        }

        request.onCompleted += handler;
        return request;
    }
}
