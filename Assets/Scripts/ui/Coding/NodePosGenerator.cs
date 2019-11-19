using System;
using UnityEngine;

public class NodePosGenerator
{
    private readonly Vector2 m_randomRange;
    private readonly IRandom m_random;

    public NodePosGenerator(Vector2 randomRange, IRandom random)
    {
        if (random == null)
        {
            throw new ArgumentNullException("random");
        }
        m_randomRange = randomRange;
        m_random = random;
    }

    /// <summary>
    /// generate a random local position in the panel
    /// </summary>
    public Vector2 Generate(CodePanel panel)
    {
        if (panel == null)
        {
            throw new ArgumentNullException("panel");
        }

        return panel.GetLocalPos(m_random.Range(m_randomRange), true);
    }

    public object state
    {
        get { return m_random.state; }
        set { m_random.state = value; }
    }
}
