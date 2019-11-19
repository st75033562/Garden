//#define DEBUG_FOCUS

using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class UIFocus 
    : MonoBehaviour
    , ISubmitHandler
    , IPointerDownHandler
{

    [SerializeField]
    private GameObject m_defaultFocus;

    [SerializeField]
    private bool m_resetFocusOnEnable = true; // true if focus should be reset to m_defaultFocus

    private GameObject m_currentFocus;

    private static Stack<UIFocus> s_focusStack = new Stack<UIFocus>();

#if UNITY_EDITOR
    private static int s_instanceCount; // for debugging
#endif

    void Awake()
    {
#if !UNITY_STANDALONE && !UNITY_EDITOR
        Destroy(this);
#endif
#if UNITY_EDITOR
        ++s_instanceCount;
#endif
        m_currentFocus = m_defaultFocus;
    }

    void Start()
    {
        TryFocusCurrentObject();
    }

    void OnDestroy()
    {
#if UNITY_EDITOR
        if (--s_instanceCount == 0)
        {
            Debug.Assert(s_focusStack.Count == 0, "non empty focus stack");
            s_focusStack.Clear();
        }
#endif
    }

    void OnEnable()
    {
        if (s_focusStack.Count > 0 && EventSystem.current)
        {
            var topFocus = s_focusStack.Peek();
            // sync with the latest change
            topFocus.m_currentFocus = EventSystem.current.currentSelectedGameObject;

#if DEBUG_FOCUS
            Debug.LogFormat("updated active focus of {0} to {1}",
                topFocus.name, topFocus.m_currentFocus ? topFocus.m_currentFocus.name : string.Empty);
#endif
        }

        if (!s_focusStack.Contains(this))
        {
            s_focusStack.Push(this);
        }

        TryFocusCurrentObject();
    }

    void TryFocusCurrentObject()
    {
        if (!EventSystem.current)
        {
            return;
        }

        if (s_focusStack.Peek() == this && !EventSystem.current.alreadySelecting)
        {
            EventSystem.current.SetSelectedGameObject(ActiveFocus);

#if DEBUG_FOCUS
            var selected = EventSystem.current.currentInputModule;
            Debug.LogFormat("set active focus of {0} to {1}", 
                name, selected ? selected.name : string.Empty);
#endif
        }
    }

    GameObject ActiveFocus
    {
        get { return m_resetFocusOnEnable ? m_defaultFocus : m_currentFocus; }
    }

    void OnDisable()
    {
        if (s_focusStack.Count > 0)
        {
            if (s_focusStack.Peek() == this)
            {
#if DEBUG_FOCUS
                Debug.LogFormat("remove focus {0}", name);
#endif

                s_focusStack.Pop();
                if (s_focusStack.Count > 0)
                {
                    Debug.Assert(s_focusStack.Peek() != null, "invalid focus found in stack");

                    if (EventSystem.current)
                    {
                        EventSystem.current.SetSelectedGameObject(s_focusStack.Peek().ActiveFocus);
                    }

#if DEBUG_FOCUS
                    Debug.LogFormat("set active focus to {0}", s_focusStack.Peek().ActiveFocus.name);
#endif
                }
                else
                {
                    if (EventSystem.current)
                    {
                        EventSystem.current.SetSelectedGameObject(null);
                    }

#if DEBUG_FOCUS
                    Debug.LogFormat("clear active focus");
#endif
                }
            }
            else
            {
                // remove us in case we're destroyed out of order
                s_focusStack = new Stack<UIFocus>(s_focusStack.Where(x => x != this).Reverse());

#if DEBUG_FOCUS
                Debug.LogFormat("focus lost out of order {0}", name);
#endif
            }
        }
    }

    void Reset()
    {
        m_defaultFocus = m_currentFocus = gameObject;
        m_resetFocusOnEnable = true;
    }

    public void OnSubmit(BaseEventData eventData)
    {
        // send to the default target
        if (m_defaultFocus != gameObject)
        {
            ExecuteEvents.Execute(m_defaultFocus, eventData, ExecuteEvents.submitHandler);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // make sure we're selected so that we can receive OnSubmit
        EventSystem.current.SetSelectedGameObject(gameObject);
        m_currentFocus = gameObject;
    }
}
