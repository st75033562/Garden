using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class UIMenuOption : MonoBehaviour
{
    [SerializeField]
    private UnityEvent m_onClicked = new UnityEvent();

    [SerializeField]
	private Text m_text;

    private Selectable m_button;

    void Awake()
    {
        m_button = GetComponent<Selectable>();
        clickable = true;
    }

    public string text
    {
        get { return m_text.text; }
        set { m_text.text = value; }
    }

    public Color textColor
    {
        get { return m_text.color; }
        set { m_text.color = value; }
    }

    public Color highlightColor
    {
        get { return m_button.colors.pressedColor; }
        set
        {
            var colors = m_button.colors;
            colors.highlightedColor = colors.pressedColor = value;
            m_button.colors = colors;
        }
    }

    public bool clickable
    {
        get;
        set;
    }

    public void OnClick()
    {
        if (clickable)
        {
            m_onClicked.Invoke();
        }
    }

    public string key { get; set; }

    public int index { get; internal set; }
}
