using System;
using UnityEngine;

#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX || UNITY_IOS && UNITY_EDITOR
public class MacResOperate : LocalResOperate {
    public override void OpenResWindow(LocalResType type, Action<LocalRes> onLoaded)
    {
		var panel = new NSOpenPanel();
        if (type == LocalResType.IMAGE)
        {
            panel.Title = "ui_select_image_title".Localize();
            panel.Message = "ui_select_image_message".Localize();
            panel.FileExts = new[] { "jpg", "png" };
        }
        else if(type == LocalResType.COURSE)
        {
            panel.Title = "ui_select_pdf_title".Localize();
            panel.Message = "ui_select_pdf_message".Localize();
            panel.FileExts = new[] { "pdf"};
        }
        else
        {
            panel.Title = "ui_select_video_title".Localize();
            panel.Message = "ui_select_video_message".Localize();
            panel.FileExts = new[] { "mp4" };
        }
		panel.Open(file => {
            if (file != null)
            {
                Load(type, file, onLoaded);
            }
		});
    }
}
#endif