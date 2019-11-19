using AssetBundles;
using System;
using System.Collections;
using UnityEngine;

public class LocalizationBundleDataSource : LocalizationDataSourceBase
{
    private AssetBundleLoadBundleOperation m_loadOp;
    private Coroutine m_coLoad;

    public override void setLanguage(SystemLanguage language)
    {
        if (m_language != language)
        {
            m_language = language;
            unloadBundle();
        }
    }

    private void unloadBundle()
    {
        if (m_loadOp != null)
        {
            m_loadOp.Dispose();
            m_loadOp = null;
        }

        if (m_coLoad != null)
        {
            CoroutineService.instance.StopCoroutine(m_coLoad);
            m_coLoad = null;
        }
    }

    public override void uninitialize()
    {
        unloadBundle();
    }

    public override AsyncRequest<string> loadString()
    {
        if (m_coLoad != null)
        {
            throw new InvalidOperationException();
        }

        var request = new SimpleAsyncRequest<string>();
        m_coLoad = CoroutineService.instance.StartCoroutine(loadStringBundle(request));
        return request;
    }

    private IEnumerator loadStringBundle(SimpleAsyncRequest<string> request)
    {
        string bundlePath = BundlePath.GetName(
            "localization", 
            LocalizedResType.String, 
            LocalizationManager.getLocaleDir(m_language));
        m_loadOp = AssetBundleManager.LoadAssetBundleAsync(bundlePath);
        yield return m_loadOp;

        string text = null;
        if (m_loadOp.assetBundle != null)
        {
            var asset = m_loadOp.assetBundle.LoadAsset<TextAsset>(StringFileName);
            text = asset ? asset.text : null;

            AssetBundleManager.UnloadAssetBundle(bundlePath);
        }
        request.SetResult(text);
        m_coLoad = null;
        m_loadOp.Dispose();
        m_loadOp = null;
    }
}
