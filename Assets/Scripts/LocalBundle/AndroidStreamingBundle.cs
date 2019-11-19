using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AndroidStreamingBundle : StreamingBundle
{
    private string streamingAssets = "jar:file://" + Application.dataPath + "!/assets/" + AssetBundles.Utility.GetPlatformName();
    private AssetBundleManifest assetMainfst;
    private AssetBundle manifestAB;
    public override void LoadBundleManifest(Action<AssetBundleManifest> done)
    {
        StartCoroutine(LoadBundleManifest_I(done));
    }

    IEnumerator LoadBundleManifest_I(Action<AssetBundleManifest> done) {
        WWW www = WWW.LoadFromCacheOrDownload(streamingAssets + "/" + AssetBundles.Utility.GetPlatformName(), 0);
        yield return www;

        if (!string.IsNullOrEmpty(www.error))
        {
            PopupManager.Notice(www.error);
            yield break;
        }
        manifestAB = www.assetBundle;
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
        foreach (var str in bundleNames) {
            WWW www = WWW.LoadFromCacheOrDownload(streamingAssets + "/" + str, assetMainfst.GetAssetBundleHash(str), 0);
            yield return www;

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
