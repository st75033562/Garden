using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompositeCoroutine : IEnumerator
{
    private readonly List<IEnumerator> m_subCoroutines = new List<IEnumerator>();
    private readonly MonoBehaviour m_service;
    private int m_finishedCount;
    private bool m_started;

    public CompositeCoroutine(MonoBehaviour service)
    {
        if (service == null)
        {
            throw new ArgumentNullException("service");
        }
        m_service = service;
    }

    public void Add(IEnumerator subCoroutine)
    {
        if (subCoroutine == null)
        {
            throw new ArgumentNullException("subCoroutine");
        }
        if (m_started)
        {
            throw new InvalidOperationException();
        }
        m_subCoroutines.Add(subCoroutine);
    }

    public object Current
    {
        get { return null; }
    }

    public bool MoveNext()
    {
        if (!m_started)
        {
            m_started = true;
            foreach (var co in m_subCoroutines)
            {
                m_service.StartCoroutine(Wrapper(co));
            }
        }
        return m_subCoroutines.Count != m_finishedCount;
    }

    private IEnumerator Wrapper(IEnumerator coroutine)
    {
        yield return CoroutineUtils.Run(coroutine);
        ++m_finishedCount;
    }

    public void Reset()
    {
        m_started = false;
        m_finishedCount = 0;
    }
}
