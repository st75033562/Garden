using UnityEditor;
using UnityEngine;

namespace Gameboard.Editor
{
    [CustomEditor(typeof(ObjectActionConfig))]
    public class ObjectActionConfigEditor : UnityEditor.Editor
    {
        private readonly ObjectActionConfigUIHelper m_uiHelper = new ObjectActionConfigUIHelper();
        private SerializedProperty m_propEntries;

        void OnEnable()
        {
            m_propEntries = serializedObject.FindProperty("m_entries");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            for (int i = 0; i < m_propEntries.arraySize; ++i)
            {
                m_uiHelper.DrawEntry(m_propEntries, i);
                EditorGUILayout.Space();
            }

            if (GUILayout.Button("Add"))
            {
                m_propEntries.InsertArrayElementAtIndex(m_propEntries.arraySize);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
