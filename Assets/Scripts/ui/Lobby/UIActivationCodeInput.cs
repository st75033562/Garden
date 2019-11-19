using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIActivationCodeInput : MonoBehaviour
{
    public InputField inputCode;

    [Serializable]
    public class OnValidChangedEvent : UnityEvent<bool> { }

    public OnValidChangedEvent onValidChanged;

    public const int CodeLength = 12;
    private const int CharsPerGroup = 4;

    private string m_formattedCode = string.Empty;
    private string m_rawCode = string.Empty;
    private bool m_valid;

    void Awake()
    {
        inputCode.onValidateInput += OnValidateInput;
    }

    /// <summary>
    /// true if code is valid
    /// </summary>
    public bool valid
    {
        get { return m_valid; }
        private set
        {
            if (m_valid != value)
            {
                m_valid = value;
                if (onValidChanged != null)
                {
                    onValidChanged.Invoke(m_valid);
                }
            }
        }
    }

    public string code
    {
        get
        {
            FormatCodeIfChanged();
            return m_rawCode;
        }
        set
        {
            inputCode.text = value;
        }
    }

    char OnValidateInput(string text, int charIndex, char addedChar)
    {
        return (char.IsLetterOrDigit(addedChar) || addedChar == ' ') ? addedChar : (char)0;
    }

    void LateUpdate()
    {
        FormatCodeIfChanged();
    }

    private void FormatCodeIfChanged()
    {
        if (inputCode.text != m_formattedCode)
        {
            var logicalCaretPos = inputCode.text.Take(inputCode.caretPosition).Count(x => x != ' ');

            string rawText = GetRawText(inputCode.text);

            // raw code not changed, a whitespace has been deleted, remove the code before the caret
            if (rawText == m_rawCode && inputCode.text.Length < m_formattedCode.Length)
            {
                rawText = GetRawText(inputCode.text.Remove(inputCode.caretPosition - 1, 1));
                --logicalCaretPos;
            }

            string formatted = "";
            for (int i = 0; i < rawText.Length; )
            {
                if (i > 0)
                {
                    formatted += " ";
                }
                string group = rawText.Substring(i, Mathf.Min(CharsPerGroup, rawText.Length - i));
                formatted += group;
                i += group.Length;
            }

            m_formattedCode = inputCode.text = formatted;
            m_rawCode = rawText;
            valid = rawText.Length == CodeLength;
            // after setting text of InputField, caret will be placed at the end
            // we need to restore the caret
            inputCode.caretPosition = logicalCaretPos + (logicalCaretPos / CharsPerGroup);
        }
    }

    private string GetRawText(string text)
    {
        string rawText = text.Replace(" ", "");
        return rawText.Substring(0, Mathf.Min(CodeLength, rawText.Length));
    }

    void Reset()
    {
        inputCode = GetComponent<InputField>();
        inputCode.contentType = InputField.ContentType.Standard;
        inputCode.lineType = InputField.LineType.SingleLine;
        inputCode.characterLimit = CodeLength + (CodeLength / CharsPerGroup) - 1;
    }
}
