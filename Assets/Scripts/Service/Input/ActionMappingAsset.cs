using System;
using UnityEngine;

[Serializable]
public struct ShortcutKey : IEquatable<ShortcutKey>
{
    public ModifierKeys modifierKeys;
    public KeyCode key;

    public ShortcutKey(ModifierKeys modifierKey, KeyCode key)
    {
        this.modifierKeys = modifierKey;
        this.key = key;
    }

    public bool Equals(ShortcutKey other)
    {
        return modifierKeys == other.modifierKeys &&
                key == other.key;
    }

    public override bool Equals(object obj)
    {
        if (obj == null)
        {
            return false;
        }

        if (!(obj is ShortcutKey))
        {
            return false;
        }

        return Equals((ShortcutKey)obj);
    }

    public override int GetHashCode()
    {
        return (int)modifierKeys * 31 + (int)key;
    }

    public static bool operator ==(ShortcutKey lhs, ShortcutKey rhs)
    {
        return lhs.Equals(rhs);
    }

    public static bool operator !=(ShortcutKey lhs, ShortcutKey rhs)
    {
        return !(lhs == rhs);
    }
}

public class ActionMappingAsset : ScriptableObject
{
    [Serializable]
    public class Action
    {
        public string name;
        public ShortcutKey[] shortcuts;

        public bool Has(ShortcutKey key)
        {
            return Array.FindIndex(shortcuts, x => x == key) != -1;
        }
    }

    [SerializeField]
    private Action[] m_actions;

    public bool Match(string actionName, ShortcutKey key)
    {
        var action = Array.Find(m_actions, x => x.name == actionName);
        if (action != null)
        {
            return action.Has(key);
        }
        return false;
    }

    public string Map(ShortcutKey key)
    {
        foreach (var action in m_actions)
        {
            if (action.Has(key))
            {
                return action.name;
            }
        }

        return null;
    }
}
