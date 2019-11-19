using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Gameboard.Editor
{
    [CustomEditor(typeof(ObjectActionManager))]
    public class ObjectActionManagerEditor : UnityEditor.Editor
    {
        private class ConfigDrawer
        {
            private readonly ObjectActionConfigUIHelper m_uiHelper = new ObjectActionConfigUIHelper();
            private readonly List<string> m_actionArgs = new List<string>();
            private readonly SerializedProperty m_propConfig;
            private SerializedObject m_config;
            private bool m_configExpanded;

            private readonly ObjectActionManager m_target;

            public ConfigDrawer(ObjectActionManager target, SerializedProperty config)
            {
                if (target == null)
                {
                    throw new ArgumentNullException("target");
                }
                if (config == null)
                {
                    throw new ArgumentNullException("config");
                }

                m_target = target;
                m_propConfig = config;

                m_uiHelper.onEntryDeleted = OnConfigEntryDeleted;
                m_uiHelper.onEntryMoved = OnConfigEntryMoved;

                ResetActionArgs();
            }

            private void OnConfigEntryDeleted(int index)
            {
                m_actionArgs.RemoveAt(index);
            }

            private void OnConfigEntryMoved(int fromIndex, int toIndex)
            {
                m_actionArgs.Swap(fromIndex, toIndex);
            }

            private void ResetActionArgs()
            {
                m_actionArgs.Clear();

                var config = (ObjectActionConfig)m_propConfig.objectReferenceValue;
                if (config != null && config.m_entries != null)
                {
                    m_actionArgs.Resize(config.m_entries.Length, "");
                }
            }

            public SerializedProperty configProperty
            {
                get { return m_propConfig; }
            }

            public void Draw()
            {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(m_propConfig);
                if (EditorGUI.EndChangeCheck())
                {
                    ResetActionArgs();
                    m_config = null;
                }

                if (!m_propConfig.objectReferenceValue)
                {
                    return;
                }

                if (m_config == null)
                {
                    m_config = new SerializedObject(m_propConfig.objectReferenceValue);
                }

                EditorGUI.indentLevel++;
                m_config.Update();
                m_configExpanded = EditorGUILayout.Foldout(m_configExpanded, "Details");
                if (m_configExpanded)
                {
                    EditorGUI.indentLevel++;
                    var propEntries = m_config.FindProperty("m_entries");
                    for (int i = 0; i < propEntries.arraySize; ++i)
                    {
                        var className = propEntries.GetArrayElementAtIndex(i).FindPropertyRelative("className").stringValue;
                        m_uiHelper.DrawEntry(propEntries, i);

                        if (className != "")
                        {
                            EditorGUI.indentLevel++;
                            var parameterAttr = GetParameterAttribute(className);
                            EditorGUILayout.BeginHorizontal();
                            if (parameterAttr != null && parameterAttr.numArgs != 0)
                            {
                                m_actionArgs[i] = EditorGUILayout.TextField("Arguments:", m_actionArgs[i]);
                            }
                            else
                            {
                                GUILayout.FlexibleSpace();
                            }
                            if (GUILayout.Button("Execute", GUILayout.ExpandWidth(false)))
                            {
                                m_target.Execute(i, m_actionArgs[i] != "" ? m_actionArgs[i].Split(',') : new string[0]);
                            }
                            EditorGUILayout.EndHorizontal();
                            EditorGUI.indentLevel--;
                        }

                        EditorGUILayout.Space();
                    }

                    if (GUILayout.Button("Add"))
                    {
                        propEntries.InsertArrayElementAtIndex(propEntries.arraySize);
                        m_actionArgs.Add("");
                    }
                    EditorGUI.indentLevel--;
                }
                EditorGUI.indentLevel--;

                m_config.ApplyModifiedProperties();
            }

            private static ObjectActionParameterAttribute GetParameterAttribute(string actionClassName)
            {
                return typeof(ObjectAction).Assembly
                                           .GetType(actionClassName)
                                           .FirstCustomAttribute<ObjectActionParameterAttribute>(false);
            }
        }

        private ConfigDrawer m_configDrawer;

        void OnEnable()
        {
            m_configDrawer = new ConfigDrawer((ObjectActionManager)target, serializedObject.FindProperty("m_config"));
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            m_configDrawer.Draw();

            if (!m_configDrawer.configProperty.objectReferenceValue)
            {
                if (GUILayout.Button("New Config"))
                {
                    string savePath;
                    var path = AssetDatabase.GetAssetPath(target);
                    if (!string.IsNullOrEmpty(path))
                    {
                        savePath = Path.GetDirectoryName(path) + "/" + Path.GetFileNameWithoutExtension(path) + "_action.asset";
                        savePath = AssetDatabase.GenerateUniqueAssetPath(savePath);
                    }
                    else
                    {
                        // not a prefab
                        savePath = EditorUtility.SaveFilePanelInProject("New Config", "New Config", "asset", "");
                    }

                    if (savePath != "")
                    {
                        var instance = ScriptableObject.CreateInstance<ObjectActionConfig>();
                        AssetDatabase.CreateAsset(instance, savePath);
                        m_configDrawer.configProperty.objectReferenceValue = 
                            AssetDatabase.LoadAssetAtPath<ScriptableObject>(savePath);
                    }
                }
            }

            // find duplicate actions. for now, we don't support multiple instances of same action type.
            var dupActions = ((MonoBehaviour)target).GetComponents<ObjectAction>()
                                                 .GroupBy(x => x.GetType())
                                                 .Where(x => x.Count() > 1)
                                                 .Select(x => x.Key)
                                                 .ToArray();
            if (dupActions.Length > 0)
            {
                var message = "Duplicate actions:\n";
                message += string.Join("\n", dupActions.Select(x => "    " + x.Name).ToArray());

                EditorGUILayout.HelpBox(message, MessageType.Error);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
