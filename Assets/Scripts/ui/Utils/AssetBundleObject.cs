using AssetBundles;
using System;
using System.Collections;
using UnityEngine;
using DataAccess;

public abstract class AssetBundleObject : MonoBehaviour
{
    [SerializeField]
    private string m_bundleName;

    [SerializeField]
    private string m_assetName;

    private AssetBundleLoadAssetOperation m_loadRequest;
    private bool m_needUnload;

    public event Action loadFinished;

    private Coroutine m_coroutine;

    protected virtual void Awake()
    {
    }

    protected virtual void Start()
    {
    }

    protected virtual void OnDestroy()
    {
        Unload();
    }

    protected virtual void OnEnable()
    {
        // reload the asset if necessary
        if (!m_needUnload && !string.IsNullOrEmpty(m_bundleName) && !string.IsNullOrEmpty(m_assetName))
        {
            SetAssetInternal(m_bundleName, m_assetName);
        }
    }

    protected virtual void OnDisable()
    {
        AbortLoading();
    }

    protected void AbortLoading()
    {
        if (AbortRequest())
        {
            UnloadAssetBundle();
            // in case OnBeginLoad was called, we need to undo any changes
            OnEndLoad();
        }
    }

    public void SetAsset(string bundleName, string assetName)
    {
        if (m_bundleName == bundleName && m_assetName == assetName)
        {
            return;
        }
        SetAssetInternal(bundleName, assetName);
    }
    
    private void SetAssetInternal(string bundleName, string assetName)
    {
        Unload();
        if (m_coroutine != null)
        {
            StopCoroutine(m_coroutine);
        }
        m_coroutine = StartCoroutine(SetAssetImpl(bundleName, assetName));
    }

    public void SetAsset(int id)
    {
        var asset = BundleAssetData.Get(id);
        if (asset != null)
        {
            SetAsset(asset.bundleName, asset.assetName);
        }
        else
        {
            Debug.LogError("invalid bundle asset id: " + id);
        }
    }

    IEnumerator SetAssetImpl(string bundleName, string assetName)
    {
        m_bundleName = bundleName;
        m_assetName = assetName;
        m_loadRequest = AssetBundleManager.LoadAssetAsync(bundleName, assetName, assetType);
        if (m_loadRequest == null)
        {
            yield break;
        }

        OnBeginLoad();
        yield return m_loadRequest;

        if (string.IsNullOrEmpty(m_loadRequest.error))
        {
            OnLoaded(m_loadRequest.asset);
            m_needUnload = true;
        }
        else
        {
            Debug.LogError(m_loadRequest.error);
            OnLoaded(null);
        }

        m_loadRequest = null;
        m_coroutine = null;
        OnEndLoad();

        if (loadFinished != null)
        {
            loadFinished();
        }
    }

    protected abstract Type assetType { get; }

    protected virtual void OnBeginLoad() { }

    protected virtual void OnEndLoad() { }

    protected abstract void OnLoaded(UnityEngine.Object asset);

    bool AbortRequest()
    {
        if (m_loadRequest != null)
        {
            m_loadRequest.Dispose();
            m_loadRequest = null;
            return true;
        }
        return false;
    }

    protected void Unload()
    {
        AbortRequest();
        UnloadAssetBundle();
        m_assetName = m_bundleName = null;
    }

    private void UnloadAssetBundle()
    {
        if (m_needUnload)
        {
            AssetBundleManager.UnloadAssetBundle(m_bundleName, false);
            m_needUnload = false;
        }
    }
}
