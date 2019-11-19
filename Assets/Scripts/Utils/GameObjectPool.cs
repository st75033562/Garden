using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class GameObjectPool : MonoBehaviour
{
    public GameObject template;

    private Stack<GameObject> m_freeObjects = new Stack<GameObject>();
    private List<GameObject> m_allocatedObjects = new List<GameObject>();

    /// <summary>
    /// optional init and de-init callbacks
    /// </summary>
    public Action<GameObject> onActivated { get; set; }

    public Action<GameObject> onDeactivated { get; set; }

    public GameObject Allocate()
    {
        GameObject go;
        if (m_freeObjects.Count == 0)
        {
            go = Instantiate(template);
        }
        else
        {
            go = m_freeObjects.Pop();
        }
        m_allocatedObjects.Add(go);
        go.SetActive(true);

        if (onActivated != null)
        {
            onActivated(go);
        }
        return go;
    }

    /// <summary>
    /// Equivalent to Allocate().GetComponent&lt;T&gt;()
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T Allocate<T>() where T : Component
    {
        return Allocate().GetComponent<T>();
    }

    public void DeallocateAll()
    {
        foreach (var go in m_allocatedObjects)
        {
            Deactivate(go);
            m_freeObjects.Push(go);
        }
        m_allocatedObjects.Clear();
    }

    private void Deactivate(GameObject go)
    {
        go.SetActive(false);
        if (onDeactivated != null)
        {
            onDeactivated(go);
        }
    }

    public void Deallocate(GameObject go)
    {
        Assert.IsTrue(m_allocatedObjects.Contains(go), "Object was not allocated by the pool");

        m_freeObjects.Push(go);
        m_allocatedObjects.Remove(go);
        Deactivate(go);
    }

    public void Shrink()
    {
        while (m_freeObjects.Count > 0)
        {
            var go = m_freeObjects.Pop();
            if (go)
            {
                Destroy(go);
            }
        }
    }

    void OnDestroy()
    {
        Shrink();

        foreach (var go in m_allocatedObjects)
        {
            if (go)
            {
                Destroy(go);
            }
        }
    }
}
