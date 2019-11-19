using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class IOSStreamingBundle : StreamingBundle
{
   // private string streamingAssets = 
    protected AssetBundleManifest assetMainfst;
    protected AssetBundle manifestAB;

    protected virtual string streamingAssets() {
        return "file://" + Application.dataPath + "/Raw/" + AssetBundles.Utility.GetPlatformName();
    }
    public override void LoadBundleManifest(Action<AssetBundleManifest> done)
    {
        StartCoroutine(LoadBundleManifest_I(done));
    }
    protected IEnumerator LoadBundleManifest_I(Action<AssetBundleManifest> done)
    {
        UnityWebRequest www = UnityWebRequest.GetAssetBundle(streamingAssets() + "/" + AssetBundles.Utility.GetPlatformName(), 0);

        yield return www.Send();

        if (!string.IsNullOrEmpty(www.error))
        {
            PopupManager.Notice(www.error);
            yield break;
        }
        manifestAB = (www.downloadHandler as DownloadHandlerAssetBundle).assetBundle;
        assetMainfst = manifestAB.LoadAsset<AssetBundleManifest>("AssetBundleManifest");

        done(assetMainfst);
    }


    public override void CacheBundle(string[] bundleNames, Action<float> progress)
    {
        StartCoroutine(CacheBundle_I(bundleNames, progress));
    }

    IEnumerator CacheBundle_I(string[] bundleNames, Action<float> progress)
    {
        int i = 0;
        foreach (var str in bundleNames)
        {
            UnityWebRequest www = UnityWebRequest.GetAssetBundle(streamingAssets() + "/" + str, assetMainfst.GetAssetBundleHash(str), 0);
            yield return www.Send();

            if (!string.IsNullOrEmpty(www.error))
            {
                PopupManager.Notice(www.error);
            }
            www.Dispose();
            progress(i++ / (float)bundleNames.Length);
        }
        manifestAB.Unload(true);
        progress(1);
    }
}
