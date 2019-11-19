using System;
using UnityEngine;

public class iOSResOperate : LocalResOperate {
    public override void OpenResWindow(LocalResType type, Action<LocalRes> onLoaded)
    {
        var pickType = type == LocalResType.IMAGE ? GalleryPickMediaType.Image : GalleryPickMediaType.Video;
        GalleryUtils.Pick(pickType,
            path => {
                if (path != null) {
                    Load(type, path, onLoaded);
                }
            });
    }
}