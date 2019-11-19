using System;
using System.IO;
using UnityEditor;
using UnityEngine.AssetBundles;

public static class AssetBundleBuilder
{
    public static void Build()
    {
        var platform = Environment.GetEnvironmentVariable("BUNDLE_PLATFORM");
        BuildTargetGroup buildGroup;
        BuildTarget buildTarget;
        switch (platform)
        {
        case "Windows":
            buildTarget = BuildTarget.StandaloneWindows;
            buildGroup = BuildTargetGroup.Standalone;
            break;

        case "Android":
            buildTarget = BuildTarget.Android;
            buildGroup = BuildTargetGroup.Android;
            break;

        case "OSX": 
            buildTarget = BuildTarget.StandaloneOSXUniversal;
            buildGroup = BuildTargetGroup.Standalone;
            break;

        case "iOS":
            buildTarget = BuildTarget.iOS;
            buildGroup = BuildTargetGroup.iOS;
            break;

        default:
            throw new Exception("Invalid BUNDLE_PLATFORM: " + platform);
        }

        if (!BuildUtils.ValidateAsset())
        {
            throw new Exception("Failed to validate assets");
        }

        if (!EditorUserBuildSettings.SwitchActiveBuildTarget(buildGroup, buildTarget))
        {
            throw new Exception("Failed to switch build target");
        }

        var outputPath = "AssetBundles/" + platform;
        if (!Directory.Exists(outputPath))
        {
            Directory.CreateDirectory(outputPath);
        }
        BuildPipeline.BuildAssetBundles(
            outputPath, BuildAssetBundleOptions.DeterministicAssetBundle, buildTarget);
    }
}
