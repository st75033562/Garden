using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
[RequireComponent(typeof(Text))]
public class ResourceFont : MonoBehaviour
{
    [SerializeField]
    private string m_fontName;

    private Text m_text;

    void Awake()
    {
        m_text = GetComponent<Text>();
        RefreshFont();
    }

    public string fontName
    {
        get { return m_fontName; }
        set
        {
            if (m_fontName != value)
            {
                m_fontName = value;
                RefreshFont();
            }
        }
    }

    public void RefreshFont()
    {
        if (!string.IsNullOrEmpty(m_fontName))
        {
            m_text.font = Resources.Load<Font>("Fonts/" + m_fontName);
        }
    }
}
