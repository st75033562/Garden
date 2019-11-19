using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;

// can be used to abort the callback
public class CallbackHandle
{
    internal CallbackHandle(Action action)
    {
        this.action = action;
    }

    internal Action action { get; private set; }

    public bool aborted { get; set; }
}

// used by multithreading code to queue callback
public class CallbackQueue : Singleton<CallbackQueue>
{
    private object m_lock = new object();
    private Queue<CallbackHandle> m_queuedHandles = new Queue<CallbackHandle>();
    private Queue<CallbackHandle> m_executingHandles = new Queue<CallbackHandle>();

    private int m_threadId;

    void Awake()
    {
        m_threadId = Thread.CurrentThread.ManagedThreadId;
    }

    public CallbackHandle Enqueue(Action action)
    {
        lock (m_lock)
        {
            var handle = new CallbackHandle(action);
            m_queuedHandles.Enqueue(handle);
            return handle;
        }
    }

    /// <summary>
    /// <para>run the action on the main thread</para>
    /// <para>if the calling thread is the main then, the action will be run immediately</para>
    /// </summary>
    public void RunOnMainThread(Action action)
    {
        if (Thread.CurrentThread.ManagedThreadId == m_threadId)
        {
            action();
        }
        else
        {
            Enqueue(action);
        }
    }

    void Update()
    {
        lock (m_lock)
        {
            Queue<CallbackHandle> tmp = m_queuedHandles;
            m_queuedHandles = m_executingHandles;
            m_executingHandles = tmp;
        }

        while (m_executingHandles.Count > 0)
        {
            try
            {
                var handle = m_executingHandles.Dequeue();
                if (!handle.aborted)
                {
                    handle.action();
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }

    void OnApplicationQuit()
    {
#if UNITY_EDITOR
        // last chance execution
        Update();
#endif
    }
}
