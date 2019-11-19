using System;
using UnityEditor;
using UnityEngine;

//[CustomPropertyDrawer(typeof(UnityKeyCommand))]
public class UnityKeyCommandDrawer : PropertyDrawer
{
    private readonly ShortcutKeyGUI m_shortcutGui = new ShortcutKeyGUI("m_modifierKeys", "m_key");
    private const int ButtonHeight = 30;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        m_shortcutGui.Draw(position, property, label);

        var button = property.FindPropertyRelative("m_button");
        var buttonRect = new Rect(position.x, position.y + m_shortcutGui.height, position.width, EditorGUI.GetPropertyHeight(button));
        EditorGUI.PropertyField(buttonRect, button);

        var triggerRect = new Rect(position.x, position.y + m_shortcutGui.height + ButtonHeight, position.width, position.height);
        EditorGUI.PropertyField(triggerRect, property.FindPropertyRelative("m_onTrigger"));
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        var height = EditorGUI.GetPropertyHeight(property.FindPropertyRelative("m_onTrigger"));
        height += m_shortcutGui.height + ButtonHeight;
        return height;
    }
}
