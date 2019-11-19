using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Scheduling
{
    /// <summary>
    /// A concurrent enumerator that will start multiple sub-tasks simultaneously
    /// </summary>
    public class ConcurrentEnumerator : IEnumerator
    {
        private readonly IScheduler m_scheduler;
        private readonly IEnumerable<IEnumerator> m_tasks;
        private bool m_started;
        private int m_pendingTaskCount;
        private TaskCancellationToken m_token;

        public ConcurrentEnumerator(IScheduler scheduler, IEnumerable<IEnumerator> tasks, TaskCancellationToken token)
        {
            if (scheduler == null)
            {
                throw new ArgumentNullException("scheduler");
            }
            if (tasks == null)
            {
                throw new ArgumentNullException("tasks");
            }

            m_scheduler = scheduler;
            m_tasks = tasks;
            m_token = token;
        }

        public object Current
        {
            get { return null; }
        }

        public bool MoveNext()
        {
            if (!m_started)
            {
                m_started = true;
                foreach (var task in m_tasks)
                {
                    var request = m_scheduler.Schedule<Void>(task, m_token);
                    if (!request.isCompleted)
                    {
                        ++m_pendingTaskCount;
                        request.onCompleted += OnRequestCompleted;
                    }
                }
            }

            return m_pendingTaskCount > 0;
        }

        private void OnRequestCompleted(IAsyncRequest<Void> request)
        {
            request.onCompleted -= OnRequestCompleted;
            Debug.Assert(m_pendingTaskCount > 0);
            --m_pendingTaskCount;
        }

        public void Reset()
        {
            throw new NotSupportedException();
        }
    }
}
