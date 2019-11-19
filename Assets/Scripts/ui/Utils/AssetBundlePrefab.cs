using System;
using AssetBundles;
using UnityEngine;

public class AssetBundlePrefab : AssetBundleObject {
    public Transform anchor;

    private GameObject m_instance;

    protected override void OnDestroy() {
        base.OnDestroy();

        if(m_instance) {
            Destroy(m_instance);
        }
    }

    protected override Type assetType {
        get { return typeof(GameObject); }
    }

    protected override void OnLoaded(UnityEngine.Object asset) {
        if(m_instance) {
            Destroy(m_instance);
            m_instance = null;
        }

        if(asset) {
            m_instance = Instantiate(asset as GameObject, anchor.position, anchor.rotation, anchor);
        }
    }

    public GameObject GetInstance() {
        return m_instance;
    }
}
