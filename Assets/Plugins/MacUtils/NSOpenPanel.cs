using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Assertions;

#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX || UNITY_IOS && UNITY_EDITOR

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void NSOpenPanelCallback(string file);

// A simple wrapper for NSOpenPanel
public class NSOpenPanel
{
    [StructLayout(LayoutKind.Sequential)]
    private class OpenPanelInfo
    {
        public string title;
        public string message;
        public string fileExts;
    }

    [DllImport("MacUtils")]
    private static extern void mu_open_panel(OpenPanelInfo info, NSOpenPanelCallback cb);

    private OpenPanelInfo m_info = new OpenPanelInfo();
    private GCHandle m_callbackHandle;
    private NSOpenPanelCallback m_callback;

    public string Title
    {
        get { return m_info.title; }
        set { m_info.title = value; }
    }

    public string Message
    {
        get { return m_info.message; }
        set { m_info.message = value; }
    }

    public string[] FileExts
    {
        get
        {
            return m_info.fileExts != null ? m_info.fileExts.Split(';') : null;
        }
        set
        {
            if (value != null)
            {
                m_info.fileExts = string.Join(";", value);
            }
            else
            {
                m_info.fileExts = null;
            }
        }
    }

    public void Open(NSOpenPanelCallback callback)
    {
        Assert.IsTrue(!m_callbackHandle.IsAllocated && callback != null);

        NSOpenPanelCallback cb = OnPanelClosed;
        m_callbackHandle = GCHandle.Alloc(cb);
        m_callback = callback;
        mu_open_panel(m_info, cb);
    }

    private void OnPanelClosed(string file)
    {
        try
        {
            if (m_callback != null)
            {
                m_callback(file);
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
        finally
        {
            m_callback = null;
        }

        m_callbackHandle.Free();
    }
}
#endif // UNITY_STANDALONE_OSX