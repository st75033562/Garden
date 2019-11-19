using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class BotIndexToggle : UIBotIndex
{
    public Color bgOnColor = Color.white;
    public Color textOnColor = Color.white;

    private Color m_bgOffColor;
    private Color m_textOffColor;

    [SerializeField]
    private Image m_targetImage;

    private bool m_isOn;

    public bool isOn
    {
        get { return m_isOn; }
        set
        {
            m_isOn = value;
            m_targetImage.color = value ? bgOnColor : m_bgOffColor;
            m_Index.color = value ? textOnColor : m_textOffColor;
        }
    }

    void Awake()
    {
        m_bgOffColor = m_targetImage.color;
        m_textOffColor = m_Index.color;
    }

    void Reset()
    {
        m_targetImage = GetComponent<Image>();
    }
}
