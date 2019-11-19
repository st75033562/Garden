using System;
using UnityEngine;

public class KeyMenuPlugins : DownMenuPlugins
{
    public KeyCode keyCode
    {
        get
        {
            var key = GetMenuValue().Substring("key_pressed_".Length);
            try
            {
                return (KeyCode)Enum.Parse(typeof(KeyCode), key, true);
            }
            catch (ArgumentException)
            {
                return KeyCode.None;
            }
        }
    }
}
