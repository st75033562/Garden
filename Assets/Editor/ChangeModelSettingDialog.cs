using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;

public class ChangeModelSettingDialog : EditorWindow
{
    private float m_scale = 1.0f;

    void OnGUI()
    {
        EditorGUILayout.BeginVertical();
        m_scale = EditorGUILayout.FloatField("Scale", m_scale);
        if (GUILayout.Button("Apply"))
        {
            var assets = Selection.GetFiltered<UnityEngine.Object>(SelectionMode.Assets | SelectionMode.TopLevel);
            foreach (var asset in assets)
            {
                var importer = ModelImporter.GetAtPath(AssetDatabase.GetAssetPath(asset)) as ModelImporter;
                if (!importer)
                {
                    continue;
                }

                importer.globalScale = m_scale;
                importer.SaveAndReimport();
            }
        }
        EditorGUILayout.EndVertical();
    }

    [MenuItem("Tools/Change Model Setting...")]
    public static void Open()
    {
        EditorWindow.GetWindow<ChangeModelSettingDialog>();
    }
}
