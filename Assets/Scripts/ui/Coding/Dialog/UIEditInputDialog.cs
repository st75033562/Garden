using UnityEngine;
using UnityEngine.UI;
using System;

public class UIEditInputDialogConfig
{
    public string title;
    public string content = string.Empty;
    public string confirmText = "ui_ok";
    public string inputHint = string.Empty;
    public bool allowEmpty;
    public bool multiline; // optional
}

public class UIEditInputDialog : UIInputDialogBase
{
    public InputField m_InputText;
    public Text m_PlaceholderText;

    public LayoutElement m_InputLayout;
    public float m_MultilineHeight;
    public float m_SinglelineHeight;

    public Text m_TitleText;
    public Text m_ConfirmText;
    public Text m_ErrorText;

    public Button m_ConfirmButton;

    private bool m_AllowEmpty;
    private IDialogInputCallback m_InputCallback;
    private IDialogInputValidator m_InputValidator;

    public void Confirm()
    {
        if (m_InputCallback != null)
        {
            m_InputCallback.InputCallBack(GetCallbackResult());
        }
        CloseDialog();
    }

    protected virtual string GetCallbackResult()
    {
        return m_InputText.text;
    }

    public void Configure(UIEditInputDialogConfig config, IDialogInputCallback inputCallback, IDialogInputValidator validator)
    {
        m_InputCallback = inputCallback;
        m_InputValidator = validator;

        m_TitleText.text = config.title.Localize();
        if (!string.IsNullOrEmpty(config.confirmText))
        {
            m_ConfirmText.text = config.confirmText.Localize();
        }
        m_AllowEmpty = config.allowEmpty;

        if (config.multiline && m_MultilineHeight > 0)
        {
            SetupLayout(m_MultilineHeight, InputField.LineType.MultiLineNewline, TextAnchor.UpperLeft);
        }
        else if (m_SinglelineHeight > 0)
        {
            SetupLayout(m_SinglelineHeight, InputField.LineType.SingleLine, TextAnchor.MiddleLeft);
        }
        m_InputText.text = config.content;
        m_PlaceholderText.text = config.inputHint.Localize();
        InputValueChanged();
    }

    void SetupLayout(float height, InputField.LineType lineType, TextAnchor alignment)
    {
        m_InputText.lineType = lineType;
        m_InputText.textComponent.alignment = alignment;
        m_PlaceholderText.alignment = alignment;
        m_InputLayout.preferredHeight = height;
    }

    public void InputValueChanged()
    {
        m_ConfirmButton.interactable = ValidateInput();
    }

    protected virtual bool ValidateInput()
    {
        if ("" == m_InputText.text && !m_AllowEmpty)
        {
            m_ErrorText.text = string.Empty;
            return false;
        }
        else
        {
            if (m_InputValidator != null)
            {
                string errorHint = m_InputValidator.ValidateInput(m_InputText.text);
                if (!string.IsNullOrEmpty(errorHint))
                {
                    m_ErrorText.text = errorHint;
                    return false;
                }
            }
            m_ErrorText.text = string.Empty;
            return true;
        }
    }

    public override UIDialog dialogType
    {
        get { return UIDialog.UIEditInputDialog; }
    }
}
