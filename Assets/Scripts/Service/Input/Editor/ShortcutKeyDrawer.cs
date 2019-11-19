using System;
using UnityEditor;
using UnityEngine;

public class ShortcutKeyGUI
{
    private readonly string m_modifierKeyName;
    private readonly string m_keyName;
    private InputKeys m_keys;
    private string m_propPath;

    public ShortcutKeyGUI(string modifierKeyName, string keyName, int height = 20)
    {
        m_modifierKeyName = modifierKeyName;
        m_keyName = keyName;
        this.height = height;
    }

    public void Draw(Rect position, SerializedProperty property, GUIContent label)
    {
        var modifierKeys = property.FindPropertyRelative(m_modifierKeyName);
        var key = property.FindPropertyRelative(m_keyName);

        var changeButtonRect = new Rect(position.x, position.y, 60, height);
        if (GUI.Button(changeButtonRect, "Change"))
        {
            var propPath = property.propertyPath;
            var window = KeyListenerWindow.Open();
            window.onEndInput += keys => {
                m_propPath = propPath;
                m_keys = keys;
            };
        }

        var clearButtonRect = new Rect(changeButtonRect.xMax + 5, position.y, 60, height);
        if (GUI.Button(clearButtonRect, "Reset"))
        {
            modifierKeys.intValue = 0;
            key.intValue = 0;
        }

        if (m_keys != null && m_propPath == property.propertyPath)
        {
            modifierKeys.intValue = (int)m_keys.modifierKeys;
            key.intValue = (int)m_keys.key;
            m_keys = null;
        }

        var keyName = ModifierKeysUtils.GetDisplayName((ModifierKeys)modifierKeys.intValue);
        if (keyName != "")
        {
            keyName += "+";
        }
        keyName += ((KeyCode)key.intValue).ToString();

        var keyLabelRect = new Rect(clearButtonRect.xMax + 5, position.y, 200, height);
        EditorGUI.LabelField(keyLabelRect, keyName);
    }

    public int height
    {
        get;
        set;
    }
}

[CustomPropertyDrawer(typeof(ShortcutKey))]
public class ShortcutKeyDrawer : PropertyDrawer
{
    private readonly ShortcutKeyGUI m_gui = new ShortcutKeyGUI("modifierKeys", "key");

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        m_gui.Draw(EditorGUI.IndentedRect(position), property, label);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return m_gui.height;
    }
}
