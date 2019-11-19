using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AssetBundles;

public static class AssetBundleBuildTool {

    private const string WinOutputPath = "../build/turtle_assetbundle/Windows/";
    private const string WinOutputPathTime = "../build/turtle_assetbundle/Android/";
    private const string IosOutputPathTime = "../build/turtle_assetbundle/iOS/";
    private const string OsxOutputPathTime = "../build/turtle_assetbundle/OSX/";

    [MenuItem("Tools/AssetBundleBuild/Windows")]
    public static void Windows()
    {
        Builder(WinOutputPath, BuildTarget.StandaloneWindows64);
    }

    [MenuItem("Tools/AssetBundleBuild/Android")]
    public static void Android()
    {
        Builder(WinOutputPathTime, BuildTarget.Android);
    }
    [MenuItem("Tools/AssetBundleBuild/Ios")]
    public static void Ios()
    {
        Builder(IosOutputPathTime, BuildTarget.iOS);
    }

    [MenuItem("Tools/AssetBundleBuild/Oxs")]
    public static void Oxs()
    {
        Builder(OsxOutputPathTime, BuildTarget.StandaloneOSXUniversal);
    }
    static void Builder(string outPath, BuildTarget target)
    {
        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

        BuildPipeline.BuildAssetBundles(outPath, BuildAssetBundleOptions.None, target);
    }

}
