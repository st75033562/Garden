using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIInputContext : MonoBehaviour, IInputListener
{
    [SerializeField]
    private int m_priority = -1;

    public bool m_propagate = false;
    public bool m_enableInputWhenCanvasIsOn = true;

    [SerializeField]
    private Canvas m_canvas;

    private static readonly List<UIInputContext> s_pendingContexts = new List<UIInputContext>();

    protected void Awake()
    {
        if (!m_canvas)
        {
            m_canvas = GetComponentInParent<Canvas>();
        }
    }

    public bool isEnabled
    {
        get
        {
            if (!enabled)
            {
                return false;
            }

            if (m_enableInputWhenCanvasIsOn)
            {
                return m_canvas && m_canvas.enabled;
            }

            return true;
        }
    }

    protected virtual void OnEnable()
    {
        s_pendingContexts.Add(this);
    }

    protected virtual void OnDisable()
    {
        if (!s_pendingContexts.Remove(this))
        {
            InputListenerManager.instance.Pop(this);
        }
    }

    public static void RegisterPendingContexts()
    {
        if (s_pendingContexts.Count > 0)
        {
            foreach (var context in s_pendingContexts)
            {
                if (context)
                {
                    var priority = InputUtils.GetUIPriority(context.transform, context.m_canvas, context.m_priority);
                    InputListenerManager.instance.Push(context, priority);
                }
            }
            s_pendingContexts.Clear();
        }
    }

    public bool OnKey(KeyEventArgs eventArgs)
    {
        if (!isEnabled)
        {
            return false;
        }

        if (OnKeyImpl(eventArgs))
        {
            return true;
        }

        return !m_propagate;
    }
    
    protected virtual bool OnKeyImpl(KeyEventArgs eventArgs)
    {
        return false;
    }

#if UNITY_EDITOR
    protected void Reset()
    {
        m_canvas = GetComponentInParent<Canvas>();
    }
#endif
}
