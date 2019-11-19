using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinStreamingBundle : IOSStreamingBundle
{
    protected override string streamingAssets()
    {
        return "file://" + Application.dataPath + "/StreamingAssets/" + AssetBundles.Utility.GetPlatformName();
    }
}
