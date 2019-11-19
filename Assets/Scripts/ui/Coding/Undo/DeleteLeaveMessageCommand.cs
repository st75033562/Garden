using System;

public class DeleteLeaveMessageCommand : BaseLeaveMessageCommand
{
    private readonly string m_messageKey;
    private readonly int m_messageIndex;
    private readonly LeaveMessage m_message;

    public DeleteLeaveMessageCommand(LeaveMessagePanel panel, string messageKey, int messageIndex)
        : base(panel)
    {
        m_messageKey = messageKey;
        m_messageIndex = messageIndex;
        var msgList = panel.MessageDataSource.getMessage(messageKey);
        if (msgList == null)
        {
            throw new ArgumentException("messageKey");
        }
        m_message = msgList.LeaveMessages[messageIndex];
    }

    protected override void UndoImpl()
    {
        m_panel.InsertMessage(m_messageKey, m_message, m_messageIndex);
    }

    protected override void RedoImpl()
    {
        m_panel.DeleteMessage(m_messageKey, m_messageIndex);
    }
}
