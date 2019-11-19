using UnityEngine;
using System.Collections;
using System;

public class AndroidResOperate : LocalResOperate
{

    public override void OpenResWindow(LocalResType type, Action<LocalRes> onLoaded)
    {
        GameObject go = new GameObject("ReciveAndroid");
        ReciveAndroid reciveAndroid = go.AddComponent<ReciveAndroid>();
        reciveAndroid.SetImageListen((path)=> {
            UnityEngine.Object.Destroy(go);
            if (string.IsNullOrEmpty(path))
            {
                return;
            }
            Load(type, path, onLoaded);
        });

        AndroidJavaClass jc = new AndroidJavaClass("com.xhg.androidtools.GalleryRes");
        AndroidJavaObject jo = jc.CallStatic<AndroidJavaObject>("initial");
        if (type == LocalResType.IMAGE)
        {
            jo.Call("OpenGetGallryImage", "text_source_select".Localize());
        }
        else
        {
            jo.Call("OpenGetGallryVideo", "text_source_select".Localize());
        }
    }
}
