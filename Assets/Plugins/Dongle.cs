using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public enum DongleNotificationType
{
    Insertion,
    Removal
}

[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
public struct DongleNotification
{
    public DongleNotificationType type;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst=20)]
    public string portName;
}

public static class Dongle
{
    public static bool Init()
    {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        return dg_init(WindowUtils.MainWindowHandle) != 0;
#else
        return false;
#endif
    }

    public static void Uninit()
    {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        dg_uninit();
#endif
    }

    public static int numNotifications
    {
        get
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            return dg_num_notifications();
#else
            return 0;
#endif
        }
    }

    public static DongleNotification GetNotification(int index)
    {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        var ptr = dg_get_notification(index);
        return (DongleNotification)Marshal.PtrToStructure(ptr, typeof(DongleNotification));
#else
        return new DongleNotification();
#endif
    }

    public static void ClearNotifications()
    {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        dg_clear_notifications();
#endif
    }

    public static void GetNotifications(ICollection<DongleNotification> result)
    {
        if (result == null)
        {
            throw new ArgumentNullException("result");
        }

        var num = numNotifications;
        for (int i = 0; i < num; ++i)
        {
            result.Add(GetNotification(i));
        }
        if (num > 0)
        {
            ClearNotifications();
        }
    }

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
    [DllImport("dongle", CallingConvention = CallingConvention.Cdecl)]
    private static extern int dg_init(IntPtr hWnd);

    [DllImport("dongle", CallingConvention = CallingConvention.Cdecl)]
    private static extern void dg_uninit();

    [DllImport("dongle", CallingConvention = CallingConvention.Cdecl)]
    private static extern int dg_num_notifications();

    [DllImport("dongle", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr dg_get_notification(int index);

    [DllImport("dongle", CallingConvention = CallingConvention.Cdecl)]
    private static extern void dg_clear_notifications();
#endif
}