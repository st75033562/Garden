using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Gameboard.Editor
{
    public class ObjectActionConfigUIHelper
    {
        private static readonly string[] s_actionClassFullNames;
        private static readonly string[] s_actionClassSimpleNames;

        private static readonly Type[] s_configClasses;

        private readonly List<bool> m_configVisible = new List<bool>();

        static ObjectActionConfigUIHelper()
        {
            var classes = Assembly.GetAssembly(typeof(ObjectAction)).GetTypes()
                                        .Where(x => x.IsSubclassOf(typeof(ObjectAction)) && !x.IsAbstract)
                                        .ToArray();

            s_actionClassFullNames = new[] { "" }.Concat(classes.Select(x => x.FullName)).ToArray();
            s_actionClassSimpleNames = new[] { "<Invalid>" }.Concat(classes.Select(x => GetActionClassSimpleName(x))).ToArray();
            s_configClasses = new Type[] { null }.Concat(classes.Select(x => x.GetNestedType("Config"))).ToArray();
        }

        static string GetActionClassSimpleName(Type type)
        {
            var nameAttribute = type.FirstCustomAttribute<ObjectActionNameAttribute>(true);
            if (nameAttribute != null)
            {
                return nameAttribute.GetSimpleName(type);
            }
            return type.FullName;
        }

        public Action<int> onEntryDeleted { get; set; }

        public Action<int, int> onEntryMoved { get; set; }
        
        public void DrawEntry(SerializedProperty propEntries, int index)
        {
            var prop = propEntries.GetArrayElementAtIndex(index);

            var propClassName = prop.FindPropertyRelative("className");
            var propJsonConfig = prop.FindPropertyRelative("jsonConfig");

            int classIndex = -1;
            if (propClassName.stringValue != "")
            {
                classIndex = Array.IndexOf(s_actionClassFullNames, propClassName.stringValue);
            }
            if (classIndex == -1)
            {
                classIndex = 0; // 0 for invalid class
            }

            m_configVisible.Resize(index + 1);

            EditorGUILayout.BeginHorizontal();
            var foldoutName = "[" + index + "] " + s_actionClassSimpleNames[classIndex];
            m_configVisible[index] = EditorGUILayout.Foldout(m_configVisible[index], foldoutName);

            if (GUILayout.Button("-", GUILayout.Width(20)))
            {
                propEntries.DeleteArrayElementAtIndex(index);
                m_configVisible.RemoveAt(index);
                return;
            }

            GUI.enabled = index > 0;
            if (GUILayout.Button("^", GUILayout.Width(20)))
            {
                propEntries.MoveArrayElement(index, index - 1);
            }

            GUI.enabled = index < propEntries.arraySize - 1;
            if (GUILayout.Button("v", GUILayout.Width(20)))
            {
                propEntries.MoveArrayElement(index, index + 1);
            }
            EditorGUILayout.EndHorizontal();

            GUI.enabled = true;
            if (!m_configVisible[index])
            {
                return;
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Action", GUILayout.MaxWidth(70));
            var newClassIndex = EditorGUILayout.Popup(classIndex, s_actionClassSimpleNames);
            if (newClassIndex != classIndex)
            {
                if (newClassIndex != 0)
                {
                    propClassName.stringValue = s_actionClassFullNames[newClassIndex];
                }
                else
                {
                    propClassName.stringValue = "";
                }
                propJsonConfig.stringValue = "";
            }
            EditorGUILayout.EndHorizontal();

            if (classIndex < s_configClasses.Length)
            {
                var configType = s_configClasses[classIndex];
                if (configType != null)
                {
                    DrawConfig(configType, propJsonConfig);
                }
            }
        }

        void DrawConfig(Type configType, SerializedProperty propJsonConfig)
        {
            var config = ObjectActionConfigFactory.Deserialize(configType.DeclaringType, propJsonConfig.stringValue);
            if (config == null)
            {
                config = Activator.CreateInstance(configType);
            }

            EditorGUILayout.LabelField("Config");
            ++EditorGUI.indentLevel;

            EditorGUI.BeginChangeCheck();
            EditorGUIUtils.Object(config);
            if (EditorGUI.EndChangeCheck())
            {
                propJsonConfig.stringValue = ObjectActionConfigFactory.Serialize(config);
            }
            --EditorGUI.indentLevel;
        }
    }
}
