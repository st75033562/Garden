using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using System.IO;
using UnityEngine;

public class CustomAssetMenu
{
    [MenuItem("Assets/Create/Scriptable Object")]
    public static void CreateScriptableObject()
    {
        string path = EditorUtility.SaveFilePanelInProject("create scriptable object", "", "asset", "");
        if (path.Length > 0)
        {
            var script = Selection.activeObject as MonoScript;
            var instance = ScriptableObject.CreateInstance(script.GetClass());
            AssetDatabase.CreateAsset(instance, path);
        }
    }

    [MenuItem("Assets/Create/Scriptable Object", true)]
    public static bool ValidateCreateScriptableObject()
    {
        var script = Selection.activeObject as MonoScript;
        return script && script.GetClass().IsSubclassOf(typeof(ScriptableObject));
    }

    [MenuItem("Assets/Move To AssetBundle")]
    public static void MoveToAssetBundle()
    {
        var assets = Selection.GetFiltered<GameObject>(SelectionMode.Assets | SelectionMode.TopLevel);
        foreach (var asset in assets)
        {
            var bundlePath = AssetBundlePostprocessor.Root + asset.name;
            if (Directory.Exists(bundlePath))
            {
                var message = string.Format("There's already a bundle for asset {0}, change the name first", asset.name);
                EditorUtility.DisplayDialog("Duplicate bundle", message, "OK");
                break;
            }
            else
            {
                AssetDatabase.CreateFolder(AssetBundlePostprocessor.RootNoSlash, asset.name);
                var assetPath = AssetDatabase.GetAssetPath(asset);
                AssetDatabase.MoveAsset(assetPath, bundlePath + "/" + Path.GetFileName(assetPath));
            }
        }
    }
        
    [MenuItem("Assets/Exclude Plugins")]
    public static void ExcludePlugins()
    {
        foreach (var asset in Selection.objects)
        {
            var importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(asset)) as PluginImporter;
            if (importer != null)
            {
                importer.SetCompatibleWithAnyPlatform(false);
                importer.SaveAndReimport();
            }
        }
    }
}
