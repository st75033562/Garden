using UnityEngine;
using UnityEngine.UI;

public class ScreenText : MonoBehaviour
{
    public Text m_text;

    private Image m_background;

    void Awake()
    {
        m_background = GetComponent<Image>();
    }

    public void SetText(string text, int fontSize, Color color)
    {
        m_text.text = text;
        m_text.fontSize = fontSize;
        m_text.color = color;

        var pos = m_text.rectTransform.localPosition;
        m_background.rectTransform.SetSize(pos.x * 2.0f + m_text.preferredWidth, -pos.y * 2.0f + m_text.preferredHeight);
    }

    public float backgroundBrightness
    {
        get { return m_background.color.r; }
        set
        {
            var color = m_background.color;
            color.r = color.g = color.b = Mathf.Clamp01(value);
            m_background.color = color;
        }
    }

    public float backgroundAlpha
    {
        get { return m_background.color.a; }
        set
        {
            var color = m_background.color;
            color.a = Mathf.Clamp01(value);
            m_background.color = color;
            m_background.enabled = color.a != 0.0f;
        }
    }
}
