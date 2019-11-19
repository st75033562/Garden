using UnityEditor;
using UnityEngine;

public static class EditorGUIUtils
{
    public static void Object(object o)
    {
        foreach (var field in o.GetType().GetFields())
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(field.Name);

            EditorGUI.BeginChangeCheck();

            var value = field.GetValue(o);
            // for simplicity, only support int, string, float
            if (field.FieldType == typeof(string))
            {
                value = EditorGUILayout.TextField((string)value);
            }
            else if (field.FieldType == typeof(float))
            {
                value = EditorGUILayout.FloatField((float)value);
            }
            else if (field.FieldType == typeof(int))
            {
                value = EditorGUILayout.IntField((int)value);
            }
            else if (field.FieldType == typeof(bool))
            {
                value = EditorGUILayout.Toggle((bool)value);
            }

            if (EditorGUI.EndChangeCheck())
            {
                field.SetValue(o, value);
            }

            var fieldValue = field.FirstCustomAttribute<FieldValueAttribute>(false);
            if (fieldValue != null)
            {
                bool enabled = GUI.enabled;
                GUI.enabled = !fieldValue.IsDefaultValue(value);
                if (GUILayout.Button("Reset"))
                {
                    field.SetValue(o, fieldValue.defaultValue);
                }
                GUI.enabled = enabled;
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}
