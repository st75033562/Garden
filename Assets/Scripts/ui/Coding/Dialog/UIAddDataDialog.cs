using System;
using System.Collections;
using UnityEngine;

public class UIAddDataDialog : UIInputDialogBase
{
	public GameObject m_StackNode;
	public GameObject m_QueueNode;

    private UIWorkspace m_Workspace;

    private VariableManager varManager
    {
        get { return m_Workspace.CodeContext.variableManager; }
    }

	public void AddVar()
	{
        var config = new UIEditInputDialogConfig {
            title = "input_data_var_name",
        };
        OpenEditVariableNameDialog(config, result => {
            AddVariable(new VariableData(result.name, result.scope) {
                globalVarOwner = result.globalVarOwner
            });
        });
		CloseDialog();
	}

	public void AddList()
	{
        var config = new UIEditInputDialogConfig {
            title = "input_data_list_name",
        };
        OpenEditVariableNameDialog(config, result => {
            AddVariable(new ListData(result.name, result.scope) {
                globalVarOwner = result.globalVarOwner
            });
        });
		CloseDialog();
	}

	public void AddStack()
	{
        var config = new UIEditInputDialogConfig {
            title = "input_data_stack_name",
        };

        OpenEditVariableNameDialog(config, result => {
            AddVariable(new StackData(result.name, result.scope) { 
                globalVarOwner = result.globalVarOwner
            });
        });
		CloseDialog();
	}

	public void AddQueue()
	{
        var config = new UIEditInputDialogConfig {
            title = "input_data_queue_name",
        };

        OpenEditVariableNameDialog(config, result => {
            AddVariable(new QueueData(result.name, result.scope) {
                globalVarOwner = result.globalVarOwner
            });
        });
		CloseDialog();
	}

    void AddVariable(BaseVariable variable)
    {
        var cmd = new AddVariablesCommand(m_Workspace, new[] { variable });
        m_Workspace.UndoManager.AddUndo(cmd);
    }

    private void OpenEditVariableNameDialog(UIEditInputDialogConfig config, Action<UIEditDataDialogResult> onAddData)
    {
        var handler = new AddDataDialogHandler(m_Workspace.CodeContext.variableManager, onAddData);
		var dialog = UIDialogManager.g_Instance.GetDialog<UIEditDataDialog>();
        dialog.ShowGlobalFlag(m_Workspace.m_NodeTempList.CanAddGlobalData);
        dialog.Configure(config, handler, handler);
        dialog.OpenDialog();
    }

	public void Configure(UIWorkspace workspace)
	{
        m_Workspace = workspace;
        bool isExpert = workspace.BlockLevel == BlockLevel.Advanced;
        m_StackNode.SetActive(isExpert);
        m_QueueNode.SetActive(isExpert);
	}

    public override UIDialog dialogType
    {
        get { return UIDialog.UIAddDataDialog; }
    }
}
