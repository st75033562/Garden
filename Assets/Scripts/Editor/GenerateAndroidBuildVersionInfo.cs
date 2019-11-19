using UnityEditor;
using UnityEditor.Callbacks;
using LitJson;
using UnityEngine;
using System.IO;

public static class GenerateAndroidBuildVersionInfo
{
    [PostProcessBuild]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
    {
        if (target != BuildTarget.Android)
        {
            return;
        }

        var versionInfo = new AndroidUpdateService.VersionInfo();
        versionInfo.apkPath = Path.GetFileName(pathToBuiltProject);
        versionInfo.hash = Md5.HashFile(pathToBuiltProject);
        versionInfo.version = Application.version;

        var versionFilePath = Path.Combine(Path.GetDirectoryName(pathToBuiltProject), "version.txt");
        File.WriteAllText(versionFilePath, JsonMapper.ToJson(versionInfo));
    }
}
