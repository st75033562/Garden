using System;
using System.Collections.Generic;

public class MovingAverageFilter
{
    private readonly Queue<float> m_samples = new Queue<float>();
    private int m_windowSize;
    private float m_sum;

    public MovingAverageFilter(int windowSize)
    {
        this.windowSize = windowSize;
    }

    public int windowSize
    {
        get { return m_windowSize; }
        set
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            m_windowSize = value;
            TrimSamples();
        }
    }

    public void addSample(float value)
    {
        m_samples.Enqueue(value);
        m_sum += value;
        TrimSamples();
    }

    private void TrimSamples()
    {
        while (m_samples.Count > m_windowSize)
        {
            m_sum -= m_samples.Peek();
            m_samples.Dequeue();
        }
    }

    public float value
    {
        get { return m_samples.Count > 0 ? m_sum / m_samples.Count : 0; }
    }
}
