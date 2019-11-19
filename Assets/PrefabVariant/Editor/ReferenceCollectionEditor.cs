using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PrefabVariant.Editor
{
    [CustomEditor(typeof(ReferenceCollection))]
    public class ReferenceCollectionEditor : UnityEditor.Editor
    {
        private bool m_foldoutReferences = true;
        private bool m_foldoutRemoved = true;
        private Dictionary<long, Component> m_parentComponents;

        void OnEnable()
        {
            var refs = (ReferenceCollection)target;
            if (refs.parentObject)
            {
                m_parentComponents = refs.parentObject.GetComponentsInChildren<Component>(true)
                    .ToDictionary(x => SerializedObjectUtils.GetFileId(x));
            }
        }

        public override void OnInspectorGUI()
        {
            var refs = (ReferenceCollection)target;
            EditorGUILayout.ObjectField("Parent", refs.parentObject, typeof(GameObject), false);

            if (!refs.parentObject) { return; }

            DrawComponents(ref m_foldoutReferences, "References", m_parentComponents, refs.references.Select(x => x.Value));
            DrawRemovedComponents(ref m_foldoutRemoved, m_parentComponents, refs.removedComponents);
        }

        void DrawComponents(ref bool foldout, string header, Dictionary<long, Component> parentComps, IEnumerable<long> ids)
        {
            foldout = EditorGUILayout.Foldout(foldout, header);
            if (foldout)
            {
                foreach (var compId in ids)
                {
                    if (parentComps.ContainsKey(compId))
                    {
                        var parentComp = parentComps[compId];
                        EditorGUILayout.ObjectField(parentComp, parentComp.GetType(), false);
                    }
                }
            }
        }

        void DrawRemovedComponents(ref bool foldout, Dictionary<long, Component> parentComps, IEnumerable<long> ids)
        {
            foldout = EditorGUILayout.Foldout(foldout, "Removed");
            if (foldout)
            {
                var restoredComps = new List<long>();
                foreach (var compId in ids)
                {
                    if (parentComps.ContainsKey(compId))
                    {
                        var parentComp = parentComps[compId];
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.ObjectField(parentComp, parentComp.GetType(), false);
                        if (GUILayout.Button("Remove"))
                        {
                            restoredComps.Add(compId);
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                }

                if (restoredComps.Count > 0)
                {
                    foreach (var compId in restoredComps)
                    {
                        ((ReferenceCollection)target).ClearParentRemoved(compId);
                    }

                    EditorUtility.SetDirty(target);
                }
            }
        }
    }
}
