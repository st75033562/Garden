using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

public static class EditorUtils
{
    private static MethodInfo s_gameViewRepaintAll;

    /// <summary>
    /// run a command with given arguments, log error output if exit code >= errorLevel
    /// </summary>
    /// <returns></returns>
    public static int Run(string command, int errorLevel, string arguments)
    {
        Debug.Log("executing " + command + " " + arguments);

        Process p = new Process();
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.CreateNoWindow = true;
        p.StartInfo.RedirectStandardOutput = true;
        p.StartInfo.RedirectStandardError = true;
        p.StartInfo.Arguments = arguments ?? string.Empty;
        p.StartInfo.FileName = command;

        p.Start();
        p.WaitForExit();
        var output = p.StandardOutput.ReadToEnd();
        if (output != string.Empty)
        {
            Debug.Log(output);
        }
        if (p.ExitCode >= errorLevel)
        {
            Debug.LogError("exit code: " + p.ExitCode);
            var errorOutput = p.StandardError.ReadToEnd();
            if (errorOutput != string.Empty) 
            {
                Debug.LogError(errorOutput);
            }
        }
        return p.ExitCode;
    }

    /// <summary>
    /// equivalent to Run(command, 1, arguments)
    /// </summary>
    /// <see cref="Run"/>
    public static int Run(string command, params string[] arguments)
    {
        return Run(command, 1, string.Join(" ", arguments.Select(x => Utils.QuoteArgument(x)).ToArray()));
    }

    public static void CaptureScreenshot(string path)
    {
        // focus the game view otherwise the screenshot won't be written immediately
        var gameViewType = typeof(UnityEditor.EditorWindow).Assembly.GetType("UnityEditor.GameView");
        var gameView = EditorWindow.GetWindow(gameViewType);
        gameView.Focus();
        Application.CaptureScreenshot(path);
    }

    /// <summary>
    /// return the relative path to the project directory
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string GetProjectRelativePath(string path)
    {
        var projectPath = Application.dataPath.Substring(0, Application.dataPath.Length - 6);
        return path.Replace('\\', '/').Replace(projectPath, "");
    }

    public static string GetDataPath(BuildTarget target, string buildPath)
    {
        switch (target)
        {
        case BuildTarget.StandaloneWindows:
        case BuildTarget.StandaloneWindows64:
            return Path.GetDirectoryName(buildPath) + "/" + Path.GetFileNameWithoutExtension(buildPath) + "_Data";

        case BuildTarget.StandaloneOSXIntel:
        case BuildTarget.StandaloneOSXIntel64:
        case BuildTarget.StandaloneOSXUniversal:
            return buildPath + "/Contents";

        default:
            throw new NotImplementedException();
        }
    }

    public static void AddLabel(UnityEngine.Object asset, string label)
    {
        var labels = AssetDatabase.GetLabels(asset);
        if (Array.IndexOf(labels, label) == -1)
        {
            AssetDatabase.SetLabels(asset, labels.Concat(new[] { label }).ToArray());
        }
    }

    public static void RemoveLabel(UnityEngine.Object asset, string label)
    {
        var labels = AssetDatabase.GetLabels(asset);
        AssetDatabase.SetLabels(asset, labels.Except(new[] { label }).ToArray());
    }

    public static void RepaintGameView()
    {
        if (s_gameViewRepaintAll == null)
        {
            s_gameViewRepaintAll = typeof(SceneView).Assembly.GetType("UnityEditor.GameView").GetMethod("RepaintAll");
        }
        s_gameViewRepaintAll.Invoke(null, null);
    }

    public static string GetPropertyDisplayName(string propertyName)
    {
        if (propertyName == null)
        {
            throw new ArgumentNullException("propertyName");
        }

        if (propertyName.StartsWith("m_"))
        {
            propertyName = propertyName.Substring(2);
        }

        return string.Join(" ", Regex.Split(propertyName, "(?=[A-Z])").Select(x => x.Capitalize()).ToArray());
    }
}

