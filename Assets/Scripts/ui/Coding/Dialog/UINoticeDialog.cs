using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;

public class UINoticeConfig
{
    public string title;
    public string notice;

    public IDialogInputCallback callback;

    public class ButtonConfig
    {
        public string textId;
        public string cmd;
    }

    public static readonly string OkCmd = "OK";
    public static readonly string CancelCmd = "CANCEL";

    public static readonly ButtonConfig DefaultCancelButtonConfig = new ButtonConfig {
        textId = "ui_cancel",
        cmd = OkCmd,
    };

    public static readonly ButtonConfig DefaultOkButtonConfig = new ButtonConfig {
        textId = "ui_ok",
        cmd = CancelCmd,
    };

    public ButtonConfig cancelButton;
    public ButtonConfig okButton;

    public static UINoticeConfig Ok(string title, string notice, IDialogInputCallback callback = null)
    {
        return new UINoticeConfig {
            title = title,
            notice = notice,
            callback = callback,
            okButton = DefaultOkButtonConfig
        };
    }

    public static UINoticeConfig Parse(string cmd)
    {
		string[] mParams = cmd.Split(',');
		if (mParams.Length < 6)
		{
			Debug.LogError("UINoticeDialog OpenDialog Params not match");
            throw new FormatException();
		}

        var config = new UINoticeConfig();
	    config.title = mParams[0];
		config.notice = mParams[1];
        if (mParams[2] != "")
        {
            config.cancelButton = new ButtonConfig {
                textId = mParams[2],
                cmd = mParams[5]
            };
        }
        if (mParams[3] != "")
        {
            config.okButton = new ButtonConfig {
                textId = mParams[3],
                cmd = mParams[4]
            };
        }
        return config;
    }
}

public class UINoticeDialog : UIInputDialogBase
{
	public Text m_Title;
	public Text m_Notice;

	public Text m_Cancel;
	public Text m_Confirm;
    public GameObject cancelButton;
    public GameObject okButton;

	string m_Confirm_CMD;
	string m_Cancel_CMD;

    private IDialogInputCallback m_Callback;
		
	public void Cancel()
	{
		if (m_Callback != null)
		{
			m_Callback.InputCallBack(m_Cancel_CMD);
		}
		CloseDialog();
	}

	public void Confirm()
	{
		if (m_Callback != null)
		{
			m_Callback.InputCallBack(m_Confirm_CMD);
		}
		CloseDialog();
	}

    public void Configure(UINoticeConfig config)
    {
        m_Title.text = config.title;
        m_Notice.text = config.notice;
        m_Callback = config.callback;

        if (config.cancelButton != null)
        {
            m_Cancel.text = config.cancelButton.textId.Localize();
            m_Cancel_CMD = config.cancelButton.cmd;
            cancelButton.SetActive(true);
        }
        else
        {
            cancelButton.SetActive(false);
        }

        if (config.okButton != null)
        {
            m_Confirm.text = config.okButton.textId.Localize();
            m_Confirm_CMD = config.okButton.cmd;
            okButton.SetActive(true);
        }
        else
        {
            okButton.SetActive(false);
        }
    }

	public void Configure(string cmd, IDialogInputCallback callback)
	{
        var config = UINoticeConfig.Parse(cmd);
        config.callback = callback;
        Configure(config);
	}

    public static UINoticeDialog Ok(string title, string notice)
    {
        var dialog = UIDialogManager.g_Instance.GetDialog<UINoticeDialog>();
        dialog.Configure(UINoticeConfig.Ok(title, notice));
        dialog.OpenDialog();
        return dialog;
    }

    public override UIDialog dialogType
    {
        get { return UIDialog.UINoticeDialog; }
    }
}
