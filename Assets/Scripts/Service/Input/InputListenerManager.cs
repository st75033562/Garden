using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

[Flags]
public enum ModifierKeys
{
    None    = 0,
    Ctrl    = 1 << 0,
    Alt     = 1 << 1,
    Shift   = 1 << 2,
    Win     = 1 << 3,
    Command = 1 << 4,
}

public class KeyEventArgs
{
    public KeyCode key { get; internal set; }

    public bool isPressed { get; internal set; }

    public ModifierKeys modifierKeys { get; internal set; }
}

public interface IInputListener
{
    bool OnKey(KeyEventArgs eventArgs);
}

public class InputListenerManager : Singleton<InputListenerManager>, IEnumerable<IInputListener>
{
    private class InputEntry
    {
        public IInputListener context;
        public int priority;

        public InputEntry(IInputListener context, int priority)
        {
            this.context = context;
            this.priority = priority;
        }
    }

    private readonly List<InputEntry> m_entries = new List<InputEntry>();
    private ModifierKeys m_modifierKeys = ModifierKeys.None;
    private readonly KeyEventArgs m_keyEventArgs = new KeyEventArgs();
    private readonly Dictionary<KeyCode, IInputListener> m_keyDownHandlers = new Dictionary<KeyCode, IInputListener>();
    
    private struct KeyEvent
    {
        public KeyCode key;
        public bool isPressed;
    }

    private readonly List<KeyEvent> m_keyEvents = new List<KeyEvent>();
    private readonly int[] m_keyStates = new int[((int)KeyCode.JoystickButton0 + 31) / 32];

    public void Init()
    {
    }

    public void Push(IInputListener context, int priority = -1)
    {
#if UNITY_EDITOR
        Assert.IsTrue(m_entries.Find(x => x.context == context) == null);
#endif

        if (priority == -1)
        {
            if (m_entries.Count > 0)
            {
                var lastPrio = m_entries[m_entries.Count - 1].priority;
                if (lastPrio < int.MaxValue)
                {
                    priority = lastPrio + 1;
                }
                else
                {
                    priority = int.MaxValue;
                }
            }
            else
            {
                priority = 0;
            }
        }

        int index = m_entries.FindLastIndex(x => x.priority <= priority);
        m_entries.Insert(index == -1 ? 0 : index + 1, new InputEntry(context, priority));
    }

    public void Pop(IInputListener context)
    {
        int index = m_entries.FindIndex(x => x.context == context);
        if (index == -1)
        {
            return;
        }
        m_entries.RemoveAt(index);
        foreach (var kv in m_keyDownHandlers)
        {
            if (kv.Value == context)
            {
                m_keyDownHandlers.Remove(kv.Key);
                break;
            }
        }
    }

    public IEnumerator<IInputListener> GetEnumerator()
    {
        for (int i = m_entries.Count - 1; i >= 0; --i)
        {
            yield return m_entries[i].context;
        }
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    void Update()
    {
        UpdateKeyStates();
        DispatchEvents();
    }

    void DispatchEvents()
    {
        UpdateModifierKeys();

        m_keyEventArgs.modifierKeys = m_modifierKeys;
        foreach (var evt in m_keyEvents)
        {
            if (evt.isPressed)
            {
                HandleKeyDown(evt.key);
            }
            else
            {
                HandleKeyUp(evt.key);
            }
        }

        m_keyEvents.Clear();
    }

    void UpdateKeyStates()
    {
        for (var key = KeyCode.Backspace; key < KeyCode.JoystickButton0; ++key)
        {
            var isDown = Input.GetKey(key) ? 1 : 0;
            UpdateKeyState(key, isDown);
        }
    }

    void UpdateKeyState(KeyCode key, int isDown)
    {
        var stateIndex = (int)key / 32;
        var stateOffset = (int)key % 32;
        var keyState = m_keyStates[stateIndex];

        var changed = (((keyState >> stateOffset) & 1) ^ isDown) != 0;
        if (changed)
        {
            m_keyEvents.Add(new KeyEvent {
                key = key,
                isPressed = isDown != 0
            });

            keyState ^= 1 << stateOffset;
            m_keyStates[stateIndex] = keyState;
        }
    }

    void UpdateModifierKeys()
    {
        foreach (var evt in m_keyEvents)
        {
            var modKey = ModifierKeysUtils.GetModifierKeys(evt.key);
            if (modKey != ModifierKeys.None)
            {
                if (evt.isPressed)
                {
                    m_modifierKeys |= modKey;
                }
                else
                {
                    m_modifierKeys &= ~modKey;
                }
            }
        }
    }

    void HandleKeyDown(KeyCode key)
    {
        //Debug.Log(key + " down");

        m_keyEventArgs.key = key;
        m_keyEventArgs.isPressed = true;

        for (int i = m_entries.Count - 1; i >= 0; --i)
        {
            var handler = m_entries[i].context;
            if (handler.OnKey(m_keyEventArgs))
            {
                m_keyDownHandlers.Add(key, handler);
                break;
            }
        }
    }

    private void HandleKeyUp(KeyCode key)
    {
        //Debug.Log(key + " up");

        m_keyEventArgs.key = key;
        m_keyEventArgs.isPressed = false;

        IInputListener keyDownHandler;
        if (m_keyDownHandlers.TryGetValue(key, out keyDownHandler))
        {
            m_keyDownHandlers.Remove(key);
        }
        for (int i = m_entries.Count - 1; i >= 0; --i)
        {
            var handler = m_entries[i].context;
            if (handler.OnKey(m_keyEventArgs) && m_keyDownHandlers == null)
            {
                break;
            }

            if (handler == m_keyDownHandlers)
            {
                break;
            }
        }
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            ClearKeyPressStates();
        }
    }

    void ClearKeyPressStates()
    {
        for (var key = KeyCode.Backspace; key < KeyCode.JoystickButton0; ++key)
        {
            UpdateKeyState(key, 0);
        }
        DispatchEvents();
    }
}