using System;
using UnityEditor;
using UnityEngine;

public class InputKeys
{
    public ModifierKeys modifierKeys;
    public KeyCode key;
}

public class KeyListenerWindow : EditorWindow
{
    public event Action<InputKeys> onEndInput;

    ModifierKeys m_modifierKeys;
    KeyCode m_key;

    private bool m_ctrl;
    private bool m_win;
    private bool m_command;

    public static KeyListenerWindow Open()
    {
        return GetWindowWithRect<KeyListenerWindow>(new Rect(0, 0, 200, 100));
    }

    void OnGUI()
    {
        switch (Event.current.type)
        {
        case EventType.KeyDown:
        {
            var modKey = ModifierKeysUtils.GetModifierKeys(Event.current.keyCode);
            if (modKey != ModifierKeys.None)
            {
                m_modifierKeys |= modKey;   
            }
            else
            {
                m_key = Event.current.keyCode;
                if (Event.current.shift)
                {
                    m_modifierKeys |= ModifierKeys.Shift;
                }

                if (m_ctrl)
                {
                    m_modifierKeys |= ModifierKeys.Ctrl;
                }
                if (m_win)
                {
                    m_modifierKeys |= ModifierKeys.Win;
                }
                if (m_command)
                {
                    m_modifierKeys |= ModifierKeys.Command;
                }

                Close();
                if (onEndInput != null)
                {
                    onEndInput(new InputKeys {
                        modifierKeys = m_modifierKeys,
                        key = m_key
                    });
                }
            }
            Event.current.Use();
        }
            break;

        case EventType.KeyUp:
        {
            var modKey = ModifierKeysUtils.GetModifierKeys(Event.current.keyCode);
            if (modKey != ModifierKeys.None)
            {
                m_modifierKeys &= ~modKey;
            }
            Event.current.Use();
        }
            break;
        }

        string keyName = ModifierKeysUtils.GetDisplayName(m_modifierKeys);

        EditorGUILayout.BeginHorizontal();
        m_ctrl = EditorGUILayout.ToggleLeft("Ctrl", m_ctrl, GUILayout.Width(50));
        m_win = EditorGUILayout.ToggleLeft("Win", m_win, GUILayout.Width(50));
        m_command = EditorGUILayout.ToggleLeft("Command", m_command, GUILayout.Width(70));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        EditorGUILayout.LabelField(keyName);
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
    }
}
