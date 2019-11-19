using UnityEngine;
using System.Collections;
using System;
using System.IO;

public enum LocalResType
{
    IMAGE = 1,
    VIDEO,
    COURSE
}

public class LocalRes
{
    public LocalResType type;
    public byte[] imageData;
    public string path;
    public string hash;
}

public abstract class LocalResOperate {
    private const int oneMegaBytes = 1024 * 1024;
    private const int maxFileSize = 30 * oneMegaBytes;

    static LocalResOperate()
    {
#if UNITY_STANDALONE_WIN || UNITY_ANDROID && UNITY_EDITOR
        instance = new WindowResOperate();
#elif UNITY_ANDROID && !UNITY_EDITOR
        instance = new AndroidResOperate();
#elif UNITY_STANDALONE_OSX || UNITY_IOS && UNITY_EDITOR
        instance = new MacResOperate();
#elif UNITY_IOS && !UNITY_EDITOR
        instance = new iOSResOperate();
#else
#error Unsupported platform
#endif
    }

    public static LocalResOperate instance
    {
        get;
        private set;
    }

    public abstract void OpenResWindow(LocalResType type, Action<LocalRes> onLoaded);

    protected void Load(LocalResType type, string path, Action<LocalRes> onLoaded, Action onError = null)
    {
        FileInfo fileInfo = new FileInfo(path);
        if(fileInfo.Length > maxFileSize) {
            PopupManager.Notice(string.Format("ui_max_file_size".Localize(), (maxFileSize / oneMegaBytes).ToString()));
            return;
        }

        int popupId = PopupManager.ShowMask("ui_loading_resource".Localize());
        var res = new LocalRes();
        res.path = path;
        res.type = type;
        LoadResource.instance.LoadLocalRes(path,
            (www) => {
                if(type != LocalResType.VIDEO)
                    res.imageData = www.bytes;
                res.hash = Md5.CreateMD5Hash(www.bytes);
                PopupManager.Close(popupId);
                onLoaded(res);
            }, onError);
    }
}
