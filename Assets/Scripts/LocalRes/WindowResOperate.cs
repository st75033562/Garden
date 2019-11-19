using UnityEngine;
using System.Collections;
using System;
using System.Runtime.InteropServices;


public class WindowResOperate : LocalResOperate
{
    public override void OpenResWindow(LocalResType type, Action<LocalRes> onLoaded)
    {
        OpenFileName ofn = new OpenFileName();
        ofn.structSize = Marshal.SizeOf(ofn);

        string filter;
        if(type == LocalResType.IMAGE) {
            filter = "ui_select_image_filter_win";
            ofn.title = "ui_select_image_title".Localize();
        } else if(type == LocalResType.COURSE) {
            filter = "ui_select_pdf_filter_win";
            ofn.title = "ui_select_pdf_filter_win".Localize();
        } else {
            filter = "ui_select_video_filter_win";
            ofn.title = "ui_select_video_title".Localize();
        }
        ofn.filter = filter.Localize().Replace("\\0", "\0");

        ofn.file = new string(new char[256]);
        ofn.maxFile = ofn.file.Length;
        ofn.fileTitle = new string(new char[64]);
        ofn.maxFileTitle = ofn.fileTitle.Length;
        ofn.initialDir = UnityEngine.Application.dataPath;//默认路径  
        ofn.flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000200 | 0x00000008;//OFN_EXPLORER|OFN_FILEMUSTEXIST|OFN_PATHMUSTEXIST| OFN_ALLOWMULTISELECT|OFN_NOCHANGEDIR  

        if (DllOpenWindow.GetOpenFileName(ofn))
        {
            Load(type, ofn.file, onLoaded);
        }
    }
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
public class OpenFileName
{
    public int structSize = 0;
    public IntPtr dlgOwner = IntPtr.Zero;
    public IntPtr instance = IntPtr.Zero;
    public String filter = null;
    public String customFilter = null;
    public int maxCustFilter = 0;
    public int filterIndex = 0;
    public String file = null;
    public int maxFile = 0;
    public String fileTitle = null;
    public int maxFileTitle = 0;
    public String initialDir = null;
    public String title = null;
    public int flags = 0;
    public short fileOffset = 0;
    public short fileExtension = 0;
    public String defExt = null;
    public IntPtr custData = IntPtr.Zero;
    public IntPtr hook = IntPtr.Zero;
    public String templateName = null;
    public IntPtr reservedPtr = IntPtr.Zero;
    public int reservedInt = 0;
    public int flagsEx = 0;
}

public class DllOpenWindow
{
    [DllImport("Comdlg32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
    public static extern bool GetOpenFileName([In, Out] OpenFileName ofn);
    public static bool GetOpenFileName1([In, Out] OpenFileName ofn)

    {
        return GetOpenFileName(ofn);
    }
}
