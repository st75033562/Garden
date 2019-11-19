using UnityEngine;
using System.Collections;

public class BaseExponentialFilter
{
    private float m_smoothFactor = 0.8f;

    public float smoothFactor
    {
        get { return m_smoothFactor; }
        set { m_smoothFactor = Mathf.Clamp01(value); }
    }
}

public class ExponentialFilter : BaseExponentialFilter
{
    public void addSample(float sample)
    {
        value += smoothFactor * (sample - value);
    }

    public float value
    {
        get;
        set;
    }
}

