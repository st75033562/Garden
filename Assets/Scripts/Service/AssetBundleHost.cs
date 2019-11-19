using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class AssetBundleHostList
{
    private const string DefaultFilename = "bundle-hosts";
    private const string DefaultPath = "Assets/Resources/" + DefaultFilename + ".json";

    public List<string> hosts = new List<string>();
    public int enabledHostIndex = -1;

    public static AssetBundleHostList Load()
    {
        string content = null;
#if UNITY_EDITOR
        if (File.Exists(DefaultPath))
        {
            content = File.ReadAllText(DefaultPath);
        }
#else
        var asset = Resources.Load<TextAsset>(DefaultFilename);
        if (asset != null)
        {
            content = asset.text;
        }
#endif
        return content != null ? JsonUtility.FromJson<AssetBundleHostList>(content) : null;
    }

    public void Save()
    {
#if UNITY_EDITOR
        File.WriteAllText(DefaultPath, JsonUtility.ToJson(this, true));
#else
        Debug.LogError("can only be saved in editor");
#endif
    }

    public string EnabledHost
    {
        get
        {
            if (enabledHostIndex >= 0 && enabledHostIndex < hosts.Count)
            {
                return hosts[enabledHostIndex];
            }
            return null;
        }
    }

    public void Remove(int i)
    {
        if (enabledHostIndex == i)
        {
            enabledHostIndex = -1;
        }
        hosts.RemoveAt(i);
    }
}
