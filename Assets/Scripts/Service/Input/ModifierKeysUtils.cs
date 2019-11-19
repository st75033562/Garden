using System;
using System.Collections.Generic;
using UnityEngine;

public static class ModifierKeysUtils
{
    private static readonly Dictionary<KeyCode, ModifierKeys> s_modifierKeyMap = new Dictionary<KeyCode, ModifierKeys>();

    static ModifierKeysUtils()
    {
        s_modifierKeyMap[KeyCode.LeftControl] = ModifierKeys.Ctrl;
        s_modifierKeyMap[KeyCode.RightControl] = ModifierKeys.Ctrl;
        s_modifierKeyMap[KeyCode.LeftAlt] = ModifierKeys.Alt;
        s_modifierKeyMap[KeyCode.RightAlt] = ModifierKeys.Alt;
        s_modifierKeyMap[KeyCode.LeftShift] = ModifierKeys.Shift;
        s_modifierKeyMap[KeyCode.RightShift] = ModifierKeys.Shift;
        s_modifierKeyMap[KeyCode.LeftWindows] = ModifierKeys.Win;
        s_modifierKeyMap[KeyCode.RightWindows] = ModifierKeys.Win;
        s_modifierKeyMap[KeyCode.LeftCommand] = ModifierKeys.Command;
        s_modifierKeyMap[KeyCode.RightCommand] = ModifierKeys.Command;
    }

    public static IDictionary<KeyCode, ModifierKeys> keyMap
    {
        get { return s_modifierKeyMap; }
    }

    public static ModifierKeys GetModifierKeys(KeyCode key)
    {
        ModifierKeys modifierKeys;
        s_modifierKeyMap.TryGetValue(key, out modifierKeys);
        return modifierKeys;
    }

    public static string GetDisplayName(ModifierKeys keys)
    {
        var names = Enum.GetNames(typeof(ModifierKeys)) ;
        var values = (ModifierKeys[])Enum.GetValues(typeof(ModifierKeys));
        string keyName = "";
        for (int i = 1; i < values.Length; ++i)
        {
            if ((values[i] & keys) != 0)
            {
                if (keyName != "")
                {
                    keyName += "+";
                }
                keyName += names[i];
            }
        }
        return keyName;
    }
}
