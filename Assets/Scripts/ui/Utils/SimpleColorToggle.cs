using System;
using UnityEngine;
using UnityEngine.UI;

public class SimpleColorToggle : SimpleToggle
{
    [Serializable]
    public class Group
    {
        public Graphic[] targets;
        public Color onColor = Color.white;
        public Color offColor = Color.white;
    }

    public Group[] m_groups;

    protected override void OnToggleChanged(bool isOn)
    {
        base.OnToggleChanged(isOn);

        foreach (var g in m_groups)
        {
            SetColor(g.targets, isOn ? g.onColor : g.offColor);
        }
    }

    private void SetColor(Graphic[] targets, Color color)
    {
        foreach (var target in targets)
        {
            target.color = color;
        }
    }
}
