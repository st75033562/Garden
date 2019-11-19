using System;
using System.Collections.Generic;
using UnityEngine;

public class UnityCommandManager : UIInputContext
{
    [SerializeField]
    private List<UnityKeyCommand> m_keyCommands = new List<UnityKeyCommand>();

    public void Add(UnityKeyCommand cmd)
    {
        if (cmd == null)
        {
            throw new ArgumentNullException("cmd");
        }

        m_keyCommands.Add(cmd);
    }

    public UnityKeyCommand Get(string action)
    {
        if (string.IsNullOrEmpty(action))
        {
            throw new ArgumentException("action");
        }

        return m_keyCommands.Find(x => x.action == action);
    }

    public bool Contains(UnityKeyCommand cmd)
    {
        return m_keyCommands.Contains(cmd);
    }

    protected override bool OnKeyImpl(KeyEventArgs eventArgs)
    {
        if (eventArgs.isPressed)
        {
            foreach (var cmd in m_keyCommands)
            {
                if (!cmd.enabled) { continue; }

                var shortcut = new ShortcutKey(eventArgs.modifierKeys, eventArgs.key);
                if (cmd.Match(shortcut))
                {
                    cmd.Execute();
                    return true;
                }
            }
        }
        return false;
    }
}
