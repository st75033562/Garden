using System;
using System.Collections.Generic;

public class AddVariablesCommand : BaseWorkspaceCommand
{
    private readonly IEnumerable<BaseVariable> m_variables;

    public AddVariablesCommand(UIWorkspace workspace, BaseVariable variable)
        : this(workspace, new[] {  variable })
    {
        if (variable == null)
        {
            throw new ArgumentNullException("variable");
        }
    }

    public AddVariablesCommand(UIWorkspace workspace, IEnumerable<BaseVariable> variables)
        : base(workspace)
    {
        if (variables == null)
        {
            throw new ArgumentException("variables");
        }

        m_variables = variables;
    }

    protected override void UndoImpl()
    {
        foreach (var variable in m_variables)
        {
            m_workspace.CodeContext.variableManager.remove(variable.name);
        }
        m_workspace.m_NodeTempList.RefreshDataNode();
    }

    protected override void RedoImpl()
    {
        foreach (var variable in m_variables)
        {
            m_workspace.CodeContext.variableManager.add(variable);
        }
        m_workspace.m_NodeTempList.RefreshDataNode();
    }
}
