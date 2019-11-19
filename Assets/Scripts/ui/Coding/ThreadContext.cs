using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveRecord
{
    private readonly IDictionary<string, string> m_arguments;
    private string m_returnValue = ""; // return value of the current function call
    private bool m_returnValueSet;

    public ActiveRecord(Guid functionId, IDictionary<string, string> arguments)
    {
        if (arguments == null)
        {
            throw new ArgumentNullException("arguments");
        }

        this.functionId = functionId;
        m_arguments = arguments;
    }

    public Guid functionId { get; private set; }

    public string GetArgument(string name)
    {
        if (name == null)
        {
            throw new ArgumentNullException("name");
        }

        string value;
        m_arguments.TryGetValue(name, out value);
        return value;
    }

    public string returnValue
    {
        get { return m_returnValue; }
        set
        {
            if (m_returnValueSet)
            {
                throw new InvalidOperationException("return value already set");
            }
            m_returnValue = value ?? "";
            m_returnValueSet = true;
        }
    }

    public bool isReturned
    {
        get { return m_returnValueSet; }
    }
}

public delegate void CallStackOverflowHandler(ThreadContext context);

/// <summary>
/// A context of thread is used to save thread specific states, such as function arguments.
/// Normally, blocks should not save any states in their instances, because those in a function definition
/// can be run from many function calls simultaneously, thus any instance states can be overwritten 
/// by a later function call.
/// The only exceptions are thread entry blocks, e.g. WhenXXXBlocks, since they won't be called
/// by any other blocks, thus there's no reentrant problems with them.
/// </summary>
public class ThreadContext
{
    private readonly MonoBehaviour m_coroutineManager;
    private int m_callStackLimit;
    private readonly Stack<FunctionNode> m_runningNodes = new Stack<FunctionNode>();
    private readonly Stack<ActiveRecord> m_activeRecords = new Stack<ActiveRecord>();
    private CallStackOverflowHandler m_onOverflow;
    private bool m_isAborted;
    private int m_loopDepth;
    private readonly LoopScope m_loopScope;

    private class LoopScope : IDisposable
    {
        private readonly ThreadContext m_context;

        public LoopScope(ThreadContext context)
        {
            m_context = context;
        }

        public void Dispose()
        {
            m_context.ExitLoop();
        }
    }

    /// <summary>
    /// construct a thread context
    /// </summary>
    /// <param name="onAborted">callback triggerd when Abort is called</param>
    /// <param name="callStackLimit">max number of nested function calls. If &le; 0, the call stack is unlimited.</param>
    public ThreadContext(MonoBehaviour coroutineManager, CallStackOverflowHandler onOverflow, int callStackLimit = 0)
    {
        if (coroutineManager == null)
        {
            throw new ArgumentNullException("coroutineManager");
        }

        if (onOverflow == null)
        {
            throw new ArgumentNullException("onOverflow");
        }

        m_coroutineManager = coroutineManager;
        m_activeRecords.Push(new ActiveRecord(Guid.Empty, new Dictionary<string, string>()));
        m_onOverflow = onOverflow;
        m_callStackLimit = callStackLimit;
        m_loopScope = new LoopScope(this);
    }

    public int callStackLimit
    {
        get { return m_callStackLimit; }
        set
        {
            if (m_callStackLimit > 0 && m_callStackLimit < callStackDepth)
            {
                throw new ArgumentException("value");
            }
            m_callStackLimit = value;
        }
    }

    /// <summary>
    /// return the number of active function calls
    /// </summary>
    public int callStackDepth
    {
        get { return m_activeRecords.Count - 1; }
    }

    /// <summary>
    /// push the active record onto the call stack
    /// </summary>
    /// <returns>true if the record is pushed successfully, false if stack overflows</returns>
    public bool Push(ActiveRecord record)
    {
        if (record == null)
        {
            throw new ArgumentNullException("record");
        }

        if (m_callStackLimit > 0 && callStackDepth == m_callStackLimit)
        {
            if (!m_isAborted)
            {
                m_isAborted = true;
                m_onOverflow(this);
            }
            return false;
        }

        m_activeRecords.Push(record);
        return true;
    }

    public void Pop()
    {
        m_activeRecords.Pop();
    }

    public ActiveRecord currentActiveRecord
    {
        get { return m_activeRecords.Peek(); }
    }

    public void PushNode(FunctionNode node)
    {
        if (node == null)
        {
            throw new ArgumentNullException("node");
        }

        m_runningNodes.Push(node);
    }

    public void PopNode()
    {
        m_runningNodes.Pop();
    }

    public string GetArgument(string argName)
    {
        return currentActiveRecord.GetArgument(argName);
    }

    public bool isReturned
    {
        get { return currentActiveRecord.isReturned; }
    }

    public IDisposable EnterLoop()
    {
        ++m_loopDepth;
        return m_loopScope;
    }

    private void ExitLoop()
    {
        if (m_loopDepth == 0)
        {
            throw new InvalidOperationException();
        }
        --m_loopDepth;
        shouldBreakFromLoop = false;
    }

    public void BreakLoop()
    {
        if (m_loopDepth == 0)
        {
            return;
        }
        shouldBreakFromLoop = true;
    }

    /// <summary>
    /// true if the inner-most loop control block should return stop execution
    /// </summary>
    public bool shouldBreakFromLoop
    {
        get;
        private set;
    }

    /// <summary>
    /// return from the current function call
    /// </summary>
    public void Return(string returnValue)
    {
        currentActiveRecord.returnValue = returnValue;
    }

    public IEnumerable<FunctionNode> runningNodes
    {
        get { return m_runningNodes; }
    }

    /// <summary>
    /// true if the execution of the thread is aborted
    /// </summary>
    public bool isAborted
    {
        get { return m_isAborted; }
    }

    public MonoBehaviour coroutineManager
    {
        get { return m_coroutineManager; }
    }

    public void Reset()
    {
        m_activeRecords.Clear();
        m_runningNodes.Clear();
        m_isAborted = false;
        shouldBreakFromLoop = false;
        m_coroutineManager.StopAllCoroutines();
    }
}
