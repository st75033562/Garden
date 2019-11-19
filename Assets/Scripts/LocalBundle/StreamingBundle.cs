using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class StreamingBundle : MonoBehaviour {

    public abstract void LoadBundleManifest(Action<AssetBundleManifest> done);

    public abstract void CacheBundle(string[] bundleNames, Action<float> progress);
}
