using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using AssetBundles;
using System;
using System.Linq;

namespace AssetBundles
{
    [CustomEditor(typeof(AssetBundleManager))]
    public class AssetBundleManagerEditor : Editor
    {
        private GUIStyle m_refCountStyle;
        private bool m_invertedDependencies;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (m_refCountStyle == null)
            {
                m_refCountStyle = new GUIStyle(GUI.skin.label);
                m_refCountStyle.alignment = TextAnchor.MiddleLeft;
            }

            EditorGUILayout.BeginVertical();
            DrawBundleGUI("Loaded", AssetBundleManager.LoadedAssetBundles);
            DrawBundleGUI("Downloading", AssetBundleManager.DownloadingAssetBundles);
            DrawDependencies();
            DrawOperations();
            EditorGUILayout.EndVertical();

            Repaint();
        }

        void DrawBundleGUI(string name, Dictionary<string, LoadedAssetBundle> bundles)
        {
            EditorGUILayout.LabelField(name);
            ++EditorGUI.indentLevel;
            foreach (var bundle in bundles)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(bundle.Key);
                EditorGUILayout.LabelField(bundle.Value.m_ReferencedCount.ToString(), m_refCountStyle, GUILayout.Width(50));
                EditorGUILayout.EndHorizontal();
            }
            --EditorGUI.indentLevel;
        }

        void DrawDependencies()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Dependencies");
            GUILayout.FlexibleSpace();
            m_invertedDependencies = EditorGUILayout.Toggle("Inverted", m_invertedDependencies);
            EditorGUILayout.EndHorizontal();

            if (m_invertedDependencies)
            {
                DrawDependents();
            }
            else
            {
                DrawDependenciesUninverted();
            }
        }

        void DrawDependenciesUninverted()
        {
            foreach (var bundle in AssetBundleManager.Dependencies.Keys)
            {
                ++EditorGUI.indentLevel;
                DrawHierarchies(bundle, key => {
                    string[] dependencies;
                    AssetBundleManager.Dependencies.TryGetValue(key, out dependencies);
                    return dependencies ?? Enumerable.Empty<string>();
                });
                --EditorGUI.indentLevel;
            }
        }

        void DrawHierarchies(string root, Func<string, IEnumerable<string>> childGetter)
        {
            EditorGUILayout.LabelField(root);
            ++EditorGUI.indentLevel;
            var children = childGetter(root);
            foreach (var child in children)
            {
                DrawHierarchies(child, childGetter);
            }
            --EditorGUI.indentLevel;
        }

        void DrawDependents()
        {
            // calculate dependents from dependencies
            var dependentsGraph = new Dictionary<string, List<string>>();

            foreach (var depKV in AssetBundleManager.Dependencies)
            {
                foreach (var dep in depKV.Value)
                {
                    List<string> dependents;
                    if (!dependentsGraph.TryGetValue(dep, out dependents))
                    {
                        dependents = new List<string>();
                        dependentsGraph.Add(dep, dependents);
                    }
                    dependents.Add(depKV.Key);
                }
            }

            foreach (var bundle in dependentsGraph.Keys)
            {
                ++EditorGUI.indentLevel;
                DrawHierarchies(bundle, key => {
                    List<string> dependents;
                    dependentsGraph.TryGetValue(key, out dependents);
                    return dependents ?? Enumerable.Empty<string>();
                });
                --EditorGUI.indentLevel;
            }
        }

        private void DrawOperations()
        {
            EditorGUILayout.LabelField("Operations");
            ++EditorGUI.indentLevel;
            foreach (var op in AssetBundleManager.InProgressOperations)
            {
                EditorGUILayout.LabelField(op.ToString());
            }
            --EditorGUI.indentLevel;
        }
    }

}