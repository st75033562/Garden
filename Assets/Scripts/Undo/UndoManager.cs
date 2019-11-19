using System;
using System.Collections.Generic;
using UnityEngine;

public class UndoManager
{
    public event Action onStackSizeChanged;
    public event Action onUndoEnabledChanged;

    public event Action<IUndoCommand> onRunCommand;
    public event Action onRunningChanged;

    private readonly List<IUndoCommand> m_commands = new List<IUndoCommand>();

    private const int AlwaysDirty = -1;

    private int m_cleanUndoStackSize;

    // [0, m_curUndoStackSize) contains all undo commands
    // [m_curUndoStackSize, size) contains all redo commands
    private int m_curUndoStackSize;

    private readonly Stack<MacroCommand> m_macroStack = new Stack<MacroCommand>();
    private MacroCommand m_topMacroCommand;
    private bool m_stackEnabled = true;

    private bool m_undoEnabled = true;
    private bool m_isRunning;
    private int m_runningCommandCount;

    public void BeginMacro(object userData = null)
    {
        m_macroStack.Push(new MacroCommand { userData = userData });
        if (m_topMacroCommand == null)
        {
            m_topMacroCommand = m_macroStack.Peek();
        }
    }

    public void EndMacro()
    {
        if (m_macroStack.Count == 0)
        {
            throw new InvalidOperationException("empty macro stack");
        }

        var macroCommand = m_macroStack.Pop();
        if (macroCommand.commandCount > 0)
        {
            // add to the parent macro command if any
            if (m_macroStack.Count != 0)
            {
                m_macroStack.Peek().Add(macroCommand);
            }
            else
            {
                // top level macro command, push onto the stack
                InternalAddUndo(macroCommand);
                m_topMacroCommand = null;
                TryResetRunningState();
            }
        }
    }

    /// <summary>
    /// Push the undo command onto the undo stack
    /// </summary>
    /// <param name="cmd">undo command</param>
    /// <param name="redo">if true, Redo will be called</param>
    public void AddUndo(IUndoCommand cmd, bool redo = true)
    {
        if (cmd == null)
        {
            throw new ArgumentNullException("cmd");
        }

        if (!stackEnabled || m_cleanUndoStackSize > m_curUndoStackSize)
        {
            m_cleanUndoStackSize = AlwaysDirty;
        }

        InternalAddUndo(cmd);
        if (redo)
        {
            if (m_macroStack.Count > 0 && !isRunning)
            {
                isRunning = true;

                if (onRunCommand != null)
                {
                    onRunCommand(m_topMacroCommand);
                }
            }

            ++m_runningCommandCount;
            cmd.onCompleted += EndRunningCommand;
            cmd.Redo();
        }
    }

    private void InternalAddUndo(IUndoCommand cmd)
    {
        if (m_macroStack.Count > 0)
        {
            m_macroStack.Peek().Add(cmd);
        }
        else
        {
            if (!stackEnabled)
            {
                return;
            }

            m_commands.RemoveRange(m_curUndoStackSize, RedoStackSize);
            m_commands.Add(cmd);
            ++m_curUndoStackSize;

            if (onStackSizeChanged != null)
            {
                onStackSizeChanged();
            }
        }
    }

    public int UndoStackSize
    {
        get { return m_curUndoStackSize; }
    }

    public int RedoStackSize
    {
        get { return m_commands.Count - m_curUndoStackSize; }
    }

    public void Reset()
    {
        m_macroStack.Clear();
        m_commands.Clear();
        m_cleanUndoStackSize = 0;
        m_curUndoStackSize = 0;
        m_topMacroCommand = null;
        m_runningCommandCount = 0;

        if (onStackSizeChanged != null)
        {
            onStackSizeChanged();
        }
    }

    public bool isRunning
    {
        get { return m_isRunning; }
        private set
        {
            if (m_isRunning != value)
            {
                m_isRunning = value;

                if (onRunningChanged != null)
                {
                    onRunningChanged();
                }
            }
        }
    }

