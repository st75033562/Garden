using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[Serializable]
public class UnityKeyCommand
{
    [SerializeField]
    private ModifierKeys m_modifierKeys;

    [SerializeField]
    private KeyCode m_key;

    [SerializeField]
    private UnityEvent m_onTrigger = new UnityEvent();

    [SerializeField]
    private Button m_button;

    [SerializeField]
    private string m_action;

    [SerializeField]
    private ActionMappingAsset m_actionMapping;

    public ModifierKeys modifierKeys
    {
        get { return m_modifierKeys; }
        set { m_modifierKeys = value; }
    }

    public KeyCode key
    {
        get { return m_key; }
        set { m_key = value; }
    }

    public Button targetButton
    {
        get { return m_button; }
        set { m_button = value; }
    }

    public bool enabled
    {
        get { return enableStateCheck != null ? enableStateCheck() : true; }
    }

    public Func<bool> enableStateCheck
    {
        get;
        set;
    }

    public string action
    {
        get { return m_action; }
        set { m_action = value ?? string.Empty; }
    }

    public bool Match(ShortcutKey shortcut)
    {
        var myShortcut = new ShortcutKey(modifierKeys, key);
        if (myShortcut == shortcut)
        {
            return true;
        }

        if (!string.IsNullOrEmpty(m_action) && m_actionMapping)
        {
            return m_actionMapping.Match(m_action, shortcut);
        }

        return false;
    }

    public UnityEvent onTrigger
    {
        get { return m_onTrigger; }
    }

    public void Execute()
    {
        if (!enabled)
        {
            return;
        }

        if (onTrigger != null)
        {
            onTrigger.Invoke();
        }

        if (m_button != null && m_button.interactable && m_button.gameObject.activeInHierarchy)
        {
            m_button.onClick.Invoke();
        }
    }

    public override bool Equals(object o)
    {
        var rhs = o as UnityKeyCommand;
        if (rhs == null)
        {
            return false;
        }

        return rhs.key == key && rhs.modifierKeys == modifierKeys;
    }

    public override int GetHashCode()
    {
        return (int)key * 31 + (int)m_modifierKeys;
    }
}
