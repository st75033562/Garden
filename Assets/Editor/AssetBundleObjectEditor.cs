using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AssetBundleObject), true)]
public class AssetBundleObjectEditor : Editor
{
    private SerializedProperty m_bundleName;
    private SerializedProperty m_assetName;

    void OnEnable()
    {
        m_bundleName = serializedObject.FindProperty("m_bundleName");
        m_assetName = serializedObject.FindProperty("m_assetName");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        base.OnInspectorGUI();

        EditorGUILayout.BeginVertical();

        GUI.enabled = !string.IsNullOrEmpty(m_bundleName.stringValue) &&
                      !string.IsNullOrEmpty(m_assetName.stringValue) &&
                      Application.isPlaying;
        if (GUILayout.Button("Change Asset"))
        {
            var instance = target as AssetBundleObject;
            instance.SetAsset(m_bundleName.stringValue, m_assetName.stringValue);
        }

        EditorGUILayout.EndVertical();

        serializedObject.ApplyModifiedProperties();
    }
}
