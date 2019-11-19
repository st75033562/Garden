using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PrefabVariant.Editor
{
    [CustomEditor(typeof(ObjectChangeCollection))]
    public class ObjectChangeCollectionEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var changes = (ObjectChangeCollection)target;

            foreach (var change in changes)
            {
                DrawChanges(change);
            }
        }

        void DrawChanges(ObjectChange objectChanges)
        {
            var deletedChanges = new List<PropertyChange>();
            foreach (var change in objectChanges)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(change.path);
                if (GUILayout.Button("Remove"))
                {
                    deletedChanges.Add(change);
                }
                EditorGUILayout.EndHorizontal();
            }
            foreach (var change in deletedChanges)
            {
                objectChanges.Remove(change);
            }
        }
    }
}
