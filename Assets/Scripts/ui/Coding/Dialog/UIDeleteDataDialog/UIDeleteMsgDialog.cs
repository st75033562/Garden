using UnityEngine;
using System.Linq;

public class UIDeleteMsgDialog : UIDeleteDataDialogBase
{
	public GameObject m_Template;
    private UIWorkspace m_Workspace;

	public void DeleteMsg(UIMsgData item)
	{
        var cmd = new DeleteMessageCommand(m_Workspace, item.m_VarName.text);
        m_Workspace.UndoManager.AddUndo(cmd);

        RemoveItem(item.gameObject);
	}

    public void Configure(UIWorkspace workspace)
    {
        m_Workspace = workspace;
    }

	public override void OpenDialog()
	{
        base.OpenDialog();

        var defaultMsg = new[] { Message.defaultMessage };
        var messages = m_Workspace.CodeContext.messageManager
                            .Except(defaultMsg, DelegatedEqualityComparer.Of<Message>((a, b) => a.name == b.name))
                            .OrderByDescending(x => x.scope)
                            .ThenBy(x => x.name)
                            .Concat(defaultMsg);

        foreach (var msg in messages)
        {
            UIMsgData item = CreateMsgItem();
            item.m_VarName.text = msg.name;
            item.SetGlobal(msg.scope == NameScope.Global);
            item.SetDeletable(msg != Message.defaultMessage);
        }

        Layout();
    }

    private UIMsgData CreateMsgItem()
    {
        GameObject instance = Instantiate(m_Template, m_ContentTrans) as GameObject;
        instance.gameObject.SetActive(true);
        return instance.GetComponent<UIMsgData>();
    }

    public override UIDialog dialogType
    {
        get { return UIDialog.UIDeleteMsgDialog; }
    }
}
