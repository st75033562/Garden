using System;
using System.Collections.Generic;
using UnityEngine;

public static class KeyPressedBlockUtil
{
    private static readonly KeyCode[] s_keys;

    static KeyPressedBlockUtil()
    {
        var keys = new List<KeyCode>();
        foreach (int value in Enum.GetValues(typeof(KeyCode)))
        {
            var code = (KeyCode)value;
            if (code == KeyCode.Escape || code == KeyCode.LeftWindows || code == KeyCode.RightWindows ||
                code == KeyCode.LeftCommand || code == KeyCode.RightCommand)
            {
                continue;
            }
            if (code >= KeyCode.Mouse0 && code <= KeyCode.Mouse6)
            {
                continue;
            }
            if (code >= KeyCode.JoystickButton0)
            {
                break;
            }

            keys.Add(code);
        }

        s_keys = keys.ToArray();
    }

    public static KeyCode[] allKeys { get { return s_keys; } }
}
