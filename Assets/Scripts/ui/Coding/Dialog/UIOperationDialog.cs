using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using UnityEngine.UI;

public enum UIOperationButton
{
    New,
    Save,
    Open,
    Share,
    Setting,
    Help,
    Num
}

public class UIOperationButtonConfig
{
    public UIOperationButton button;
    public Action callback; // callback for setting is ignored
    public bool interactable = true;
}

public class UIOperationDialogConfig
{
    public readonly UIOperationButtonConfig[] configs = new UIOperationButtonConfig[(int)UIOperationButton.Num];
    public UIWorkspace workspace; // can be null if setting button is not visible or interactable

    public void AddAllButtons()
    {
        for (int i = 0; i < configs.Length; ++i)
        {
            configs[i] = new UIOperationButtonConfig();
        }
    }

    public void AddButton(UIOperationButton button, Action callback = null)
    {
        configs[(int)button] = new UIOperationButtonConfig {
            button = button,
            callback = callback
        };
    }

    public UIOperationButtonConfig GetButton(UIOperationButton button)
    {
        return configs[(int)button];
    }
}

public class UIOperationDialog : UIInputDialogBase
{
    public Button[] m_Buttons;

    private UIWorkspace m_Workspace;
    private Action[] m_ButtonCallbacks;

    public override void Init()
    {
        base.Init();
        m_ButtonCallbacks = new Action[m_Buttons.Length];
    }

    public void Configure(UIOperationDialogConfig config)
    {
        m_Workspace = config.workspace;
        for (int i = 0; i < m_Buttons.Length; ++i)
        {
            var cfg = config.configs[i];
            m_Buttons[i].gameObject.SetActive(cfg != null);
            if (cfg != null)
            {
                m_Buttons[i].interactable = cfg.interactable;
                m_ButtonCallbacks[i] = cfg.callback;

#if UNITY_EDITOR
                if (i == (int)UIOperationButton.Setting && cfg.interactable)
                {
                    Assert.IsNotNull(m_Workspace);
                }
#endif
            }
        }
    }

	public void NewProject()
	{
        FireCallback(UIOperationButton.New);
		CloseDialog();
	}

	public void SaveProject()
	{
        FireCallback(UIOperationButton.Save);
        CloseDialog();
	}

	public void OpenProject()
	{
        FireCallback(UIOperationButton.Open);
		CloseDialog();
	}

	public void Share()
	{
        FireCallback(UIOperationButton.Share);
		CloseDialog();
	}

	public void Setting()
	{
        var dialog = UIDialogManager.g_Instance.GetDialog<UISystemSettingsDialog>();
        dialog.Configure(new WorkspaceSettingsViewModel(m_Workspace));
        dialog.OpenDialog();
		CloseDialog();
	}

	public void Help()
	{
        FireCallback(UIOperationButton.Help);
		CloseDialog();
	}

    private void FireCallback(UIOperationButton button)
    {
        var cb = m_ButtonCallbacks[(int)button];
        if (cb != null)
        {
            cb();
        }
    }

    public override UIDialog dialogType
    {
        get { return UIDialog.UIOperationDialog; }
    }
}
