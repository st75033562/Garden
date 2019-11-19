using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

[CustomEditor(typeof(ContentSizeFitter), true)]
[CanEditMultipleObjects]
public class ContentSizeFitterEditor : SelfControllerEditor
{
    SerializedProperty m_HorizontalFit;
    SerializedProperty m_VerticalFit;
    SerializedProperty m_MinSize;
    SerializedProperty m_MaxSize;
    SerializedProperty m_Padding;

    protected virtual void OnEnable()
    {
        m_HorizontalFit = serializedObject.FindProperty("m_HorizontalFit");
        m_VerticalFit = serializedObject.FindProperty("m_VerticalFit");
        m_MinSize = serializedObject.FindProperty("m_MinSize");
        m_MaxSize = serializedObject.FindProperty("m_MaxSize");
        m_Padding = serializedObject.FindProperty("m_Padding");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(m_HorizontalFit, true);
        EditorGUILayout.PropertyField(m_VerticalFit, true);
        EditorGUILayout.PropertyField(m_MinSize, true);
        EditorGUILayout.PropertyField(m_MaxSize, true);
        EditorGUILayout.PropertyField(m_Padding, true);
        serializedObject.ApplyModifiedProperties();

        base.OnInspectorGUI();
    }
}
