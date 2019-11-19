using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.UI;
using UnityEngine;

public class SimpleTextToggle : SimpleToggle
{
    public int m_onDeltaTextSize;

    private int m_offTextSize;

    protected override void Start()
    {
        m_offTextSize = (m_toggle.targetGraphic as Text).fontSize;
        base.Start();
    }

    protected override void OnToggleChanged(bool isOn)
    {
        base.OnToggleChanged(isOn);

        var text = m_toggle.targetGraphic as Text;
        text.fontSize = isOn ? (m_onDeltaTextSize + m_offTextSize) : m_offTextSize;
    }
}
