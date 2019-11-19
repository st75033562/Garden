using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class SimpleToggle : MonoBehaviour
{
    public UnityEvent m_ToggleOn;

    protected Toggle m_toggle;

    private bool m_isOn;

    protected virtual void Awake()
    {
        m_toggle = GetComponent<Toggle>();
        // we'll set the target graphic's color manually
        m_toggle.transition = Selectable.Transition.None;
        m_toggle.onValueChanged.AddListener(OnToggleChanged);
        m_isOn = m_toggle.isOn;
    }

    protected virtual void Start()
    {
        OnToggleChanged(m_toggle.isOn);

        // reset renderer color settings in case they were modified by Toggle
        if (m_toggle.targetGraphic)
        {
            var renderer = m_toggle.targetGraphic.GetComponent<CanvasRenderer>();
            renderer.SetColor(Color.white);
            renderer.SetAlpha(1.0f);
        }
    }

    protected virtual void OnToggleChanged(bool isOn)
    {
        if (m_toggle.targetGraphic)
        {
            if (isOn)
            {
                m_toggle.targetGraphic.color = m_toggle.colors.highlightedColor;
            }
            else if (m_toggle.interactable)
            {
                m_toggle.targetGraphic.color = m_toggle.colors.normalColor;
            }
            else
            {
                m_toggle.targetGraphic.color = m_toggle.colors.disabledColor;
            }
        }

        if (isOn && !m_isOn)
        {
            m_isOn = true;
            if (m_ToggleOn != null)
            {
                m_ToggleOn.Invoke();
            }
        }
        else
        {
            m_isOn = isOn;
        }
    }
}