using UnityEngine;
using System.Linq;

public class UIDeleteDataDialog : UIDeleteDataDialogBase
{
    public GameObject m_Template;
    public Sprite[] m_VariableIcons;

    private UIWorkspace m_Workspace;
    private NodeTemplateList m_NodeTempList;
    private VariableManager m_VarManager;

    public void DeleteVar(UIVariableData item)
    {
        if (m_NodeTempList.deletingVariableHandler != null)
        {
            m_NodeTempList.deletingVariableHandler(item.varName, () => InternalDeleteVar(item));
            return;
        }

        InternalDeleteVar(item);
    }

    private void InternalDeleteVar(UIVariableData item)
    {
        var cmd = new DeleteVariableCommand(m_Workspace, item.varName);
        m_Workspace.UndoManager.AddUndo(cmd);

        RemoveItem(item.gameObject);
    }

    public void Configure(UIWorkspace workspace)
    {
        m_Workspace = workspace;
        m_NodeTempList = workspace.m_NodeTempList;
        m_VarManager = workspace.CodeContext.variableManager;

        foreach (var variable in m_VarManager.OrderByDescending(x => x.type).ThenBy(x => x.name))
        {
            UIVariableData item = CreateVariableItem();
            item.m_Icon.sprite = m_VariableIcons[(int)variable.type];
            item.varName = variable.name;
            item.SetGlobal(variable.scope == NameScope.Global);
            item.SetReserved(variable.isReserved);
        }

        Layout();
    }

    private UIVariableData CreateVariableItem()
    {
        GameObject instance = Instantiate(m_Template, m_ContentTrans) as GameObject;
        instance.gameObject.SetActive(true);
        return instance.GetComponent<UIVariableData>();
    }

    public override UIDialog dialogType
    {
        get { return UIDialog.UIDeleteDataDialog; }
    }
}
