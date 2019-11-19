using System;
using System.Runtime.InteropServices;

public static class iOSUtils
{
#if UNITY_IOS && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void util_log(string message);

    [DllImport("__Internal")]
    private static extern bool util_is_debugger_attached();
#endif

    public static void Log(string message)
    {
        if (message == null)
        {
            throw new ArgumentNullException("message");
        }

#if UNITY_IOS && !UNITY_EDITOR
        util_log(message);
#endif
    }

    public static bool isDebuggerAttached
    {
        get
        {
#if UNITY_IOS && !UNITY_EDITOR
            return util_is_debugger_attached();
#else
            return false;
#endif
        }
    }
}
