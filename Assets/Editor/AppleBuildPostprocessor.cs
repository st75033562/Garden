using cn.sharesdk.unity3d.sdkporter;
using LitJson;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using UnityEngine;

public static class AppleBuildPostprocessor
{
    [PostProcessBuild]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
    {
        if (target != BuildTarget.iOS &&
            target != BuildTarget.StandaloneOSXUniversal &&
            target != BuildTarget.StandaloneOSXIntel &&
            target != BuildTarget.StandaloneOSXIntel64)
        {
            return;
        }

        PatchInfoPlist(target, pathToBuiltProject);
        Fix1024IconWarning(target, pathToBuiltProject);
        AddLocalizations(target, pathToBuiltProject);
    }

    private static void PatchInfoPlist(BuildTarget target, string pathToBuiltProject)
    {
        string plistPath;
        if (target == BuildTarget.iOS)
        {
            plistPath = pathToBuiltProject + "/Info.plist";
        }
        else
        {
            plistPath = pathToBuiltProject + "/Contents/Info.plist";
        }
        PlistDocument plist = new PlistDocument();
        plist.ReadFromFile(plistPath);

        string plistPatch = target == BuildTarget.iOS ? "iOS.plist" : "MacOS.plist";
        PlistDocument patch = new PlistDocument();
        patch.ReadFromFile(Application.dataPath + "/Editor/Build/Plist/" + plistPatch);

        plist.Merge(patch);
        plist.WriteToFile(plistPath);

        if (target != BuildTarget.iOS)
        {
            string resourcesDir = pathToBuiltProject + "/Contents/Resources/";
            string[] langIds = { "en", "zh-CN" };
            foreach (var lang in langIds)
            {
                EditorUtils.Run("cp", "-R", string.Format("Assets/Editor/Build/{0}.lproj", lang), resourcesDir);
                // remove all metas
                foreach (var path in Directory.GetFiles(resourcesDir, "*.meta"))
                {
                    File.Delete(path);
                }
            }
        }
    }

    private static void Fix1024IconWarning(BuildTarget target, string pathToBuiltProject)
    {
        if (target != BuildTarget.iOS)
        {
            return;
        }

        var targetDir = pathToBuiltProject + "/Unity-iPhone/Images.xcassets/AppIcon.appiconset";
        var contentPath = targetDir + "/Contents.json";
        var jsonString = File.ReadAllText(contentPath);
        var content = JsonMapper.ToObject(jsonString);
        var images = content["images"];

        JsonData targetSetting = null;
        for (int i = 0; i < images.Count; ++i)
        {
            var imageSetting = images[i];
            if ((string)imageSetting["idiom"] == "ios-marketing")
            {
                targetSetting = imageSetting;
                break;
            }
        }

        // add the entry for 1024 icon
        if (targetSetting == null)
        {
            targetSetting = new JsonData();
            targetSetting["idiom"] = "ios-marketing";
            targetSetting["size"] = "1024x1024";
            targetSetting["scale"] = "1x";
            images.Add(targetSetting);
        }

        if (!targetSetting.HasKey("filename"))
        {
            var sourceIconPath = "Assets/UI/logo/new-logo-opaque-1024x1024.jpg";
            var targetIconPath = targetDir + "/" + Path.GetFileName(sourceIconPath);

            if (File.Exists(targetIconPath))
            {
                File.Delete(targetIconPath);
            }
            File.Copy(sourceIconPath, targetIconPath);
            targetSetting["filename"] = Path.GetFileName(sourceIconPath);

            Debug.Log("added 1024 icon");
            File.WriteAllText(contentPath, JsonMapper.ToJson(content));
        }
    }

    private static void AddLocalizations(BuildTarget target, string pathToBuiltProject)
    {
        if (target != BuildTarget.iOS)
        {
            return;
        }

        var proj = new XCProject(pathToBuiltProject);
        proj.ApplyMod(Application.dataPath + "/Editor/Build/project.projmods");
        proj.Save();
    }
}
