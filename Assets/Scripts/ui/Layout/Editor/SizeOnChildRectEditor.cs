using UnityEditor;
using UnityEditor.UI;

[CustomEditor(typeof(SizeOnChildRect), true)]
[CanEditMultipleObjects]
public class SizeOnChildRectEditor : Editor
{
    SerializedProperty m_HorizontalFit;
    SerializedProperty m_VerticalFit;
    SerializedProperty m_Padding;
    SerializedProperty m_MinSize;
    SerializedProperty m_MaxSize;
    SerializedProperty m_UsePreferredSize;

    protected virtual void OnEnable()
    {
        m_HorizontalFit = serializedObject.FindProperty("m_HorizontalFit");
        m_VerticalFit = serializedObject.FindProperty("m_VerticalFit");
        m_Padding = serializedObject.FindProperty("m_Padding");
        m_MinSize = serializedObject.FindProperty("m_MinSize");
        m_MaxSize = serializedObject.FindProperty("m_MaxSize");
        m_UsePreferredSize = serializedObject.FindProperty("m_UsePreferredSize");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(m_HorizontalFit, true);
        EditorGUILayout.PropertyField(m_VerticalFit, true);
        EditorGUILayout.PropertyField(m_Padding, true);
        EditorGUILayout.PropertyField(m_MinSize, true);
        EditorGUILayout.PropertyField(m_MaxSize, true);
        EditorGUILayout.PropertyField(m_UsePreferredSize, true);
        serializedObject.ApplyModifiedProperties();
    }
}
