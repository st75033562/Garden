using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

public class PythonEditorManager : Singleton<PythonEditorManager>
{
    private static readonly A8.Logger s_logger = A8.Logger.GetLogger<PythonEditorManager>();

    private Coroutine m_editorProcessMonitor;
    private readonly Dictionary<string, Process> m_editorDict = new Dictionary<string, Process>();

    public void Shutdown()
    {
        if (m_editorProcessMonitor == null)
        {
            StopCoroutine(m_editorProcessMonitor);
            m_editorProcessMonitor = null;
        }

        foreach (var proc in m_editorDict.Values)
        {
            proc.Dispose();
        }
        m_editorDict.Clear();
    }

    /// <summary>
    /// open an editor for editing, if there's already an opened editor, it will be brought to the front.
    /// </summary>
    /// <param name="path">absolute path to the python script</param>
    public void Open(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            throw new ArgumentException("path");
        }

        if (m_editorProcessMonitor == null)
        {
            m_editorProcessMonitor = StartCoroutine(EditorProcessMonitor());
        }

        Process editorProc;
        if (m_editorDict.TryGetValue(path, out editorProc))
        {
            if (editorProc.HasExited)
            {
                editorProc.Dispose();
                m_editorDict.Remove(path);
                OpenEditor(path);
            }
            else
            {
                FocusEditor(editorProc, path);
            }
        }
        else
        {
            OpenEditor(path);
        }
    }

    private IEnumerator EditorProcessMonitor()
    {
        for (;;)
        {
            yield return new WaitForSecondsRealtime(.25f);

            foreach (var kv in m_editorDict.Where(x => x.Value.HasExited).ToArray())
            {
                kv.Value.Dispose();
                m_editorDict.Remove(kv.Key);
            }
        }
    }

    void OpenEditor(string scriptPath)
    {
        try
        {
            var process = new Process();
            if (Application.platform == RuntimePlatform.WindowsEditor ||
                Application.platform == RuntimePlatform.WindowsPlayer)
            {
                var startDir = ApplicationUtils.externalToolsPath + "/Python/Windows";
                process.StartInfo.FileName = startDir + "/pythonw.exe";
                process.StartInfo.WorkingDirectory = startDir;
                process.StartInfo.Arguments = @"Lib\idlelib\idle.pyw " + Utils.QuoteArgument(scriptPath);
            }
            else if (Application.platform == RuntimePlatform.OSXEditor ||
                     Application.platform == RuntimePlatform.OSXPlayer)
            {
                process.StartInfo.FileName = "sh";
                process.StartInfo.WorkingDirectory = ApplicationUtils.externalToolsPath + "/Python/OSX";
                process.StartInfo.Arguments = @"robot.sh Lib/idlelib/idle.pyw " + Utils.QuoteArgument(scriptPath);
            }
            else
            {
                UnityEngine.Debug.LogError("not implemented");
                return;
            }

            process.Start();
            m_editorDict.Add(scriptPath, process);
            FocusEditor(process, scriptPath);
        }
        catch (Exception e)
        {
            s_logger.LogException(e);
        }
    }

    void FocusEditor(Process editorProc, string sourcePath)
    {
        int pid;
        try
        {
            pid = editorProc.Id;
        }
        catch (InvalidOperationException)
        {
            // the process has exited
            editorProc.Dispose();
            m_editorDict.Remove(sourcePath);
            return;
        }

#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
        string Source = @"
            tell application ""System Events""
                set delayTime to 0.25
                repeat 5 times
                    repeat with p in (processes whose name is ""python.exe"")
                        tell p
                            set ppid to do shell script ""ps -o ppid= -p "" & unix id & "" | grep -o '[0-9]\\+'""
                            if ppid is """ + pid + @""" then
                                set frontmost to true
                                click(menu item 1 where its name contains """ + sourcePath + @""") of menu 1 of menu bar item ""Window"" of menu bar 1
                                return
                            end if
                        end tell
                    end repeat
                    delay delayTime
                    set delayTime to delayTime * 2
                    if delayTime > 1
                        set delayTime to 1
                    end if
                end repeat
            end tell
        ";
        MacUtils.RunAppleScript(Source);
#elif UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        foreach (var hWnd in WindowUtils.EnumWindows(pid))
        {
            if (WindowUtils.GetWindowText(hWnd).Contains(FileUtils.normalizeSlash(sourcePath)))
            {
                WindowUtils.BringWindowToTop(hWnd);
                break;
            }
        }
#endif
    }
}
