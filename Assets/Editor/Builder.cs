using LitJson;
using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using Debug = UnityEngine.Debug;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Checksums;

public static class Builder
{
    private const string WinOutputPath = "build/win/Pocketurtle.exe";
    private const string WinOutputPathTime = "../build/test/turtle/win-{time}/Pocketurtle.exe";
    private const string OSXOutputPath = "build/turtle/Pocketurtle.app";
    private const string AndroidOutputPath = "../build/publish/turtle/android/Pocketurtle-{version}.apk";
    private const string AndroidOutputPathTime = "../build/test/turtle/android-{time}/Pocketurtle-{version}.apk";
    private const string OSXOutputPathTime = "../build/turtle/osx-{time}/Pocketurtle.app";
    private const string IOSOutputPath = "build/iOS";

    private enum Platform
    {
        Windows,
        OSX,
        Android,
        iOS
    }

    private enum Config
    {
        Test,
        Production
    }

    [MenuItem("Tools/Build/Windows/Production")]
    public static void BuildWindows()
    {
        Build(Platform.Windows, Config.Production, WinOutputPath);
    }

    [MenuItem("Tools/Build/Windows/Time")]
    public static void BuildWindowsTime() {
        Build(Platform.Windows, Config.Test, WinOutputPathTime, compress:true, open:false);
    }

    [MenuItem("Tools/Build/OSX/Production")]
    public static void BuildOSX()
    {
        Build(Platform.OSX, Config.Production, OSXOutputPath);
    }

    [MenuItem("Tools/Build/OSX/Test")]
    public static void BuildOSXTest()
    {
        Build(Platform.OSX, Config.Test, OSXOutputPath);
    }

    [MenuItem("Tools/Build/OSX/Time")]
    public static void BuildOSXTime()
    {
        Build(Platform.OSX, Config.Test, OSXOutputPathTime, compress:true, open: false);
    }

    [MenuItem("Tools/Build/OSX/App Store")]
    public static void BuildOSXAppStorePkg()
    {
        Build(Platform.OSX, Config.Production, OSXOutputPath);
        EditorUtils.Run("Assets/Editor/Build/sign_mac_app.sh", OSXOutputPath);
    }

    [MenuItem("Tools/Build/Android/Production")]
    public static void BuildAndroid()
    {
        Build(Platform.Android, Config.Production, AndroidOutputPath);
    }

    [MenuItem("Tools/Build/Android/Time")]
    public static void BuildAndroidTime() {
        Build(Platform.Android, Config.Test, AndroidOutputPathTime, open:false);
    }

    [MenuItem("Tools/Build/iOS/Production")]
    public static void BuildiOS()
    {
        Build(Platform.iOS, Config.Production, IOSOutputPath);
    }

    [MenuItem("Tools/Build/iOS/Test")]
    public static void BuildiOSTest()
    {
        Build(Platform.iOS, Config.Test, IOSOutputPath);
    }

    static void CreateZip(string sourceFilePath, string destinationZipFilePath) {
        if(sourceFilePath[sourceFilePath.Length - 1] != System.IO.Path.DirectorySeparatorChar)
            sourceFilePath += System.IO.Path.DirectorySeparatorChar;
        ZipOutputStream zipStream = new ZipOutputStream(File.Create(destinationZipFilePath));
        zipStream.SetLevel(0);  // 压缩级别 0-9
        CreateZipFiles(sourceFilePath, zipStream, sourceFilePath.Length);
        zipStream.Finish();
        zipStream.Close();
    }

    /// 递归压缩文件
    static void CreateZipFiles(string sourceFilePath, ZipOutputStream zipStream, int subIndex) {
        Crc32 crc = new Crc32();
        string[] filesArray = Directory.GetFileSystemEntries(sourceFilePath);
        foreach(string file in filesArray) {
            if(Directory.Exists(file))                     //如果当前是文件夹，递归
            {
                CreateZipFiles(file, zipStream, subIndex);
            } else                                            //如果是文件，开始压缩
              {
                FileStream fileStream = File.OpenRead(file);
                byte[] buffer = new byte[fileStream.Length];
                fileStream.Read(buffer, 0, buffer.Length);
                string tempFile = file.Substring(subIndex);
                ZipEntry entry = new ZipEntry(tempFile);
                entry.DateTime = DateTime.Now;
                entry.Size = fileStream.Length;
                fileStream.Close();
                crc.Reset();
                crc.Update(buffer);
                entry.Crc = crc.Value;
                zipStream.PutNextEntry(entry);
                zipStream.Write(buffer, 0, buffer.Length);
            }
        }
    }

