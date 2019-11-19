using AssetBundles;
using DataAccess;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gameboard
{
    public class ObjectAssetCache
    {
        private static readonly A8.Logger s_logger = A8.Logger.GetLogger<ObjectAssetCache>();

        private class CacheRequest
        {
            public int assetId;
            public AssetBundleLoadAssetOperation operation;
        }

        public class CachedAsset
        {
            public readonly BundleAssetData asset;
            public GameObject objectAsset { get; internal set; }

            public CachedAsset(BundleAssetData asset)
            {
                this.asset = asset;
            }
        }

        private readonly Dictionary<int, CachedAsset> m_cachedAssets = new Dictionary<int, CachedAsset>();
        private readonly List<CacheRequest> m_cacheRequests = new List<CacheRequest>();

        public void Cache(IEnumerable<BundleAssetData> assets)
        {
            if (assets == null)
            {
                throw new ArgumentNullException("assets");
            }

            foreach (var asset in assets)
            {
                if (!m_cachedAssets.ContainsKey(asset.id))
                {
                    m_cachedAssets.Add(asset.id, new CachedAsset(asset));

                    var op = AssetBundleManager.LoadAssetAsync(asset.bundleName, asset.assetName, typeof(GameObject));
                    var request = new CacheRequest {
                        assetId = asset.id,
                        operation = op
                    };
                    m_cacheRequests.Add(request);

                    s_logger.Log("caching " + asset.assetName);
                }
            }
        }

        public void Evict(IEnumerable<BundleAssetData> assets)
        {
            if (assets == null)
            {
                throw new ArgumentNullException("assets");
            }

            foreach (var asset in assets)
            {
                if (m_cachedAssets.Remove(asset.id))
                {
                    var requestIndex = m_cacheRequests.FindIndex(x => x.assetId == asset.id);
                    if (requestIndex != -1)
                    {
                        m_cacheRequests[requestIndex].operation.Dispose();
                        m_cacheRequests.RemoveAt(requestIndex);
                    }
                    else
                    {
                        AssetBundleManager.UnloadAssetBundle(asset.bundleName);
                    }

                    s_logger.Log("evict " + asset.assetName);
                }
            }
        }

        // return the cached asset if any
        // if the asset is not ready, return null
        public CachedAsset GetAsset(int assetId)
        {
            CachedAsset cachedAsset;
            m_cachedAssets.TryGetValue(assetId, out cachedAsset);
            return cachedAsset != null && cachedAsset.objectAsset ? cachedAsset : null;
        }

        // true if all assets are cached
        public bool isReady
        {
            get { return m_cacheRequests.Count == 0; }
        }

        public void Update()
        {
            for (int i = 0; i < m_cacheRequests.Count; ++i)
            {
                var request = m_cacheRequests[i];
                if (request.operation.IsDone())
                {
                    m_cacheRequests.RemoveAt(i);
                    --i;

                    if (!request.operation.isError)
                    {
                        var cachedAsset = m_cachedAssets[request.assetId];
                        cachedAsset.objectAsset = request.operation.GetAsset<GameObject>();
                        if (cachedAsset.objectAsset)
                        {
                            s_logger.Log("cached " + cachedAsset.asset.assetName);
                        }
                        else
                        {
                            // should not happen
                            s_logger.LogError("invalid asset id: " + request.assetId);
                            AssetBundleManager.UnloadAssetBundle(cachedAsset.asset.bundleName);
                        }
                    }
                    else
                    {
                        m_cachedAssets.Remove(request.assetId);
                    }
                }
            }
        }

        public void Uninitialize()
        {
            foreach (var entry in m_cachedAssets.Values)
            {
                if (entry.objectAsset)
                {
                    AssetBundleManager.UnloadAssetBundle(entry.asset.bundleName);
                }
            }
            m_cachedAssets.Clear();

            foreach (var request in m_cacheRequests)
            {
                request.operation.Dispose();
            }
            m_cachedAssets.Clear();
        }
    }
}
