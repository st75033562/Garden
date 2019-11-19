using System;
using UnityEngine;
using UnityEngine.UI;

public class UIRecordTimer : MonoBehaviour
{
    public Text m_Text;

    private float m_RecorderTime = -1.0f;

    public void Begin()
    {
        m_RecorderTime = 0.0f;
    }

    public void End()
    {
        m_RecorderTime = -1.0f;
    }

    void Update()
    {
        if (m_RecorderTime >= 0.0f)
        {
            m_RecorderTime += Time.unscaledDeltaTime;
            int seconds = (int)Math.Round(m_RecorderTime, 0);
            m_Text.text = TimeUtils.GetHHmmssString(seconds);
        }
    }

    void Reset()
    {
        m_Text = GetComponent<Text>();
    }
}
