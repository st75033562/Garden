using System;

public abstract class BaseLeaveMessageCommand : BaseUndoCommand
{
    protected readonly LeaveMessagePanel m_panel;

    protected BaseLeaveMessageCommand(LeaveMessagePanel panel)
        : base(true)
    {
        if (panel == null)
        {
            throw new ArgumentNullException("panel");
        }

        m_panel = panel;
    }
}
