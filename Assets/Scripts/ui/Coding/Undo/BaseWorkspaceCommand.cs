using System;

public abstract class BaseWorkspaceCommand : BaseUndoCommand
{
    protected readonly UIWorkspace m_workspace;

    protected BaseWorkspaceCommand(UIWorkspace workspace)
        : base(true)
    {
        if (workspace == null)
        {
            throw new ArgumentNullException("workspace");
        }

        m_workspace = workspace;
    }
}
