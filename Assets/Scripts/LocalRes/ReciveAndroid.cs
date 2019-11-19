using UnityEngine;
using System.Collections;
using System;

public class ReciveAndroid : MonoBehaviour {

    private Action<string> imageListen;

    public void SetImageListen(Action<string> action)
    {
        imageListen = action;
    }

    public void ReciveGalleryImagePath(string str)
    {
        if(imageListen != null)
            imageListen(str);
        imageListen = null;
    }

    public void ReciveGalleryVideoPath(string str)
    {
        if (imageListen != null)
            imageListen(str);
        imageListen = null;
    }


}
