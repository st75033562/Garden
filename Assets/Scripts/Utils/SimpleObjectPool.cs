using System;
using System.Collections.Generic;

public class SimpleObjectPool<T>
{
    private readonly Func<T> m_factory;
    private readonly Action<T> m_reset;
    private readonly List<T> m_objects = new List<T>();
    private int m_usedObjects;

    public SimpleObjectPool(Func<T> factory, Action<T> reset)
    {
        if (factory == null)
        {
            throw new ArgumentNullException("factory");
        }
        m_factory = factory;
        m_reset = reset;
    }

    public T Allocate()
    {
        if (m_usedObjects == m_objects.Count)
        {
            m_objects.Add(m_factory());
        }
        return m_objects[m_usedObjects++];
    }

    public void ReleaseAll()
    {
        m_usedObjects = 0;
        if (m_reset != null)
        {
            foreach (var obj in m_objects)
            {
                m_reset(obj);
            }
        }
    }
}
