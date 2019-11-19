using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OSXStreamingBundle : IOSStreamingBundle
{
    protected override string streamingAssets()
    {
        return "file://" + Application.streamingAssetsPath + "/" + AssetBundles.Utility.GetPlatformName();
    }
}
