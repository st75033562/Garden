using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public abstract class RobotCodeControllerBase : MonoBehaviour
{
    [SerializeField] protected UIWorkspace m_Workspace;
    protected SaveCodeController m_saveController;

    protected virtual void Start()
    {
        m_Workspace.OnSystemMenuClicked += OpenSystemMenu;
        m_Workspace.OnBackClicked += Exit;
        m_Workspace.m_OnStartRunning.AddListener(OnStartRunningCode);
        m_Workspace.m_OnStopRunning.AddListener(OnStopRunningCode);

        m_saveController = new SaveCodeController(m_Workspace);
    }

    protected virtual void OnDestroy()
    { }

	public virtual void OnStartRunningCode()
	{
        m_Workspace.UndoManager.undoEnabled = false;
	}

	public virtual void OnStopRunningCode()
	{
        m_Workspace.UndoManager.undoEnabled = true;
	}

	public void OpenSystemMenu()
	{
        var dialog = UIDialogManager.g_Instance.GetDialog<UIOperationDialog>();
        var dialogConfig = new UIOperationDialogConfig();
        dialogConfig.workspace = m_Workspace;
        for (var i = UIOperationButton.New; i < UIOperationButton.Num; ++i)
        {
            if (i == UIOperationButton.Share || i == UIOperationButton.Help)
            {
                continue;
            }

            dialogConfig.AddButton(i);
            var cfg = dialogConfig.GetButton(i);
            ConfigOperationButton(cfg);

            switch (i)
            {
            case UIOperationButton.New:
                cfg.callback = OnNewProject;
                break;

            case UIOperationButton.Open:
                cfg.callback = OnOpenProject;
                break;

            case UIOperationButton.Save:
                cfg.callback = OnSaveProject;
                break;
            }
        }

        dialog.Configure(dialogConfig);
        dialog.OpenDialog();
	}

    protected virtual void ConfigOperationButton(UIOperationButtonConfig config) { }

    void OnNewProject()
    {
        m_saveController.SaveWithConfirm(status => {
            if (status == SaveCodeStatus.Saved)
            {
                OnProjectSaved();
            }
            NewProject(null);
        });
    }

    private void NewProject(string workingDir)
    {
        m_Workspace.New();
        m_Workspace.Id = null;
        if (workingDir != null)
        {
            m_Workspace.WorkingDirectory = workingDir;
        }
        OnProjectNew();
    }


    protected virtual void OnProjectNew() { }

    protected virtual void OnProjectSaved() { }

    void OnOpenProject()
    {
        m_saveController.SaveWithConfirm(status => {
            if (status == SaveCodeStatus.Saved)
            {
                OnProjectSaved();
            }
            SelectProject();
        });
    }

    private void SelectProject()
    {
        PopupManager.ProjectView(path => {
            if (path.name != "")
            {
                LoadProject(path);
            }
            else
            {
                NewProject(path.parent.ToString());
            }
        }, OnAbortOpen);
    }

    protected virtual void LoadProject(IRepositoryPath path) { }

    protected virtual void OnAbortOpen() { }

    void OnSaveProject()
    {
        m_saveController.SaveAs(OnProjectSaved);
    }

    public void OnBackPressed()
    {
        if (!m_Workspace.IsDragging)
        {
            Exit();
        }
    }

    public virtual void Exit() { }
}
