using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using UnityEngine;
using System.Text;

public static class WindowUtils
{
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
    [DllImport("user32.dll")]
    static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    [DllImport("user32.Dll")]
    static extern bool EnumWindows(EnumWindowsProc callback, IntPtr lParam);

    delegate bool EnumWindowsProc(IntPtr hwnd, IntPtr lParam);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    static extern bool SetWindowText(IntPtr hwnd, String lpString);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    static extern int GetWindowText(IntPtr hwnd, StringBuilder lpString, int nMaxCount);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    static extern IntPtr GetSystemMenu(IntPtr hwnd, bool bRevert);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    static extern bool EnableMenuItem(IntPtr hMenu, uint uIDEnableItem, uint uEnable);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern bool BringWindowToTop(IntPtr hWnd);

    private const uint SC_CLOSE = 0xF060u;

    private const uint MF_DISABLED = 2u;
    private const uint MF_ENABLED = 0u;
    private const uint MF_GRAYED = 1u;

    private static IntPtr s_mainWindowHandle;

    static WindowUtils()
    {
        InitializeMainWindowHandle();
    }

    private static void InitializeMainWindowHandle()
    {
        EnumWindows(EnumAppMainWindowCallback, IntPtr.Zero);
    }

    private static bool EnumAppMainWindowCallback(IntPtr hWnd, IntPtr lParam)
    {
        uint procId;
        GetWindowThreadProcessId(hWnd, out procId);
        if (procId == Process.GetCurrentProcess().Id)
        {
            s_mainWindowHandle = hWnd;
            return false;
        }
        return true;
    }
#elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
    [DllImport("MacUtils")]
    private static extern void mu_set_main_window_title(string title);
#endif

    public static void SetMainWindowTitle(string title)
    {
        if (title == null)
        {
            throw new ArgumentNullException();
        }

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        if (s_mainWindowHandle != IntPtr.Zero)
        {
            SetWindowText(s_mainWindowHandle, title);
        }
#elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
        mu_set_main_window_title(title);
#endif
    }

    public static void EnableCloseButton(bool enabled)
    {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        if (s_mainWindowHandle != IntPtr.Zero)
        {
            // https://blogs.msdn.microsoft.com/oldnewthing/20100604-00/?p=13803
            var state = enabled ? MF_ENABLED : MF_DISABLED | MF_GRAYED;
            EnableMenuItem(GetSystemMenu(s_mainWindowHandle, false), SC_CLOSE, state);
        }
#endif
    }

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
    public static List<IntPtr> EnumWindows(int pid)
    {
        var windows = new List<IntPtr>();
        EnumWindows((hWnd, lParam) => {
            uint procId;
            GetWindowThreadProcessId(hWnd, out procId);
            if (pid == procId)
            {
                windows.Add(hWnd);
            }
            return true;
        }, IntPtr.Zero);
        return windows;
    }

    public static string GetWindowText(IntPtr hWnd)
    {
        if (hWnd == IntPtr.Zero)
        {
            throw new ArgumentException("hWnd");
        }

        var buffer = new StringBuilder(512);
        GetWindowText(hWnd, buffer, buffer.Capacity);
        return buffer.ToString();
    }

    public static IntPtr MainWindowHandle
    {
        get { return s_mainWindowHandle; }
    }
#endif
}
