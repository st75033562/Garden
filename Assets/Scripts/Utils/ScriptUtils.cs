using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Debug = UnityEngine.Debug;

public interface IScriptHandle : IDisposable
{
    void KillTree();
}

class SimpleScriptHandle : IScriptHandle
{
    private readonly Process m_process;

    public SimpleScriptHandle(Process p)
    {
        if (p == null)
        {
            throw new ArgumentNullException("p");
        }

        m_process = p;
    }

    public void Dispose()
    {
        m_process.Dispose();
    }

    public void KillTree()
    {
        m_process.KillTree();
        m_process.Dispose();
    }
}

#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
class MacScriptHandle : IScriptHandle
{
    private readonly Process m_process;
    private readonly string m_command;
    private readonly string m_tempScriptPath;

    public MacScriptHandle(string command, string tempScriptPath, Process p)
    {
        if (string.IsNullOrEmpty(command))
        {
            throw new ArgumentException("command");
        }
        if (string.IsNullOrEmpty(tempScriptPath))
        {
            throw new ArgumentException("tempScriptPath");
        }
        if (p == null)
        {
            throw new ArgumentNullException("p");
        }

        m_command = command;
        m_tempScriptPath = tempScriptPath;
        m_process = p;
    }

    public void Dispose()
    {
        m_process.Dispose();
    }

    public void KillTree()
    {
        int pid = FindPid();
        if (pid != -1)
        {
            if (Kill(pid))
            {
                CloseTerminal();
            }
            else
            {
                Debug.Log("kill error");
            }
        }
    }

    private int FindPid()
    {
        var temp = FileUtils.appTempPath + "/" + Guid.NewGuid();
        using (var proc = Process.Start(new ProcessStartInfo
        {
            FileName = "sh",
            Arguments = "-c 'ps -u $(whoami) -o pid,command >" + temp + "'",
            UseShellExecute = true,
        }))
        {
            proc.WaitForExit();
            if (proc.ExitCode == 0)
            {
                try
                {
                    foreach (var line in File.ReadAllLines(temp))
                    {
                        var fields = line.TrimStart().Split(new[] { ' ' }, 2);
                        if (fields[1].Contains(m_command))
                        {
                            return int.Parse(fields[0]);
                        }
                    }
                }
                catch (IOException)
                {
                }
                finally
                {
                    FileUtils.safeDelete(temp);
                }
            }
        }

        return -1;
    }

    private bool Kill(int pid)
    {
        using (var proc = Process.Start(new ProcessStartInfo
        {
            FileName = "kill",
            Arguments = pid.ToString(),
            CreateNoWindow = true
        }))
        {
            proc.WaitForExit();
            return proc.ExitCode == 0;
        }
    }

    private void CloseTerminal()
    {
        var script = string.Format(@"
            tell application ""Terminal""
                repeat with win in windows
                    if name of win contains ""{0}"" then
                        close window (name of win)
                        exit repeat
                    end if
                end repeat
            end tell
        ", m_tempScriptPath);
        MacUtils.RunAppleScript(script);
    }

}
#endif

public static class ScriptUtils
{

    public static IScriptHandle Run(string scriptPath, Dictionary<string, string> env = null)
    {
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
        string commandPath = ApplicationUtils.externalToolsPath + "/Python/OSX/robot.sh";
#else
        string commandPath = ApplicationUtils.externalToolsPath + "/Python/Windows/robot.bat";
#endif
        try
        {
            string command = Utils.QuoteArgument(commandPath) + " " + Utils.QuoteArgument(FileUtils.normalizeSlash(scriptPath));
            string tempScriptPath;
            var proc = Utils.RunInTerminal(command, out tempScriptPath, env);
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
            return new MacScriptHandle(
                commandPath + " " + FileUtils.normalizeSlash(scriptPath), // command without quotes
                tempScriptPath, 
                proc);
#else
            return new SimpleScriptHandle(proc);
#endif
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            return null;
        }
    }
}
