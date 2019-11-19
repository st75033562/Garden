using System;
using System.Collections;

namespace Scheduling
{
    /// <summary>
    /// The type represents void
    /// </summary>
    public sealed class Void
    {
        private Void() { }

        public static readonly Void instance = new Void();
    }

    sealed class Suspended
    {
        private Suspended() { }

        public static readonly Suspended instance = new Suspended();
    }

    /// <summary>
    /// return a result from a task, mainly for multiple returns in a task
    /// </summary>
    public sealed class Return
    {
        public Return(object value)
        {
            this.value = value;
        }

        public object value { get; private set; }

        public static readonly Return Null = new Return(null);
    }

    public struct TaskCancellationToken
    {
        private readonly TaskCancellationSource m_source;

        public event Action onCancelled
        {
            add 
            { 
                if (m_source != null)
                {
                    m_source.onCanceled += value;
                }
            }
            remove
            {
                if (m_source != null)
                {
                    m_source.onCanceled -= value;
                }
            }
        }

        public TaskCancellationToken(TaskCancellationSource source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            m_source = source;
        }

        public bool isCancelled { get { return m_source != null && m_source.isCancelled; } }

        /// <summary>
        /// the token which will never be canceled
        /// </summary>
        public static readonly TaskCancellationToken none = new TaskCancellationToken();
    }

    public class TaskCancellationSource
    {
        private Action m_onCanceled;

        public event Action onCanceled
        {
            add
            {
                if (value == null) { return; }
                if (isCancelled)
                {
                    value();
                    return;
                }
                m_onCanceled += value;
            }
            remove
            {
                m_onCanceled -= value;
            }
        }

        public TaskCancellationToken token
        {
            get { return new TaskCancellationToken(this); }
        }

        public void Cancel()
        {
            if (!isCancelled)
            {
                isCancelled = true;
                if (m_onCanceled != null)
                {
                    m_onCanceled();
                }
            }
        }

        public bool isCancelled { get; private set; }
    }

    public interface ITask : IAsyncRequest
    {
        bool isCancelled { get; }
    }

    // NOTE: .NET 2 does not support generic contra-variant delegates, do not subscribe to IAsyncRequest.onCompleted
    public interface ITask<T> : ITask, IAsyncRequest<T> { }
        
    public interface IScheduler
    {
        /// <summary>
        /// schedule a task whose return value type is T
        /// </summary>
        /// <remarks>
        /// The last yielded value must be of type T, otherwise default(T) is returned
        /// To yield the control to other tasks, simply yield return from the task
        /// </remarks>
        /// <typeparam name="T">return value type of the task</typeparam>
        /// <param name="task">task to schedulerun</param>
        /// <param name="token">cancellation token</param>
        /// <returns>the task handle</returns>
        ITask<T> Schedule<T>(IEnumerator task, TaskCancellationToken token);

#if NET_4_6 || NET_STANDARD_2_0
        /// <summary>
        /// schedule a task which has no return value
        /// </summary>
        /// <param name="task">task to schedule</param>
        /// <param name="token">cancellation token</param>
        /// <returns>the task handle</returns>
        ITask Schedule(IEnumerator task, TaskCancellationToken token);
#endif

        /// <summary>
        /// cancel all the tasks, ITask.isCancelled will return true
        /// </summary>
        void CancelAllTasks();
    }
}