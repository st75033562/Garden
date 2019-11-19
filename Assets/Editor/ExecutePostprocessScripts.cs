using UnityEngine;
using System.Collections;
using UnityEditor.Callbacks;
using UnityEditor;
using System.IO;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

public static class ExecutePostprocessScripts
{
    [PostProcessBuild(1)]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject) {
        if (target != BuildTarget.iOS)
        {
            return;
        }

        var files = Directory.GetFiles(Application.dataPath, "PostprocessBuildPlayer", SearchOption.AllDirectories);
        foreach (var f in files)
        {
            if (Path.GetDirectoryName(f).EndsWith("Editor"))
            {
                // Debug.Log(f);
                EditorUtils.Run(f, pathToBuiltProject, target.ToString());
            }
        }
    }

}
