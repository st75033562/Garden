using System;
using System.Collections;
using UnityEngine;

namespace Scheduling
{
    public class TimeSlicedScheduler : MonoBehaviour, IScheduler
    {
        private float m_maxRuntime;
        private readonly IntrusiveList<Task> m_runningTasks = new IntrusiveList<Task>();
        private readonly IntrusiveList<Task> m_waitingTasks = new IntrusiveList<Task>();

        public float maxMillisecondsPerFrame
        {
            get { return m_maxRuntime; }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("value", "must not be negative");
                }
                m_maxRuntime = value;
            }
        }

        public ITask<T> Schedule<T>(IEnumerator task, TaskCancellationToken token)
        {
            if (task == null)
            {
                throw new ArgumentNullException("task");
            }

            var newTask = new Task<T>(task, token, this);
            newTask.onStateChanged += OnTaskStateChanged;
            m_runningTasks.AddLast(newTask);
            return newTask;
        }

#if NET_4_6 || NET_STANDARD_2_0
        public ITask Schedule(IEnumerator task, TaskCancellationToken token)
        {
            return Schedule<Void>(task, token);
        }
#endif

        public void CancelAllTasks()
        {
            foreach (var task in m_waitingTasks)
            {
                task.Cancel();
            }
            m_waitingTasks.Clear();

            foreach (var task in m_runningTasks)
            {
                task.Cancel();
            }
            m_runningTasks.Clear();
        }

        private void OnTaskStateChanged(Task task)
        {
            if (task.state == Task.State.Running)
            {
                m_waitingTasks.Remove(task);
                m_runningTasks.AddLast(task);
            }
            else if (task.state == Task.State.Cancelled)
            {
                if (task.list == m_runningTasks)
                {
                    m_runningTasks.Remove(task);
                }
                if (task.list == m_waitingTasks)
                {
                    m_waitingTasks.Remove(task);
                }
            }
        }

        void LateUpdate()
        {
            if (m_maxRuntime == 0)
            {
                return;
            }

            var totalTime = m_maxRuntime;
            var lastTime = Time.realtimeSinceStartup * 1000.0f;

            var curTask = m_runningTasks.first;
            while (totalTime > 0 && curTask != null)
            {
                var curTime = Time.realtimeSinceStartup * 1000.0f;
                totalTime -= curTime - lastTime;
                curTask.Update();
                if (totalTime <= 0)
                {
                    break;
                }

                if (curTask.state == Task.State.Waiting)
                {
                    var nextTask = m_runningTasks.Remove(curTask);
                    m_waitingTasks.AddLast(curTask);
                    curTask = nextTask;
                }
                else if (curTask.state == Task.State.Finished)
                {
                    curTask = m_runningTasks.Remove(curTask);
                }
                else
                {
                    Debug.Assert(curTask.state == Task.State.Running);
                    curTask = curTask.next;
                }

                if (curTask == null)
                {
                    curTask = m_runningTasks.first;
                }

                lastTime = curTime;
            }
        }
    }
}
