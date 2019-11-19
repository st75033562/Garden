using AssetBundles;
using DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Gameboard
{
    class InvalidObjectRequest : AsyncRequest<int>
    {
        private InvalidObjectRequest()
        {
            SetCompleted();
        }

        protected override int GetResult()
        {
            return 0;
        }

        public static readonly InvalidObjectRequest instance = new InvalidObjectRequest();
    }

    class CachedObjectCreateRequest : AsyncRequest<int>
    {
        private int m_objectId;

        public CachedObjectCreateRequest(int objectId)
        {
            m_objectId = objectId;
            SetCompleted();
        }

        protected override int GetResult()
        {
            return m_objectId;
        }
    }

    class CreateObjectRequest : AsyncRequest<int>
    {
        private enum State
        {
            Loading,
            Loaded,
            Disposed
        }

        private readonly ObjectFactory m_factory;
        private readonly ObjectCreateInfo m_createInfo;

        private AssetBundleLoadAssetOperation m_loadRequest;
        private int m_objectId;
        private State m_state = State.Loading;
        private BundleAssetData m_asset;

        public CreateObjectRequest(ObjectFactory factory, BundleAssetData asset, ObjectCreateInfo info)
        {
            if (factory == null)
            {
                throw new ArgumentNullException("factory");
            }
            if (asset == null)
            {
                throw new ArgumentNullException("asset");
            }
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }

            m_factory = factory;

            m_createInfo = info;
            m_asset = asset;
            m_loadRequest = AssetBundleManager.LoadAssetAsync(
                asset.bundleName, asset.assetName, typeof(GameObject));
        }

        protected override int GetResult()
        {
            return m_objectId;
        }

        public bool Update()
        {
            if (m_loadRequest == null)
            {
                return false;
            }

            if (m_loadRequest.IsDone())
            {
                if (m_state == State.Loading)
                {
                    var template = m_loadRequest.GetAsset<GameObject>();
                    if (template)
                    {
                        m_objectId = m_factory.InternalCreateEntity(m_asset, template, m_createInfo);
                    }
                    else
                    {
                        ObjectFactory.s_logger.LogError("failed to load asset for " + m_asset.id);
                        m_loadRequest.Dispose();
                    }
                    m_loadRequest = null;
                    m_state = State.Loaded;
                    SetCompleted();
                }

                return false;
            }
            return true;
        }

        public override void Dispose()
        {
            if (m_loadRequest != null)
            {
                m_loadRequest.Dispose();
                m_loadRequest = null;
            }

            if (m_state == State.Loading)
            {
                m_state = State.Disposed;
                base.Dispose();
            }
        }
    }

    public class ObjectCreateInfo
    {
        public int assetId;
        public bool active = true;

        public string name = "";
        public int objectId;

        public Vector3 position = Vector3.right * 10000;
        public Vector3 rotation;
        public Vector3 localScale = Vector3.one;

        public ObjectCreateInfo() { }

        public ObjectCreateInfo(ObjectInfo info)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }

            assetId = info.assetId;
            name = info.name;
            position = info.position;
            rotation = info.rotation;
            localScale = info.scale;
        }
    }

    public class ObjectFactory
    {
        internal static readonly A8.Logger s_logger = A8.Logger.GetLogger<ObjectFactory>();

        private readonly GameboardSceneManager m_sceneManager;
        private readonly List<CreateObjectRequest> m_requests = new List<CreateObjectRequest>();
        private readonly ObjectAssetCache m_cache = new ObjectAssetCache();

        public ObjectFactory(GameboardSceneManager sceneManager)
        {
            if (sceneManager == null)
            {
                throw new ArgumentNullException();
            }
            m_sceneManager = sceneManager;
        }

        public void Cache(IEnumerable<BundleAssetData> assets)
        {
            m_cache.Cache(assets);
        }

        public void Evict(IEnumerable<BundleAssetData> assets)
        {
            m_cache.Evict(assets);
        }

        public bool inEditor
        {
            get;
            set;
        }

        // true if all assets are cached
        public bool isCacheReady
        {
            get { return m_cache.isReady; }
        }

        public AsyncRequest<int> Create(ObjectCreateInfo info)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }

            var cachedAsset = m_cache.GetAsset(info.assetId);
            if (cachedAsset != null)
            {
                var entity = InternalCreateEntity(BundleAssetData.Get(cachedAsset.asset.id), cachedAsset.objectAsset, info);
                return new CachedObjectCreateRequest(entity);
            }

            var asset = BundleAssetData.Get(info.assetId);
            if (asset == null)
            {
                s_logger.LogError("invalid asset id: " + info.assetId);
                return InvalidObjectRequest.instance;
            }

            var request = new CreateObjectRequest(this, asset, info);
            m_requests.Add(request);
            return request;
        }

        internal int InternalCreateEntity(BundleAssetData asset, GameObject template, ObjectCreateInfo info)
        {
            var sceneRoot = asset.threeD ? m_sceneManager.sceneRoot : m_sceneManager.uiRoot;
            var obj = GameObject.Instantiate(template, info.position, Quaternion.Euler(info.rotation), sceneRoot);
            var entity = obj.GetComponent<Entity>();

            entity.id = info.objectId;
            entity.asset = asset;
            entity.entityName = info.name;
            entity.sceneRoot = sceneRoot;
            entity.transform.localScale = info.localScale;

            if (entity.asset.runtimeCollision)
            {
                var collisionEvent = entity.GetComponent<EntityCollisionEvent>();
                if (!collisionEvent)
                {
                    collisionEvent = entity.gameObject.AddComponent<EntityCollisionEvent>();
                }
            }
            else if (!inEditor)
            {
                foreach (var collider in entity.GetComponentsInChildren<Collider>())
                {
                    UnityEngine.Object.DestroyImmediate(collider);
                }
            }

            // for non-rigid body entities, in order to receive collision events from child colliders,
            // we need to add a helper script
            if (!entity.GetComponent<Rigidbody>())
            {
                foreach (var source in entity.GetComponentsInChildren<Collider>())
                {
                    // no need to forward events for the entity object
                    if (source.gameObject != entity.gameObject)
                    {
                        source.gameObject.AddComponent<CollisionEventSource>();
                    }
                }
            }

            entity.Initialize();
            entity.positional.Synchornize();

            m_sceneManager.objectManager.Register(entity, info.active);
            return entity.id;
        }

        public void Update()
        {
            for (int i = 0; i < m_requests.Count; ++i)
            {
                var request = m_requests[i];
                if (!request.Update())
                {
                    m_requests.RemoveAt(i);
                    --i;
                }
            }

            m_cache.Update();
        }

        public void RemoveAllRequests()
        {
            foreach (var request in m_requests)
            {
                request.Dispose();
            }
            m_requests.Clear();
        }

        public void Uninitialize()
        {
            RemoveAllRequests();
            m_cache.Uninitialize();
        }
    }
}
