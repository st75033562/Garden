using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;

public class UINumTextInputDialogConfig
{
    public UINumInputDialogConfig numInputConfig;
    public UIEditInputDialogConfig textInputConfig;
}

public class UINumTextInputDialog : UIInputDialogBase
{
    [Flags]
    private enum Mode
    {
        None = 0,
        Number = 0x1,
        Text   = 0x2,
        TextAndNumber = Number | Text
    }

    public UINumInputDialog numInputDialog;
    public GameObject textModeButton;

    public UIEditInputDialog textInputDialog;
    public GameObject numModeButton;

    private UIEditInputDialogConfig m_textDialogCfg;
    private UINumInputDialogConfig m_numDialogCfg;

    private Mode m_mode;
    private Mode m_activeMode;

    private IDialogInputCallback m_inputCallback;
    private IDialogInputValidator m_inputValidator;

    public override void Init()
    {
        base.Init();

        numInputDialog.Init();
        textInputDialog.Init();
    }

    public void Configure(UINumTextInputDialogConfig config, IDialogInputCallback inputCallback, IDialogInputValidator inputValidator)
    {
        m_inputCallback = inputCallback;
        m_inputValidator = inputValidator;

        if (config.numInputConfig != null)
        {
            m_mode |= Mode.Number;
            m_numDialogCfg = config.numInputConfig;
        }
        if (config.textInputConfig != null)
        {
            m_mode |= Mode.Text;
            m_textDialogCfg = config.textInputConfig;
        }

        if (m_mode == Mode.Text)
        {
            OpenDialog(Mode.Text);
        }
        else
        {
            OpenDialog(Mode.Number);
        }
    }

    public override void CloseDialog()
    {
        base.CloseDialog();
        textInputDialog.onClosed -= OnClosedSubDialog;
        numInputDialog.onClosed -= OnClosedSubDialog;
        m_mode = Mode.None;
        m_activeMode = Mode.None;
    }

    private void OpenDialog(Mode mode)
    {
        if (m_activeMode == mode)
        {
            return;
        }

        if (m_activeMode == Mode.Text)
        {
            textInputDialog.onClosed -= OnClosedSubDialog;
            textInputDialog.gameObject.SetActive(false);
        }
        else if (m_activeMode == Mode.Number)
        {
            numInputDialog.onClosed -= OnClosedSubDialog;
            numInputDialog.gameObject.SetActive(false);
        }

        m_activeMode = mode;

        if (mode == Mode.Text)
        {
            textInputDialog.onClosed += OnClosedSubDialog;
            textInputDialog.gameObject.SetActive(true);
            textInputDialog.Configure(m_textDialogCfg, m_inputCallback, m_inputValidator);
            textInputDialog.OpenDialog();
            numModeButton.SetActive(m_mode == Mode.TextAndNumber);
            textModeButton.SetActive(false);
        }
        else
        {
            numInputDialog.onClosed += OnClosedSubDialog;
            numInputDialog.gameObject.SetActive(true);
            numInputDialog.Configure(m_numDialogCfg, m_inputCallback);
            numInputDialog.OpenDialog();
            textModeButton.SetActive(m_mode == Mode.TextAndNumber);
            numModeButton.SetActive(false);
        }
    }

    private void OnClosedSubDialog()
    {
        // user closed the sub-dialog
        CloseDialog();
    }

    public void OnClickTextModeButton()
    {
        OpenDialog(Mode.Text);
    }

    public void OnClickNumModeButton()
    {
        OpenDialog(Mode.Number);
    }

    public override UIDialog dialogType
    {
        get { return UIDialog.UINumTextInputDialog; }
    }
}
