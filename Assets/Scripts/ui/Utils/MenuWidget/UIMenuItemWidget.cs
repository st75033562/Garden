using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIMenuItemWidget : MonoBehaviour
{
    public Text m_text;

    protected virtual void Start()
    {
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    public string text
    {
        get { return m_text.text; }
        set { m_text.text = value; }
    }

    public Action<UIMenuItemWidget> onClick { get; set; }

    internal int itemIndex { get; set; }

    protected virtual void OnClick()
    {
        if (onClick != null)
        {
            onClick(this);
        }
    }
}