    public bool Undo()
    {
        if (!undoEnabled)
        {
            return false;
        }

        if (m_macroStack.Count > 0)
        {
            throw new InvalidOperationException("macro stack not empty");
        }

        if (m_isRunning)
        {
            throw new InvalidOperationException("already executing");
        }

        if (UndoStackSize > 0)
        {
            var cmd = m_commands[m_curUndoStackSize - 1];
            --m_curUndoStackSize;

            RunCommand(cmd, cmd.Undo);

            if (onStackSizeChanged != null)
            {
                onStackSizeChanged();
            }
            return true;
        }

        return false;
    }

    public bool Redo()
    {
        if (!undoEnabled)
        {
            return false;
        }

        if (m_macroStack.Count > 0)
        {
            throw new InvalidOperationException("macro stack not empty");
        }

        if (m_isRunning)
        {
            throw new InvalidOperationException("already executing");
        }

        if (RedoStackSize > 0)
        {
            var cmd = m_commands[m_curUndoStackSize];
            ++m_curUndoStackSize;

            RunCommand(cmd, cmd.Redo);

            if (onStackSizeChanged != null)
            {
                onStackSizeChanged();
            }
            return true;
        }

        return false;
    }

    // run the top level command
    private void RunCommand(IUndoCommand cmd, Action action)
    {
        Debug.Assert(m_macroStack.Count == 0);

        ++m_runningCommandCount;
        cmd.onCompleted += EndRunningCommand;
        isRunning = true;

        if (onRunCommand != null)
        {
            onRunCommand(cmd);
        }

        action();
    }

    private void EndRunningCommand(IUndoCommand cmd)
    {
        cmd.onCompleted -= EndRunningCommand;

        Debug.Assert(m_runningCommandCount > 0);
        --m_runningCommandCount;
        TryResetRunningState();
    }

    private void TryResetRunningState()
    {
        if (m_macroStack.Count == 0 && m_runningCommandCount == 0)
        {
            isRunning = false;
        }
    }

    public bool undoEnabled
    {
        get { return m_undoEnabled; }
        set
        {
            if (m_undoEnabled != value)
            {
                m_undoEnabled = value;

                if (onUndoEnabledChanged != null)
                {
                    onUndoEnabledChanged();
                }
            }
        }
    }

    public bool stackEnabled
    {
        get { return m_stackEnabled; }
        set
        {
            m_stackEnabled = value;
            if (!value)
            {
                m_cleanUndoStackSize = !IsClean() ? AlwaysDirty : 0;
                m_curUndoStackSize = 0;
                m_commands.Clear();

                if (onStackSizeChanged != null)
                {
                    onStackSizeChanged();
                }
            }
        }
    }

    /// <summary>
    /// The clean state of the undo stack, used to implement dirty flag
    /// </summary>
    /// <remarks>
    /// If you set the state to false, then the state will remain false until it is set to true.
    /// This is useful if you want to mark the state to false when the stack is empty, e.g.
    /// after restoring an unsaved document.
    /// </remarks>
    public bool IsClean()
    {
        if (m_cleanUndoStackSize == AlwaysDirty)
        {
            return false;
        }

        int start;
        int end;
        if (m_curUndoStackSize <= m_cleanUndoStackSize)
        {
            start = m_curUndoStackSize;
            end = m_cleanUndoStackSize;
        }
        else
        {
            start = m_cleanUndoStackSize;
            end = m_curUndoStackSize;
        }

        return m_commands.FindIndex(start, end - start, x => x.hasSideEffect) == -1;
    }

    public void SetClean(bool clean)
    {
        if (m_macroStack.Count > 0)
        {
            throw new InvalidOperationException("macro stack not empty");
        }

        if (clean)
        {
            m_cleanUndoStackSize = m_curUndoStackSize;
        }
        else
        {
            m_cleanUndoStackSize = AlwaysDirty;
        }
    }
}
