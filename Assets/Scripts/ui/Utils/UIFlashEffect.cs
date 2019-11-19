using UnityEngine;
using UnityEngine.UI;

public class UIFlashEffect : MonoBehaviour
{
    public float m_flashInterval = 0.5f;
    public Image m_target;

    private float m_time;

    void OnEnable()
    {
        m_time = 0;
    }

    void Update()
    {
        m_time += Time.deltaTime;
        if (m_time > m_flashInterval)
        {
            m_time -= m_flashInterval;
            m_target.enabled = !m_target.enabled;
        }
    }

    void Reset()
    {
        m_target = GetComponent<Image>();
    }
}
