using System;
using System.Collections;
using System.Collections.Generic;

namespace Scheduling
{
    public static class SchedulerExtensions
    {
        public static ITask<T> Schedule<T>(this IScheduler scheduler, IEnumerator task)
        {
            if (scheduler == null)
            {
                throw new ArgumentNullException("scheduler");
            }

            return scheduler.Schedule<T>(task, TaskCancellationToken.none);
        }

        public static ITask<Void> Schedule(this IScheduler scheduler, IEnumerator task)
        {
            if (scheduler == null)
            {
                throw new ArgumentNullException("scheduler");
            }

            return scheduler.Schedule<Void>(task, TaskCancellationToken.none);
        }

        public static ITask<Void> Schedule(this IScheduler scheduler, IEnumerable<IEnumerator> tasks)
        {
            return Schedule(scheduler, tasks, TaskCancellationToken.none);
        }

        public static ITask<Void> Schedule(this IScheduler scheduler, IEnumerable<IEnumerator> tasks, TaskCancellationToken token)
        {
            if (scheduler == null)
            {
                throw new ArgumentNullException("scheduler");
            }

            return scheduler.Schedule<Void>(new ConcurrentEnumerator(scheduler, tasks, token), token);
        }

        public static ITask<Void> Schedule(this IScheduler scheduler, Action task)
        {
            return Schedule(scheduler, task, TaskCancellationToken.none);
        }

        public static ITask<Void> Schedule(this IScheduler scheduler, Action task, TaskCancellationToken token)
        {
            if (scheduler == null)
            {
                throw new ArgumentNullException("scheduler");
            }

            return scheduler.Schedule<Void>(Utils.AsEnumerator(task), token);
        }
    }
}
