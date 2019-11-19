using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UISendCodeButton : MonoBehaviour
{
    public const float CoolDownTime = 60;

    public Button sendCodeButton;
    public Text sendCodeText;

    private DateTime? m_sendTime;
    private float m_remainingTime;

    void OnEnable()
    {
        if (m_sendTime != null)
        {
            var remainingTime = CoolDownTime - (float)(DateTime.UtcNow - m_sendTime.Value).TotalSeconds;
            StartCoroutine(UpdateSendCodeButton(Mathf.Max(0, Mathf.RoundToInt(remainingTime))));
        }
    }

    public void Reset()
    {
        StopAllCoroutines();
        m_sendTime = null;
        m_remainingTime = 0;
        sendCodeText.text = "ui_register_code_send".Localize();
        sendCodeButton.interactable = true;
    }

    public void Enable(bool enabled)
    {
        if (m_remainingTime <= 0 && enabled)
        {
            sendCodeButton.interactable = true;
        }
        else if (!enabled)
        {
            sendCodeButton.interactable = false;
        }
    }

    public void StartCooldown()
    {
        if (m_sendTime == null)
        {
            m_sendTime = DateTime.UtcNow;
            StartCoroutine(UpdateSendCodeButton(CoolDownTime));
        }
    }

    private IEnumerator UpdateSendCodeButton(float remainingTime)
    {
        m_remainingTime = remainingTime;
        sendCodeButton.interactable = false;
        while (m_remainingTime > 0)
        {
            sendCodeText.text = "ui_register_code_resend".Localize(m_remainingTime);
            yield return new WaitForSeconds(1);
            m_remainingTime -= 1;
        }

        sendCodeText.text = "ui_register_code_send".Localize();
        sendCodeButton.interactable = true;

        m_sendTime = null;
    }
}
