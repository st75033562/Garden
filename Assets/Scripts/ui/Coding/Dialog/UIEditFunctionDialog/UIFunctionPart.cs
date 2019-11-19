using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[ExecuteInEditMode]
public class UIFunctionPart : MonoBehaviour, IPointerDownHandler
{
    [SerializeField]
    private FunctionPartType m_type;

    public event Action<UIFunctionPart> onPointerDown;
    public InputField m_textInput;

    public string text
    {
        get { return m_textInput.text; }
        set { m_textInput.text = value; }
    }

    public void BeginEdit()
    {
        m_textInput.ActivateInputField();
    }

    public FunctionPartType type { get { return m_type; ; } }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (onPointerDown != null)
        {
            onPointerDown(this);
        }
    }
}