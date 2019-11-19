using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public abstract class SceneTransition : MonoBehaviour
{
    private Action<SceneTransition> m_done;
    private Direction m_direction;

    public enum Direction
    {
        In,
        Out,
    }

    public virtual void Begin(Direction d, Action<SceneTransition> done)
    {
        m_direction = d;
        m_done = done;
    }

    public Direction direction
    {
        get { return m_direction; }
    }

    public virtual void End()
    {
        if (m_done != null)
        {
            m_done(this);
        }
    }
}
