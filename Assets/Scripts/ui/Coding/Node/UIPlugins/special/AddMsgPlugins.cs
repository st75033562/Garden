using System;

public class AddMsgPlugins : IDialogInputCallback, IDialogInputValidator
{
    private readonly UIWorkspace m_workspace;

    public AddMsgPlugins(UIWorkspace workspace)
    {
        if (workspace == null)
        {
            throw new ArgumentNullException("workspace");
        }
        m_workspace = workspace;
    }

	public void InputCallBack(string str)
	{
        var result = UIEditMessageDialogResult.Parse(str);
        var msg = new Message(result.name, result.global ? NameScope.Global : NameScope.Local);
        msg.targetRobotIndices.AddRange(result.targetRobots);

        var cmd = new AddMessageCommand(m_workspace, new[] { msg });
        m_workspace.UndoManager.AddUndo(cmd);
    }

	public string ValidateInput(string value)
	{
        if (m_workspace.CodeContext.messageManager.has(value))
        {
            return "name_already_in_use".Localize();
        }

        return null;
	}
}
