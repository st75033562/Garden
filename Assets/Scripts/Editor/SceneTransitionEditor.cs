using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SceneTransition), true)]
public class SceneTransitionEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Fade In"))
        {
            ((SceneTransition)target).Begin(SceneTransition.Direction.In, null);
        }
        if (GUILayout.Button("Fade Out"))
        {
            ((SceneTransition)target).Begin(SceneTransition.Direction.Out, null);
        }
        EditorGUILayout.EndHorizontal();
    }
}
