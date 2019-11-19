using System;
using UnityEngine;

public class RenameVariableCommand : BaseUndoCommand
{
    private readonly VariableManager m_varManager;
    private readonly string m_oldName;
    private readonly string m_newName;

    public RenameVariableCommand(VariableManager varManager, string oldName, string newName)
        : base(true)
    {
        if (varManager == null)
        {
            throw new ArgumentNullException("varManager");
        }
        if (string.IsNullOrEmpty(oldName))
        {
            throw new ArgumentException("oldName");
        }
        if (string.IsNullOrEmpty(newName))
        {
            throw new ArgumentException("newName");
        }

        m_varManager = varManager;
        m_oldName = oldName;
        m_newName = newName;
    }

    protected override void UndoImpl()
    {
        Rename(m_newName, m_oldName);
    }

    protected override void RedoImpl()
    {
        Rename(m_oldName, m_newName);
    }

    private void Rename(string oldName, string newName)
    {
        var variable = m_varManager.get(oldName);
        if (variable == null)
        {
            Debug.LogErrorFormat("variable {0} not found", oldName);
            return;
        }

        m_varManager.rename(variable, newName);
    }
}
