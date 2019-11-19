using System;

public class AddLeaveMessageCommand : BaseLeaveMessageCommand
{
    private readonly string m_key;
    private readonly LeaveMessage m_message;

    public AddLeaveMessageCommand(LeaveMessagePanel panel, string key, LeaveMessage message)
        : base(panel)
    {
        if (string.IsNullOrEmpty(key))
        {
            throw new ArgumentException("key");
        }

        if (message == null)
        {
            throw new ArgumentNullException("message");
        }

        m_key = key;
        m_message = message;
    }

    protected override void UndoImpl()
    {
        m_panel.DeleteMessages(m_key, new[] { m_message });
    }

    protected override void RedoImpl()
    {
        m_panel.AddMessage(m_key, m_message);
    }
}
