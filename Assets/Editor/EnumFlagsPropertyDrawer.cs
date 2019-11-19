using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(EnumFlagsAttribute))]
public class EnumFlagsPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var displayName = EditorUtils.GetPropertyDisplayName(property.name);
        property.intValue = EditorGUI.MaskField(position, displayName, property.intValue, property.enumNames);
    }
}