    private static void Build(Platform platform, Config config, string outputPath, bool compress = false, bool open = false)
    {
        if (platform == Platform.Android)
        {
            AndroidSigning.Setup();
        }

        outputPath = outputPath.Replace("{version}", PlayerSettings.bundleVersion);

        outputPath = outputPath.Replace("{time}", DateTime.Now.ToString("yy-MM-dd-HH-mm"));

        var parentDir = Path.GetDirectoryName(outputPath);
        // make a sub folder
        if (!Directory.Exists(parentDir))
        {
            Directory.CreateDirectory(parentDir);
        }

        const string BuildConfigPath = ("Assets/build_config.json");
        var jsonConfig = JsonMapper.ToObject(File.ReadAllText(BuildConfigPath));

        string configKey = config.ToString().ToLower();
        string platformKey = platform.ToString();
        if (!jsonConfig.HasKey(platformKey) || !jsonConfig[platform.ToString()].HasKey(configKey))
        {
            platformKey = "default";
        }

        var defines = (string)jsonConfig[platformKey][configKey];
        PlayerSettings.SetScriptingDefineSymbolsForGroup(GetTargetGroup(platform), defines);

        var scenes = EditorBuildSettings.scenes.Where(x => x.enabled).ToArray();
        BuildOptions buildOpts = BuildOptions.None;

        var showBuiltPlayer = Environment.GetEnvironmentVariable("SHOW_BUILT_PLAYER");
        if (showBuiltPlayer != "0" && open)
        {
            buildOpts |= BuildOptions.ShowBuiltPlayer;
        }

        if (platform == Platform.iOS)
        {
            buildOpts |= BuildOptions.AcceptExternalModificationsToPlayer;
        }
        if (config == Config.Test)
        {
            if (EditorUserBuildSettings.development)
            {
                buildOpts |= BuildOptions.Development;
            }
            if (EditorUserBuildSettings.connectProfiler)
            {
                buildOpts |= BuildOptions.ConnectWithProfiler;
            }
            if (EditorUserBuildSettings.allowDebugging)
            {
                buildOpts |= BuildOptions.AllowDebugging;
            }
        }

        if (!EditorUserBuildSettings.SwitchActiveBuildTarget(GetTargetGroup(platform), GetTarget(platform)))
        {
            throw new ApplicationException("Failed to switch to build target");
        }
        string error = BuildPipeline.BuildPlayer(scenes, outputPath, GetTarget(platform), buildOpts);
        if(!string.IsNullOrEmpty(error)) {
            Debug.LogError(error);
            throw new ApplicationException(error);
        } 

        if (!CopyExtenalTools(platform, outputPath))
        {
            throw new ApplicationException("Failed to copy external tool");
        }

        if (compress)
        {
            string compressSource = outputPath.Replace("..", "");
            compressSource = compressSource.Substring(0, compressSource.LastIndexOf('/'));
            string destinationName = compressSource.Substring(compressSource.LastIndexOf('/'));
            string rootPath = Application.dataPath.Substring(0, Application.dataPath.LastIndexOf('/'));
            rootPath = rootPath.Substring(0, rootPath.LastIndexOf('/'));

            CreateZip(rootPath + compressSource, rootPath + compressSource.Substring(0, compressSource.LastIndexOf('/')) + destinationName + ".zip");
            DirectoryInfo subdir = new DirectoryInfo(rootPath + compressSource);
            subdir.Delete(true);
        }
    }

    private static BuildTarget GetTarget(Platform platform)
    {
        switch (platform)
        {
        case Platform.Windows:
            return BuildTarget.StandaloneWindows;

        case Platform.OSX:
            return BuildTarget.StandaloneOSXIntel64;

        case Platform.Android:
            return BuildTarget.Android;

        case Platform.iOS:
            return BuildTarget.iOS;

        default:
            throw new ArgumentException();
        }
    }

    private static BuildTargetGroup GetTargetGroup(Platform platform)
    {
        switch (platform)
        {
        case Platform.Windows:
        case Platform.OSX:
            return BuildTargetGroup.Standalone;

        case Platform.Android:
            return BuildTargetGroup.Android;

        case Platform.iOS:
            return BuildTargetGroup.iOS;

        default:
            throw new ArgumentException();
        }
    }

    private static bool CopyExtenalTools(Platform platform, string outputPath)
    {
        if (platform != Platform.OSX &&
            platform != Platform.Windows)
        {
            return true;
        }

        bool error = false;
        var dataDir = EditorUtils.GetDataPath(GetTarget(platform), outputPath);
        var targetPlatform = platform.ToString();
        var excludePlatforms = Enum.GetNames(typeof(Platform)).Except(targetPlatform.ToString()).ToArray();
        foreach (var toolDir in GetExternalTools())
        {
            string targetDir = dataDir + "/" + toolDir;
#if UNITY_EDITOR_WIN
            var args = string.Format("\"{0}\" \"{1}\" /S /XD {2} /XF *.meta /XF *.pyc /UNICODE /NJH /NJS /NDL /NC /NS /NFL", 
                                     toolDir, targetDir, string.Join(" ", excludePlatforms));
            int code = EditorUtils.Run("robocopy", 2, args);
            if (code <= 1)
            {
                code = 0;
            }
#else
            if (!Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }
            var excludeDirs = string.Join(" ", excludePlatforms.Select(x => "--exclude=" + x + "/").ToArray());
            var args = string.Format("-r \"{0}/\" \"{1}\" {2} --exclude='*.meta' --exclude='*.pyc'", toolDir, targetDir, excludeDirs);
            int code = EditorUtils.Run("rsync", 1, args);
#endif
            if (code != 0)
            {
                Debug.LogErrorFormat("Failed to copy {0}, code: {1}", toolDir, code);
                error = true;
            }
        }

        return !error;
    }

    private static string[] GetExternalTools()
    {
        return Directory.GetDirectories("ExternalTools");
    }
}