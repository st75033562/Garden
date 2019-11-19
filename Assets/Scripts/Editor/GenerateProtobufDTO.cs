using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Diagnostics;
using System.ComponentModel;
using System;
using Debug = UnityEngine.Debug;
using System.IO;

public class GenerateProtobufDTO
{
    private const string ToolPath = "Tools/protobuf/protoc.exe";

    [MenuItem("Tools/Generate Protobuf DTO")]
    public static void Run()
    {
        string ProtoDir = "Assets/Scripts/Proto";
        string OutputPath = "Assets/Scripts/Generated";

        if (GenerateDTO(ProtoDir, OutputPath, "csharp"))
        {
            AssetDatabase.ImportAsset(OutputPath, ImportAssetOptions.ImportRecursive);
        }

        const string PythonDir = "ExternalTools/Python/Windows/Lib/HamsterAPI";
        GenerateDTO(ProtoDir + "/gb_scripting.proto", PythonDir, "python");
    }

    public static bool GenerateDTO(string inputPath, string outputPath, string language)
    {
        string inputDir = inputPath;
        string protoFiles = inputPath;
        if (Directory.Exists(inputPath))
        {
            protoFiles = string.Join(" ", Directory.GetFiles(inputPath, "*.proto"));
        }
        else
        {
            inputDir = Path.GetDirectoryName(inputPath);
        }

        Process p = new Process();
        p.StartInfo.FileName = ToolPath;
        p.StartInfo.Arguments = string.Format("-I{0} -I{1} --{2}_out={3} {4}",
            Path.GetDirectoryName(ToolPath), inputDir, language, outputPath, protoFiles);
        p.StartInfo.RedirectStandardOutput = true;
        p.StartInfo.RedirectStandardError = true;
        p.StartInfo.CreateNoWindow = true;
        p.StartInfo.UseShellExecute = false;

        try
        {
            p.Start();
            p.WaitForExit();

            if (p.ExitCode == 0)
            {
                return true;
            }
            else
            {
                Debug.LogError(p.StandardError.ReadToEnd());
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }

        return false;
    }
}
