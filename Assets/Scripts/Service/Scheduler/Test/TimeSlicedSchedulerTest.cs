#if UNITY_EDITOR

using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using Scheduling;

public class TimeSlicedSchedulerTest
{
    private TimeSlicedScheduler m_scheduler;

    [SetUp]
    public void Setup()
    {
        var go = new GameObject();
        m_scheduler = go.AddComponent<TimeSlicedScheduler>();
    }

    [TearDown]
    public void TearDown()
    {
        if (m_scheduler)
        {
            Object.Destroy(m_scheduler.gameObject);
        }
    }

    [UnityTest]
    public IEnumerator CancelAllTasks_IndependentTask()
    {
        bool onCompleteCalled = false;
        var taskStopped = new ValueWrapper<bool>(true);
        var task = m_scheduler.Schedule(IndependentTask(taskStopped));
        task.onCompleted += delegate { onCompleteCalled = true; };

        yield return null;
        yield return null;

        m_scheduler.CancelAllTasks();

        yield return null;
        yield return null;

        Assert.IsTrue(task.isCancelled);
        Assert.IsTrue(task.isCompleted);
        Assert.IsTrue(onCompleteCalled);
        Assert.IsTrue(taskStopped.value);
    }

    private IEnumerator IndependentTask(ValueWrapper<bool> stopped)
    {
        yield return null;
        stopped.value = false;
    }

    [UnityTest]
    public IEnumerator CancelAllTasks_DependentTask()
    {
        var taskStopped = new ValueWrapper<bool>(true);
        m_scheduler.Schedule(DependentTask(taskStopped));

        yield return null;
        yield return null;

        m_scheduler.CancelAllTasks();

        yield return new WaitForSeconds(2);

        Assert.IsTrue(taskStopped.value);
    }

    private IEnumerator DependentTask(ValueWrapper<bool> stopped)
    {
        yield return m_scheduler.Schedule(Wait(stopped));
    }

    private IEnumerator Wait(ValueWrapper<bool> stopped)
    {
        yield return new WaitForSeconds(1);
        stopped.value = false;
    }
}

#endif
