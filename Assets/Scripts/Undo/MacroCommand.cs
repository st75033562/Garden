using System;
using System.Collections.Generic;
using System.Linq;

public class MacroCommand : IUndoCommand
{
    public event Action<IUndoCommand> onCompleted;

    private readonly List<IUndoCommand> m_commands = new List<IUndoCommand>();

    private int m_completionNum;

    public void Add(IUndoCommand command)
    {
        if (command == null)
        {
            throw new ArgumentNullException("command");
        }

        m_commands.Add(command);

        command.onCompleted += OnCompleted;
    }

    private void OnCompleted(IUndoCommand cmd)
    {
        if (++m_completionNum == m_commands.Count)
        {
            FireCompleted();
        }
    }

    private void FireCompleted()
    {
        if (onCompleted != null)
        {
            onCompleted(this);
        }
    }

    public int commandCount
    {
        get { return m_commands.Count; }
    }

    public void Undo()
    {
        m_completionNum = 0;

        if (m_commands.Count > 0)
        {
            for (int i = m_commands.Count - 1; i >= 0; --i)
            {
                m_commands[i].Undo();
            }
        }
        else
        {
            FireCompleted();
        }
    }

    public void Redo()
    {
        m_completionNum = 0;

        if (m_commands.Count > 0)
        {
            foreach (var command in m_commands)
            {
                command.Redo();
            }
        }
        else
        {
            FireCompleted();
        }
    }

    public bool hasSideEffect
    {
        get
        {
            return m_commands.Any(x => x.hasSideEffect);
        }
    }

    public object userData
    {
        get;
        set;
    }
}
