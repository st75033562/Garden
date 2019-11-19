using UnityEngine.UI;

public class SimpleTabButtonWidget : TabButtonWidget
{
    public Text m_buttonText;
    public Image m_underlineImage;

    public override bool isOn
    {
        get
        {
            return m_underlineImage.enabled;
        }
        set
        {
            m_underlineImage.enabled = value;
        }
    }

    public string text
    {
        get { return m_buttonText.text; }
        set { m_buttonText.text = value; }
    }
}
