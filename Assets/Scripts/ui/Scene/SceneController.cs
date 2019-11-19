using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class SceneController : MonoBehaviour, IInputListener
{
    [Serializable]
    public class SceneEvent : UnityEvent<SceneController> { }

    [SerializeField]
    private SceneTransition[] m_transitions;
    private int m_pendingTransitions;

#if UNITY_EDITOR
    private bool m_initialized;
#endif

    protected virtual void Awake()
    {
        InputListenerManager.instance.Push(this, int.MinValue);
    }

    protected virtual void OnDestroy()
    {
        InputListenerManager.instance.Pop(this);
    }

    protected virtual void Start()
    {
        SceneDirector.Init();
#if UNITY_EDITOR
        if (!m_initialized)
        {
            Init(null, false);
        }
#endif
    }

    // isRestored is true if the scene is loaded from the scene stack
    public virtual void Init(object userData, bool isRestored)
    {
#if UNITY_EDITOR
        m_initialized = true;
#endif
    }

    public void BeginTransition(SceneTransition.Direction d)
    {
        for (int i = 0; i < m_transitions.Length; ++i)
        {
            m_transitions[i].Begin(d, OnTransitionEnd);
        }
        m_pendingTransitions = m_transitions.Length;

        if (m_pendingTransitions == 0)
        {
            OnTransitionEnd(null);
        }
    }

    private void OnTransitionEnd(SceneTransition t)
    {
		if (m_pendingTransitions > 0)
		{
			--m_pendingTransitions;
		}

        if (m_pendingTransitions == 0)
        {
            SceneDirector.OnSceneTransitionEnd(this);
            OnTransitionEnd();
        }
    }

    protected virtual void OnTransitionStart()
    {
    }

    protected virtual void OnTransitionEnd()
    {
    }

    public virtual void OnBackPressed()
    {
        SceneDirector.Pop();
    }

    /// <summary>
    /// get the save state which will be used when the scene is loaded when loaded from the scene stack
    /// </summary>
    public virtual object OnSaveState()
    {
        return null;
    }

    #region editor

    [ContextMenu("Setup")]
    protected void Setup()
    {
        m_transitions = GetComponentsInChildren<SceneTransition>();
    }

    #endregion

    public virtual bool OnKey(KeyEventArgs eventArgs)
    {
        if (eventArgs.isPressed && eventArgs.key == KeyCode.Escape)
        {
            OnBackPressed();
        }
        return false;
    }

    public string Name
    {
        get { return "scene controller"; }
    }
}
