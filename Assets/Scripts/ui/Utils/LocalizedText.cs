using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class LocalizedText : LocalizedComponent
{
    private Text m_text;

    [SerializeField]
    private string m_stringId;

    protected override void Awake()
    {
        base.Awake();

        m_text = GetComponent<Text>();
    }

    protected override void OnLanguageChanged()
    {
        m_text.SetLocText(m_stringId);
    }

    public string stringId
    {
        get { return m_stringId; }
        set
        {
            if (m_stringId != value)
            {
                m_stringId = value;
                m_text.SetLocText(value);
            }
        }
    }
}
