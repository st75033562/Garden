using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UISwitchButton))]
public class UISwitchButtonColor : MonoBehaviour
{
    public Color m_highlightColor;
    public Color m_normalColor;

    public Graphic[] m_optionGraphis;

    private UISwitchButton m_switchButton;

    // Use this for initialization
    void Awake()
    {
        m_switchButton = GetComponent<UISwitchButton>();
        m_switchButton.onAnimationBegin.AddListener(OnAnimationBegin);
        m_switchButton.onAnimationEnd.AddListener(OnAnimationEnd);
    }

    private void OnAnimationBegin()
    {
        for (int i = 0; i < m_optionGraphis.Length; ++i)
        {
            m_optionGraphis[i].DOColor(GetOptionColor(i), m_switchButton.animationTime);
        }
    }

    private void OnAnimationEnd()
    {
        for (int i = 0; i < m_optionGraphis.Length; ++i)
        {
            m_optionGraphis[i].color = GetOptionColor(i);
        }
    }

    private Color GetOptionColor(int i)
    {
        return i == m_switchButton.option ? m_highlightColor : m_normalColor;
    }
}
