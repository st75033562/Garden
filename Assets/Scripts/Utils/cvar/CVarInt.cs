using System;
using UnityEngine;

public class CVarInt : CVar
{
    private int m_value;
    private int m_defaultValue;

    public CVarInt(string name, int value = 0, int min = int.MinValue, int max = int.MaxValue)
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
        intValue = value;
    }

    public override string stringValue
    {
        get
        {
            return intValue.ToString();
        }
        set
        {
            intValue = int.Parse(value);
        }
    }

    public int intValue
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

    public int minValue
    {
        get;
        private set;
    }

    public int maxValue
    {
        get;
        private set;
    }

    public override void Reset()
    {
        intValue = m_defaultValue;
    }

    public static implicit operator int(CVarInt variable)
    {
        return variable.intValue;
    }
}