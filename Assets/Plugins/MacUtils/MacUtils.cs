using System;
using System.Runtime.InteropServices;

#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
public static class MacUtils
{
    [DllImport("MacUtils")]
    private static extern void mu_run_apple_script(string source);

    public static void RunAppleScript(string source)
    {
        if (source == null)
        {
            throw new ArgumentNullException("source");
        }

        mu_run_apple_script(source);
    }
}
#endif