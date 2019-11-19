using System;
using System.IO;
using System.Linq;
using UnityEngine;

public static class LogListener
{
    private const int MaxLogNum = 2;
    private static readonly string LogDir = Application.persistentDataPath + "/.log";

    private static readonly object s_gate = new object();
    private static StreamWriter s_logWriter;

    public static void Init()
    {
        try
        {
            if (!Directory.Exists(LogDir))
            {
                Directory.CreateDirectory(LogDir);
            }

            FileUtils.ensureMaximumFiles(LogDir, MaxLogNum - 1);

            var logPath = LogDir + "/" + DateTime.Now.ToString("yyyy-MM-dd'_'HH-mm-ss") + ".txt";
            s_logWriter = new StreamWriter(new FileStream(logPath, FileMode.Create, FileAccess.Write, FileShare.Read));
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            return;
        }

        Application.logMessageReceivedThreaded += LogMessageReceived;

        CmdServer.Register("log", OnLogCommand);
    }

    public static void Shutdown()
    {
        CmdServer.Unregister("log");

        Application.logMessageReceivedThreaded -= LogMessageReceived;

        lock (s_gate)
        {
            if (s_logWriter != null)
            {
                s_logWriter.Close();
                s_logWriter = null;
            }
        }
    }

    static void LogMessageReceived(string condition, string stackTrace, LogType type)
    {
        lock (s_gate)
        {
            s_logWriter.WriteLine(condition);
            s_logWriter.WriteLine(stackTrace);
            s_logWriter.Flush();
        }

#if UNITY_IOS && !UNITY_EDITOR
        // we don't want to double log when the app is run from xcode.
        if (!iOSUtils.isDebuggerAttached)
        {
            iOSUtils.Log(condition);
            iOSUtils.Log(stackTrace);
        }
#endif
    }

    private static string OnLogCommand(string[] args)
    {
        switch (args[0])
        {
        case "list":
            var files = Directory.GetFiles(LogDir).Select(x => Path.GetFileName(x)).ToArray();
            return string.Join("\n", files);

        case "cat":
            using (var fs = new FileStream(LogDir + "/" + args[1], FileMode.Open, FileAccess.Read, FileShare.Write))
            using (var reader = new StreamReader(fs))
            {
                return reader.ReadToEnd();
            }
        }

        return "";
    }
}
