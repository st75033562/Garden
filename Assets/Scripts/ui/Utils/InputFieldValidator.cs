using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(InputField))]
public class InputFieldValidator : MonoBehaviour
{
    public enum ContentType
    {
        NonNegativeDecimal
    }

    [SerializeField]
    private ContentType m_contentType;

    private InputField m_inputField;

    void Awake()
    {
        m_inputField = GetComponent<InputField>();
        m_inputField.onValidateInput += OnValidateInput;
    }

    public ContentType contentType
    {
        get { return m_contentType; }
        set { m_contentType = value; }
    }

    char OnValidateInput(string text, int pos, char ch)
    {
        switch (contentType)
        {
        case ContentType.NonNegativeDecimal:
            if (ch >= '0' && ch <= '9') return ch;
            if (ch == '.' && !text.Contains(".")) return ch;
            break;
        }
        return (char)0;
    }
}
