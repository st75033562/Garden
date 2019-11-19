using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class CopyComponentEditor : EditorWindow
{
    private Component m_source;
    private Component m_target;

    [MenuItem("Tools/Copy Component...")]
    public static void Open()
    {
        GetWindow<CopyComponentEditor>();
    }

    void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();
        m_source = RenderGroup("Source", m_source);
        m_target = RenderGroup("Target", m_target);
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Copy"))
        {
            const BindingFlags FieldFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var sourceFields = m_source.GetType().GetFields(FieldFlags);
            foreach (var sourceField in sourceFields)
            {
                if (!IsSerializable(sourceField))
                {
                    continue;
                }

                var targetField = m_target.GetType().GetField(sourceField.Name, FieldFlags);
                if (targetField == null)
                {
                    continue;
                }

                if (IsSerializable(targetField))
                {
                    targetField.SetValue(m_target, sourceField.GetValue(m_source));
                }
            }
        }
    }

    private bool IsSerializable(FieldInfo field)
    {
        if (field.IsPublic) { return true; }
        return field.GetCustomAttributes(false).Any(x => x is SerializeField);
    }

    Component RenderGroup(string name, Component selectedComp)
    {
        EditorGUILayout.BeginVertical();
        var go = Selection.activeGameObject;
        EditorGUILayout.LabelField(name);
        foreach (var comp in go.GetComponents<Component>())
        {
            EditorGUILayout.BeginHorizontal();
            bool selected = EditorGUILayout.Toggle(selectedComp == comp, GUILayout.Width(20));
            if (selected)
            {
                selectedComp = comp;
            }
            EditorGUILayout.ObjectField(comp, comp.GetType(), true);
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();
        return selectedComp;
    }
}
