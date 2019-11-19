using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;

namespace Robomation
{
    public class TaskQueue
    {
        private readonly Queue<Action> m_queue = new Queue<Action>();
        private int m_maxConcurrency;
        private int m_concurrency;
        private int m_waiters;

        public TaskQueue(int concurrency)
        {
            if (concurrency <= 0)
            {
                m_maxConcurrency = int.MaxValue;
            }
            else
            {
                m_maxConcurrency = concurrency;
            }
        }

        public void add(Action action)
        {
            if (action == null)
            {
                throw new ArgumentNullException();
            }

            lock (m_queue)
            {
                if (m_concurrency < m_maxConcurrency)
                {
                    ThreadPool.QueueUserWorkItem(execute, action);
                    ++m_concurrency;
                }
                else
                {
                    m_queue.Enqueue(action);
                }
            }
        }

        private void execute(object arg)
        {
            Action task = (Action)arg;
            if (task == null)
            {
                lock (m_queue)
                {
                    task = m_queue.Dequeue();
                }
            }

            try
            {
                task();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            lock (m_queue)
            {
                if (m_queue.Count > 0)
                {
                    ThreadPool.QueueUserWorkItem(execute, m_queue.Dequeue());
                }
                else
                {
                    if (--m_concurrency == 0 && m_waiters > 0)
                    {
                        Monitor.PulseAll(m_queue);
                    }
                }
            }
        }

        // wait until all tasks are finished
        public bool wait(int timeout = Timeout.Infinite)
        {
            lock (m_queue)
            {
                while (m_queue.Count != 0 || m_concurrency > 0)
                {
                    ++m_waiters;
                    try
                    {
                        if (!Monitor.Wait(m_queue, timeout))
                        {
                            return false;
                        }
                    }
                    finally
                    {
                        --m_waiters;
                    }
                }
            }

            return true;
        }
    }
}
