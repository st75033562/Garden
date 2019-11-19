using System;
using UnityEngine;

public class CVarFloat : CVar
{
    private float m_value;
    private float m_defaultValue;

    public CVarFloat(string name, float value = 0, float min = float.MinValue, float max = float.MaxValue)
        : base(name)
    {
        if (min > max)
        {
            throw new ArgumentException("min > max");
        }
        if (value < min || value > max)
        {
            throw new ArgumentOutOfRangeException("value");
        }

        m_defaultValue = value;
        minValue = min;
        maxValue = max;
        floatValue = value;
    }

    public float floatValue
    {
        get { return m_value; }
        set
        {
            value = Mathf.Clamp(value, minValue, maxValue);
            if (m_value != value)
            {
                m_value = value;
                FireChanged();
            }
        }
    }

    public override string stringValue
    {
        get
        {
            return m_value.ToString();
        }
        set
        {
            floatValue = float.Parse(value);
        }
    }

    public float minValue
    {
        get;
        private set;
    }

    public float maxValue
    {
        get;
        private set;
    }

    public override void Reset()
    {
        floatValue = m_defaultValue;
    }

    public static implicit operator float(CVarFloat variable)
    {
        return variable.floatValue;
    }
}

