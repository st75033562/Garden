using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIMenuDialogConfig
{
    public string[] items;
    public Color color = UIMenuItem.DefaultColor;
}

public class UIMenuDialog : UIInputDialogBase
{
	public UIMenu m_Menu;

    private UIMenuSelectCallback m_OnSelectedItem;
    private IDialogInputCallback m_InputCallback;
    private string m_args;
    private RectTransform m_target;
    private UIMenuConfig m_config;

	public override void Init()
	{
		base.Init();
		m_Menu.Init();
        m_Menu.SetSelectCallback(SelectOne);
    }

    public void Configure(UIMenuConfig config, UIMenuSelectCallback callback, float scale = 1.0f)
    {
        Reset();
        m_config = config;
        m_OnSelectedItem = callback;
        SetScale(scale);
    }

    private void SetScale(float scale)
    {
        m_Menu.transform.localScale = Vector3.one * scale;
    }

    public override void OpenDialog()
    {
        base.OpenDialog();

        if (m_config != null)
        {
            m_Menu.OpenMenu(m_config);
        }
        else
        {
            m_Menu.OpenMenu(m_args, m_target);
        }
    }

    private void Reset()
    {
        m_OnSelectedItem = null;
        m_InputCallback = null;
        m_args = null;
        m_target = null;
        m_config = null;
    }

	public void PointUp()
	{
        EventBus.Default.AddEvent(EventId.GuideInvalidInput, null);
        CloseDialog();
	}

	public void SelectOne(string text)
	{
		if (m_InputCallback != null)
		{
			m_InputCallback.InputCallBack(text);
		}
        if (m_OnSelectedItem != null)
        {
            m_OnSelectedItem(text);
        }
		CloseDialog();
	}

	public override void CloseDialog()
	{
		base.CloseDialog();
		m_Menu.CloseMenu();
        m_OnSelectedItem = null;
	}

    public override UIDialog dialogType
    {
        get { return UIDialog.UIMenuDialog; }
    }

    public override bool OnKey(KeyEventArgs eventArgs)
    {
        return false;
    }
}
