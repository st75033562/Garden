using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Scheduling
{
    abstract class Task : IntrusiveListNode<Task>
    {
        public event Action<Task> onStateChanged;

        public enum State
        {
            Running,
            Waiting,
            Finished,
            Cancelled,
        }

        private State m_state = State.Running;

        public State state
        {
            get { return m_state; }
            protected set
            {
                if (m_state != value)
                {
                    m_state = value;
                    OnStateChanged();
                }
            }
        }

        protected virtual void OnStateChanged()
        {
            if (onStateChanged != null)
            {
                onStateChanged(this);
            }
        }

        public abstract object result { get; }

        public abstract void Update();

        public void Cancel()
        {
            if (state != State.Cancelled)
            {
                OnCancelled();
                state = State.Cancelled;
            }
        }

        protected abstract void OnCancelled();
    }

    class Task<T> : Task, ITask<T>
    {
        private readonly Stack<IEnumerator> m_coroutines = new Stack<IEnumerator>();
        private readonly MonoBehaviour m_coroutineService;
        private object m_lastYieldedValue;
        private Coroutine m_awaitedCoroutine;

        public Task(IEnumerator coroutine, TaskCancellationToken token, MonoBehaviour coroutineService)
        {
            if (coroutine == null)
            {
                throw new ArgumentNullException("coroutine");
            }
            if (coroutineService == null)
            {
                throw new ArgumentNullException("coroutineService");
            }

            m_coroutines.Push(coroutine);
            m_coroutineService = coroutineService;
            token.onCancelled += Cancel;
        }

        public override void Update()
        {
            if (state != State.Running)
            {
                return;
            }

            while (m_coroutines.Count > 0)
            {
                var curTask = m_coroutines.Peek();
                if (curTask.MoveNext())
                {
                    m_lastYieldedValue = curTask.Current;
                    if (m_lastYieldedValue is YieldInstruction)
                    {
                        state = State.Waiting;
                        m_awaitedCoroutine = m_coroutineService.StartCoroutine(Wait((YieldInstruction)m_lastYieldedValue));
                        break;
                    }
#if !(NET_4_6 || NET_STANDARD_2_0)
                    else if (m_lastYieldedValue is Task)
                    {
                        // since mono does not support contravarint delegates?, we can only unpack results from Tasks
                        state = State.Waiting;
                        (m_lastYieldedValue as Task).onStateChanged += OnAwaitedTaskStateChanged;
                        break;
                    }
#else
                    else if (m_lastYieldedValue is IAsyncRequest)
                    {
                        state = State.Waiting;
                        (m_lastYieldedValue as IAsyncRequest).onCompleted += OnAwaitedAsyncRequestCompleted;
                        break;
                    }
#endif
                    else if (m_lastYieldedValue is Return)
                    {
                        m_lastYieldedValue = (m_lastYieldedValue as Return).value;
                        FinishTask(curTask);
                    }
                    else if (m_lastYieldedValue is IEnumerator)
                    {
                        m_coroutines.Push((IEnumerator)m_lastYieldedValue);
                        continue;
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    FinishTask(curTask);
                }
            }
        }

        private void FinishTask(IEnumerator curTask)
        {

            if (curTask is IDisposable)
            {
                (curTask as IDisposable).Dispose();
            }

            m_coroutines.Pop();
            if (m_coroutines.Count == 0)
            {
                if (typeof(T) == typeof(Void))
                {
                    m_lastYieldedValue = Void.instance;
                }
                else if (m_lastYieldedValue != null && !(m_lastYieldedValue is T))
                {
                    Debug.LogErrorFormat("Invalid return type from {0}, expected {1}, got {2}",
                        Utils.GetEnumeratorMethodName(curTask),
                        typeof(T).FullName,
                        m_lastYieldedValue != null ? m_lastYieldedValue.GetType().FullName : null);
                    m_lastYieldedValue = default(T);
                }
                state = State.Finished;
            }
        }

        private void OnAwaitedTaskStateChanged(Task task)
        {
            if (task.state >= State.Finished)
            {
                m_lastYieldedValue = task.result;
                task.onStateChanged -= OnAwaitedTaskStateChanged;
                state = State.Running;
            }
        }

        private void OnAwaitedAsyncRequestCompleted(IAsyncRequest request)
        {
            m_lastYieldedValue = request.result;
            request.onCompleted -= OnAwaitedAsyncRequestCompleted;
            state = State.Running;
        }

        private IEnumerator Wait(YieldInstruction instruction)
        {
            yield return instruction;
            state = State.Running;
        }

        protected override void OnStateChanged()
        {
            base.OnStateChanged();

            if (isCompleted && m_onCompleted != null)
            {
                m_onCompleted(this);
            }
        }

        protected override void OnCancelled()
        {
            if (m_awaitedCoroutine != null)
            {
                m_coroutineService.StopCoroutine(m_awaitedCoroutine);
                m_awaitedCoroutine = null;
            }
            else if (m_lastYieldedValue != null)
            {
                (m_lastYieldedValue as IAsyncRequest).onCompleted -= OnAwaitedAsyncRequestCompleted;
            }

            foreach (var pending in m_coroutines)
            {
                if (pending is IDisposable)
                {
                    (pending as IDisposable).Dispose();
                }
            }

            m_coroutines.Clear();
            m_lastYieldedValue = default(T);
        }

        public override object result
        {
            get { return (this as IAsyncRequest<T>).result; }
        }

        #region ITask

        public bool isCancelled
        {
            get { return state == State.Cancelled; }
        }


        private AsyncRequestCompleted<T> m_onCompleted;
        public event AsyncRequestCompleted<T> onCompleted
        {
            add
            {
                if (value == null)
                {
                    return;
                }

                if (isCompleted)
                {
                    value(this);
                    return;
                }

                m_onCompleted += value;
            }
            remove
            {
                m_onCompleted -= value;
            }
        }

        T IAsyncRequest<T>.result
        {
            get
            {
                if (!isCompleted)
                {
                    throw new AsyncRequestException("task has not completed yet");
                }
                return (T)m_lastYieldedValue;
            }
        }

        object IAsyncRequest.result
        {
            get { return (this as IAsyncRequest<T>).result; }
        }

        event AsyncRequestCompleted IAsyncRequest.onCompleted
        {
            add { onCompleted += new AsyncRequestCompleted<T>(value); }
            remove { onCompleted -= new AsyncRequestCompleted<T>(value); }
        }

        public bool isCompleted
        {
            get { return state >= State.Finished; }
        }

        public object Current
        {
            get { return null; }
        }

        public bool MoveNext()
        {
            return !isCompleted;
        }

        public void Reset()
        {
            throw new NotSupportedException();
        }

        #endregion ITask
    }
}
