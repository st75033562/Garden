using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[CustomEditor(typeof(ResourceFont))]
public class ResourceFontEditor : Editor
{
    private SerializedProperty m_fontName;
    private ResourceFont m_target;
    private string[] m_fonts;
    private int m_selectedFont;

    void OnEnable()
    {
        m_fontName = serializedObject.FindProperty("m_fontName");
        m_target = target as ResourceFont;

        List<string> fonts = new List<string>();
        foreach (var guid in AssetDatabase.FindAssets("t:Font", new[] { "Assets/Resources/Fonts" }))
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            fonts.Add(Path.GetFileNameWithoutExtension(path));
        }
        m_fonts = fonts.ToArray();

        m_selectedFont = Array.IndexOf(m_fonts, m_fontName.stringValue);
        if (m_selectedFont == -1)
        {
            m_selectedFont = 0;
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Font");
        m_selectedFont = EditorGUILayout.Popup(m_selectedFont, m_fonts);
        m_fontName.stringValue = m_fonts[m_selectedFont];
        m_target.fontName = m_fontName.stringValue;
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Save"))
        {
            m_target.GetComponent<Text>().font = null;
            var instanceRoot = PrefabUtility.FindRootGameObjectWithSameParentPrefab(m_target.gameObject);
            PrefabUtility.ReplacePrefab(instanceRoot, PrefabUtility.GetPrefabParent(m_target), ReplacePrefabOptions.ConnectToPrefab);
        }
        if (GUILayout.Button("Test Font"))
        {
            m_target.RefreshFont();
            EditorUtility.SetDirty(m_target.GetComponent<Text>());
        }
        EditorGUILayout.EndHorizontal();
        serializedObject.ApplyModifiedProperties();
    }
}
