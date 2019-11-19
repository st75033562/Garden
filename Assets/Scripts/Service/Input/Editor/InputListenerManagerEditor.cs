using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(InputListenerManager))]
public class InputListenerManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField("Contexts");

        ++EditorGUI.indentLevel;
        var manager = target as InputListenerManager;
        foreach (var context in manager)
        {
            EditorGUILayout.BeginHorizontal();
            if (context is MonoBehaviour)
            {
                var behaviour = context as MonoBehaviour;
                if (behaviour)
                {
                    EditorGUILayout.LabelField(behaviour.name);
                    EditorGUILayout.ObjectField(behaviour, typeof(MonoBehaviour), true);
                }
            }
            else
            {
                EditorGUILayout.LabelField("unknown");
            }
            EditorGUILayout.EndHorizontal();
        }
        --EditorGUI.indentLevel;

        EditorGUILayout.EndVertical();
    }
}
