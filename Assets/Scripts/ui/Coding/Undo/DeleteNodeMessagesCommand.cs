using System;
using System.Collections.Generic;

public class DeleteNodeMessagesCommand : BaseLeaveMessageCommand
{
    private DeleteMessagesResult m_deleteResult;
    private int m_nodeIndex;

    public DeleteNodeMessagesCommand(LeaveMessagePanel panel, int nodeIndex)
        : base(panel)
    {
        m_nodeIndex = nodeIndex;
    }

    protected override void UndoImpl()
    {
        for (int j = m_deleteResult.deletedMsgLists.Count - 1; j >= 0; --j)
        {
            m_panel.AddMessageList(m_deleteResult.deletedMsgLists[j]);
        }

        for (int j = m_deleteResult.changedMessages.Count - 1; j >= 0; --j)
        {
            var change = m_deleteResult.changedMessages[j];
            m_panel.DeleteMessages(change.newKey, change.oldMsgList.LeaveMessages);
            m_panel.AddMessageList(change.oldMsgList);
        }
    }

    protected override void RedoImpl()
    {
        m_deleteResult = m_panel.DeleteNodeMessages(m_nodeIndex);
    }
}
