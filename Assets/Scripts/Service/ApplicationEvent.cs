using System;
using System.Collections.Generic;
using UnityEngine;

public class ApplicationQuitEvent
{
    public void Accept()
    {
        ApplicationEvent.ContinueQuit();
    }

    public void Ignore()
    {
        ApplicationEvent.AbortQuit();
    }
}

public delegate void ApplicationQuitEventHandler(ApplicationQuitEvent evt);

public class ApplicationEvent : Singleton<ApplicationEvent>
{
    private static readonly A8.Logger s_logger = A8.Logger.GetLogger<ApplicationEvent>();

    public static event Action onResolutionChanged;

    public static event ApplicationQuitEventHandler onQuit
    {
        add
        {
            if (value == null)
            {
                return;
            }
            s_quitHandlers.AddFirst(value);
        }
        remove
        {
            if (s_curQuitHandler != null && s_curQuitHandler.Value == value)
            {
                s_curHandlerRemoved = true;
                return;
            }
            s_quitHandlers.Remove(value);
        }
    }

    private static int s_curScreenWidth;
    private static int s_curScreenHeight;

    private static readonly LinkedList<ApplicationQuitEventHandler> s_quitHandlers = new LinkedList<ApplicationQuitEventHandler>();
    private static LinkedListNode<ApplicationQuitEventHandler> s_curQuitHandler;
    private static bool s_curHandlerRemoved;
    private static readonly ApplicationQuitEvent s_quitEvent = new ApplicationQuitEvent();

    void Awake()
    {
        s_curScreenWidth = Screen.width;
        s_curScreenHeight = Screen.height;
    }

    void Update()
    {
        if (Screen.width != s_curScreenWidth || Screen.height != s_curScreenHeight)
        {
            s_curScreenWidth = Screen.width;
            s_curScreenHeight = Screen.height;

            if (onResolutionChanged != null)
            {
                onResolutionChanged();
            }
        }
    }

    public static void ContinueQuit()
    {
        s_logger.Log("continue quit");

        if (s_curQuitHandler == null)
        {
            throw new InvalidOperationException();
        }

        if (s_curQuitHandler.Next != null)
        {
            var nextHandler = s_curQuitHandler.Next;
            if (s_curHandlerRemoved)
            {
                s_quitHandlers.Remove(s_curQuitHandler);
                s_curHandlerRemoved = false;
            }
            s_curQuitHandler = nextHandler;
            s_curQuitHandler.Value(s_quitEvent);
        }
        else
        {
            s_logger.Log("quit");

            s_curQuitHandler = null;
            s_curHandlerRemoved = false;
            isQuitting = true;

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }

    public static void AbortQuit()
    {
        s_logger.Log("abort quit");

        s_curQuitHandler = null;
        s_curHandlerRemoved = false;
    }

    [ContextMenu("Simulate Quit")]
    public void OnApplicationQuit()
    {
        if (isQuitting) { return; }

        if (s_curQuitHandler == null && s_quitHandlers.First != null)
        {
            s_logger.Log("handle quit event");

            Application.CancelQuit();

            s_curQuitHandler = s_quitHandlers.First;
            s_curQuitHandler.Value(s_quitEvent);
        }
        else if (s_curQuitHandler != null)
        {
            Application.CancelQuit();
        }
    }

    /// <summary>
    /// true only when the app is about to quit
    /// </summary>
    public static bool isQuitting
    {
        get;
        private set;
    }
}
