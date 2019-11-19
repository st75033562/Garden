using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIPauseButton : MonoBehaviour
{
    public Image m_pauseIndicator;
    public Image m_background;
    public Color m_pauseColor = Color.white;
    public float m_fadeTime;
    private Color m_normalColor;
    private float m_indicatorAlpha;

    private bool m_paused;
    private Button m_button;
    private float m_deltaTime;

    void Awake()
    {
        m_button = GetComponent<Button>();
        m_normalColor = m_background.color;
        m_indicatorAlpha = m_pauseIndicator.color.a;
        isPaused = false;
    }

    public UnityEvent onClick
    {
        get { return m_button.onClick; }
    }

    public bool interactable
    {
        get { return m_button.interactable; }
        set { m_button.interactable = value; }
    }

    public bool isPaused
    {
        get { return m_paused; }
        set
        {
            m_paused = value;
            m_background.color = m_paused ? m_pauseColor : m_normalColor;
            if (!value)
            {
                var color = m_pauseIndicator.color;
                color.a = m_indicatorAlpha;
                m_pauseIndicator.color = color;
                m_deltaTime = 0.0f;
            }
            else
            {
                // start with full alpha
                m_deltaTime = m_fadeTime;
            }
        }
    }

    void Update()
    {
        if (m_paused)
        {
            m_deltaTime += Time.unscaledDeltaTime;
            var color = m_pauseIndicator.color;
            color.a = Mathf.PingPong(m_deltaTime / m_fadeTime, 1.0f);
            m_pauseIndicator.color = color;
        }
    }
}
