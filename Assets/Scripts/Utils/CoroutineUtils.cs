using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoroutineUtils
{
    public static IEnumerator Async(Action<Action> callable)
    {
        bool done = false;
        callable(() => {
            done = true;
        });
        while (!done)
        {
            yield return null;
        }
    }

    public static IEnumerator WaitUntil(Func<bool> isDone)
    {
        while (!isDone())
        {
            yield return null;
        }
    }

    /// <summary>
    /// Run the coroutine with low latency.
    /// </summary>
    public static IEnumerator Run(IEnumerator coroutine)
    {
        if (coroutine == null)
        {
            throw new ArgumentNullException("coroutine");
        }

        var stack = new Stack<IEnumerator>();
        stack.Push(coroutine);
        try
        {
            while (stack.Count > 0)
            {
                var top = stack.Peek();
                bool hasNext;
                while (hasNext = top.MoveNext())
                {
                    var cur = top.Current;
                    if (cur is YieldInstruction)
                    {
                        yield return cur;
                    }
                    else if (cur is IEnumerator)
                    {
                        stack.Push((IEnumerator)cur);
                        break;
                    }
                    else
                    {
                        yield return cur;
                    }
                }
                if (!hasNext)
                {
                    if (top is IDisposable)
                    {
                        (top as IDisposable).Dispose();
                    }
                    stack.Pop();
                }
            }
        }
        finally
        {
            foreach (var co in stack)
            {
                if (co is IDisposable)
                {
                    (co as IDisposable).Dispose();
                }
            }
        }
    }
}
