using System;

public class DeleteMessageCommand : BaseWorkspaceCommand
{
    private readonly Message m_message;

    public DeleteMessageCommand(UIWorkspace workspace, string name)
        : base(workspace)
    {
        m_message = workspace.CodeContext.messageManager.get(name);
        if (m_message == null)
        {
            throw new ArgumentException("message");
        }
    }

    protected override void UndoImpl()
    {
        m_workspace.CodeContext.messageManager.add(m_message);
        m_workspace.m_NodeTempList.RefreshEventNode();
    }

    protected override void RedoImpl()
    {
        m_workspace.CodeContext.messageManager.delete(m_message.name);
        m_workspace.m_NodeTempList.RefreshEventNode();
    }
}
