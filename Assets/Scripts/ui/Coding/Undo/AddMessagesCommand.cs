using System;
using System.Collections.Generic;

public class AddMessageCommand : BaseWorkspaceCommand
{
    private readonly IEnumerable<Message> m_messages;

    public AddMessageCommand(UIWorkspace workspace, IEnumerable<Message> messages)
        : base(workspace)
    {
        if (messages == null)
        {
            throw new ArgumentNullException("messages");
        }

        m_messages = messages;
    }

    protected override void UndoImpl()
    {
        foreach (var message in m_messages)
        {
            m_workspace.CodeContext.messageManager.delete(message.name);
        }
        m_workspace.m_NodeTempList.RefreshEventNode();
    }

    protected override void RedoImpl()
    {
        foreach (var message in m_messages)
        {
            m_workspace.CodeContext.messageManager.add(message);
        }
        m_workspace.m_NodeTempList.RefreshEventNode();
    }
}
