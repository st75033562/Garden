using UnityEngine;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

public static class ApplicationUtils
{
    static ApplicationUtils()
    {
#if UNITY_EDITOR
        externalToolsPath = Application.dataPath + "/../ExternalTools";
#else
        externalToolsPath = Application.dataPath + "/ExternalTools";
#endif

        externalToolsPath = Path.GetFullPath(externalToolsPath).Replace('\\', '/');
    }

    public static string externalToolsPath
    {
        get; private set;
    }

    // return path to a file in a the given tool for the current platform
    // the path is <externalToolsPath>/<toolName>/<platform>/file
    public static string GetPlatformFilePath(string toolName, string file)
    {
        string platform;
        #if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
        platform = "OSX";
        #elif UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        platform = "Windows";
        #else
        Debug.Log("Unsupported platform");
        platform = "_Unsupported";
        #endif

        return string.Format("{0}/{1}/{2}/{3}", externalToolsPath, toolName, platform, file);
    }

    public static string identifier
    {
        get
        {
#if UNITY_STANDALONE_WIN
            return "com.activ8.pockebot";
#elif UNITY_EDITOR
            return PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Standalone);
#else
            return Application.identifier;
#endif
        }
    }
}

