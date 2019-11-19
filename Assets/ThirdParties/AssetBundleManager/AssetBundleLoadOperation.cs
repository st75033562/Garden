using UnityEngine;
#if UNITY_5_3_OR_NEWER
using UnityEngine.SceneManagement;
#endif
#if ENABLE_IOS_ON_DEMAND_RESOURCES
using UnityEngine.iOS;
#endif
using UnityEngine.Networking;
using System.Collections;

namespace AssetBundles
{
    public abstract class AssetBundleLoadOperation : IEnumerator
    {
        public object Current
        {
            get
            {
                return null;
            }
        }

        public bool MoveNext()
        {
            return !IsDone();
        }

        public void Reset()
        {
        }

        abstract public bool Update();

        abstract public bool IsDone();
    }

    public abstract class AssetBundleDownloadOperation : AssetBundleLoadOperation
    {
        bool done;

        public string assetBundleName { get; private set; }

        public string error { get; protected set; }

        public abstract float progress { get; }

        protected abstract bool downloadIsDone { get; }
        protected abstract void FinishDownload();

        public override bool Update()
        {
            if (!done && downloadIsDone)
            {
                FinishDownload();
                done = true;
            }

            return !done;
        }

        public override bool IsDone()
        {
            return done;
        }

        public abstract string GetSourceURL();

        public AssetBundleDownloadOperation(string assetBundleName, LoadedAssetBundle assetBundle)
        {
            this.assetBundleName = assetBundleName;
            this.assetBundle = assetBundle;
        }

        internal LoadedAssetBundle assetBundle { get; private set; }
    }

#if ENABLE_IOS_ON_DEMAND_RESOURCES
    // Read asset bundle asynchronously from iOS / tvOS asset catalog that is downloaded
    // using on demand resources functionality.
    public class AssetBundleDownloadFromODROperation : AssetBundleDownloadOperation
    {
        OnDemandResourcesRequest request;

        public AssetBundleDownloadFromODROperation(string assetBundleName, LoadedAssetBundle assetBundle)
            : base(assetBundleName, assetBundle)
        {
            // Work around Xcode crash when opening Resources tab when a 
            // resource name contains slash character
            request = OnDemandResources.PreloadAsync(new string[] { assetBundleName.Replace('/', '>') });
        }

        protected override bool downloadIsDone { get { return (request == null) || request.isDone; } }

        public override float progress
        {
            get
            {
                if (request != null)
                {
                    return request.progress;
                }
                return 1.0f;
            }
        }

        public override string GetSourceURL()
        {
            return "odr://" + assetBundleName;
        }

        protected override void FinishDownload()
        {
            error = request.error;
            if (error != null)
                return;

            var path = "res://" + assetBundleName;
            var bundle = AssetBundle.LoadFromFile(path);
            if (bundle == null)
            {
                error = string.Format("Failed to load {0}", path);
                request.Dispose();
            }
            else
            {
                // At the time of unload request is already set to null, so capture it to local variable.
                var localRequest = request;
                assetBundle.m_AssetBundle = bundle;
                // Dispose of request only when bundle is unloaded to keep the ODR pin alive.
                assetBundle.unload += () =>
                {
                    localRequest.Dispose();
                };
            }

            request = null;
        }
    }
#endif

#if ENABLE_IOS_APP_SLICING
    // Read asset bundle synchronously from an iOS / tvOS asset catalog
    public class AssetBundleOpenFromAssetCatalogOperation : AssetBundleDownloadOperation
    {
        public AssetBundleOpenFromAssetCatalogOperation(string assetBundleName, LoadedAssetBundle assetBundle)
            : base(assetBundleName, assetBundle)
        {
            var path = "res://" + assetBundleName;
            var bundle = AssetBundle.LoadFromFile(path);
            if (bundle == null)
                error = string.Format("Failed to load {0}", path);
            else
                assetBundle.m_AssetBundle = bundle;
        }

        public override float progress
        {
            get
            {
                return error != null ? 0.0f : 1.0f;
            }
        }

        protected override bool downloadIsDone { get { return true; } }

        protected override void FinishDownload() {}

        public override string GetSourceURL()
        {
            return "res://" + assetBundleName;
        }
    }
#endif

    public class AssetBundleDownloadFromWebOperation : AssetBundleDownloadOperation
    {
        string m_Url;
        TimedUnityWebRequest m_TimedRequest;

        public AssetBundleDownloadFromWebOperation(string assetBundleName, LoadedAssetBundle assetBundle, UnityWebRequest webRequest)
            : base(assetBundleName, assetBundle)
        {
            if (webRequest == null)
                throw new System.ArgumentNullException("webRequest");
            m_Url = webRequest.url;
            m_TimedRequest = new TimedUnityWebRequest(webRequest);
        }

        public override float progress
        {
            get
            {
                return m_TimedRequest != null ? Mathf.Max(0, m_TimedRequest.progress) : 0.0f;
            }
        }

        protected override bool downloadIsDone
        {
            get { return m_TimedRequest == null || m_TimedRequest.isDone; }
        }

        protected override void FinishDownload()
        {
            error = m_TimedRequest.error;
            if (!string.IsNullOrEmpty(error))
            {
                m_TimedRequest.Dispose();
                m_TimedRequest = null;
                return;
            }

            var bundle = (m_TimedRequest.rawRequest.downloadHandler as DownloadHandlerAssetBundle).assetBundle;
            if (bundle == null)
            {
                error = string.Format("{0} is not a valid asset bundle.", assetBundleName);
            }
            else
            {
                assetBundle.m_AssetBundle = bundle;
            }

            m_TimedRequest.Dispose();
            m_TimedRequest = null;
        }

        public override string GetSourceURL()
        {
            return m_Url;
        }
    }

#if UNITY_EDITOR
    public class AssetBundleLoadLevelSimulationOperation : AssetBundleLoadOperation
    {
        AsyncOperation m_Operation = null;

        public AssetBundleLoadLevelSimulationOperation(string assetBundleName, string levelName, bool isAdditive)
        {
            string[] levelPaths = UnityEditor.AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName(assetBundleName, levelName);
            if (levelPaths.Length == 0)
            {
                ///@TODO: The error needs to differentiate that an asset bundle name doesn't exist
                //        from that there right scene does not exist in the asset bundle...

                Debug.LogError("There is no scene with name \"" + levelName + "\" in " + assetBundleName);
                return;
            }

            if (isAdditive)
                m_Operation = UnityEditor.EditorApplication.LoadLevelAdditiveAsyncInPlayMode(levelPaths[0]);
            else
                m_Operation = UnityEditor.EditorApplication.LoadLevelAsyncInPlayMode(levelPaths[0]);
        }

        public override bool Update()
        {
            return false;
        }

        public override bool IsDone()
        {
            return m_Operation == null || m_Operation.isDone;
        }
    }
#endif

    public class AssetBundleLoadLevelOperation : AssetBundleLoadOperation
    {
        protected string                m_AssetBundleName;
        protected string                m_LevelName;
        protected bool                  m_IsAdditive;
        protected string                m_DownloadingError;
        protected AsyncOperation        m_Request;

        public AssetBundleLoadLevelOperation(string assetbundleName, string levelName, bool isAdditive)
        {
            m_AssetBundleName = assetbundleName;
            m_LevelName = levelName;
            m_IsAdditive = isAdditive;
        }

        public override bool Update()
        {
            if (m_Request != null)
                return false;

            var bundle = AssetBundleManager.GetLoadedAssetBundle(m_AssetBundleName, out m_DownloadingError);
            if (bundle != null)
            {
#if UNITY_5_3_OR_NEWER
                m_Request = SceneManager.LoadSceneAsync(m_LevelName, m_IsAdditive ? LoadSceneMode.Additive : LoadSceneMode.Single);
#else
                if (m_IsAdditive)
                    m_Request = Application.LoadLevelAdditiveAsync(m_LevelName);
                else
                    m_Request = Application.LoadLevelAsync(m_LevelName);
#endif
                return false;
            }
            else
                return m_DownloadingError == null;
        }

        public override bool IsDone()
        {
            // Return if meeting downloading error.
            // m_DownloadingError might come from the dependency downloading.
            if (m_Request == null && m_DownloadingError != null)
            {
                Debug.LogError(m_DownloadingError);
                return true;
            }

            return m_Request != null && m_Request.isDone;
        }

        public string error { get { return m_DownloadingError; } }
    }

    public abstract class AssetBundleLoadAssetOperation : AssetBundleLoadOperation
    {
        public T GetAsset<T>() where T: UnityEngine.Object
        {
            return (T)asset;
        }

        public abstract UnityEngine.Object asset { get; }

        // dispose the request and unload the bundle
        // NOTE: if you don't Dispose the request after it has finished successfully, you need to unload the bundle by hand.
        public abstract void Dispose();

        public abstract string error { get; }

        public bool isError { get { return !string.IsNullOrEmpty(error); } }
    }

    public class AssetBundleLoadAssetOperationSimulation : AssetBundleLoadAssetOperation
    {
        Object                          m_SimulatedObject;

        public AssetBundleLoadAssetOperationSimulation(Object simulatedObject)
        {
            m_SimulatedObject = simulatedObject;
        }

        public override Object asset
        {
            get { return m_SimulatedObject; }
        }

        public override bool Update()
        {
            return false;
        }

        public override bool IsDone()
        {
            return true;
        }

        public override void Dispose()
        {
        }

        public override string error
        {
            get { return null; }
        }
    }

    public class AssetBundleLoadAssetOperationFull : AssetBundleLoadAssetOperation
    {
        protected string                m_AssetBundleName;
        protected string                m_AssetName;
        protected string                m_DownloadingError;
        protected System.Type           m_Type;
        protected AssetBundleRequest    m_Request = null;
        private bool m_Disposed;

        public AssetBundleLoadAssetOperationFull(string bundleName, string assetName, System.Type type)
        {
            m_AssetBundleName = bundleName;
            m_AssetName = assetName;
            m_Type = type;
        }

        public override Object asset
        {
            get
            {
                if (m_Request != null && m_Request.isDone)
                {
                    return m_Request.asset;
                }
                else
                    return null;
            }
        }

        // Returns true if more Update calls are required.
        public override bool Update()
        {
            if (m_Request != null || m_Disposed)
                return false;

            var bundle = AssetBundleManager.GetLoadedAssetBundle(m_AssetBundleName, out m_DownloadingError);
            if (bundle != null)
            {
                ///@TODO: When asset bundle download fails this throws an exception...
                m_Request = bundle.m_AssetBundle.LoadAssetAsync(m_AssetName, m_Type);
                return false;
            }
            else
            {
                return m_DownloadingError == null;
            }
        }

        public override bool IsDone()
        {
            // Return if meeting downloading error.
            // m_DownloadingError might come from the dependency downloading.
            if (m_Request == null && m_DownloadingError != null)
            {
                Debug.LogError(m_DownloadingError);
                return true;
            }

            if (m_Disposed)
            {
                return true;
            }

            return m_Request != null && m_Request.isDone;
        }

        public override void Dispose()
        {
            if (m_Disposed) { return; }
            m_Disposed = true;

            if (string.IsNullOrEmpty(m_DownloadingError))
            {
                AssetBundleManager.UnloadAssetBundle(m_AssetBundleName);
                m_Request = null;
            }
        }

        public override string error { get { return m_DownloadingError; } }

        public override string ToString()
        {
            return string.Format("bundle: {0}, asset: {1}", m_AssetBundleName, m_AssetName);
        }
    }

    public class AssetBundleLoadManifestOperation : AssetBundleLoadAssetOperationFull
    {
        public AssetBundleLoadManifestOperation(string bundleName, string assetName, System.Type type)
            : base(bundleName, assetName, type)
        {
        }

        public override bool Update()
        {
            base.Update();

            if (m_Request != null && m_Request.isDone)
            {
                AssetBundleManager.AssetBundleManifestObject = GetAsset<AssetBundleManifest>();
                return false;
            }
            else
            {
                if (m_DownloadingError != null)
                {
                    Debug.LogError(m_DownloadingError);
                }
                return m_DownloadingError == null;
            }
        }
    }

    public class AssetBundleDownloadAllOperation : CustomYieldInstruction
    {
        public bool isDone { get { return !string.IsNullOrEmpty(error) || progress == 1.0f; } }

        public string error { get; internal set; }

        public float progress { get; internal set; }

        public override bool keepWaiting
        {
            get { return progress < 1.0f && string.IsNullOrEmpty(error); }
        }
    }

    public class AssetBundleLoadBundleOperation : AssetBundleLoadOperation, System.IDisposable
    {
        private bool m_disposed = false;
        private readonly string m_bundleName;
        private string m_error;
        private bool m_ownsAsset = true;
        private AssetBundle m_assetBundle;

        public AssetBundleLoadBundleOperation(string bundleName)
        {
            m_bundleName = bundleName;
        }

        public AssetBundle assetBundle
        {
            get
            {
                if (m_assetBundle)
                {
                    m_ownsAsset = false;
                }
                return m_assetBundle;
            }
        }

        public override bool Update()
        {
            LoadedAssetBundle bundle = AssetBundleManager.GetLoadedAssetBundle(m_bundleName, out m_error);
            if (bundle != null)
            {
                m_assetBundle = bundle.m_AssetBundle;
                return false;
            }
            else
            {
                return string.IsNullOrEmpty(m_error);
            }
        }

        public override bool IsDone()
        {
            return m_disposed || m_assetBundle != null || !string.IsNullOrEmpty(m_error);
        }

        public string error
        {
            get { return m_error; }
        }

        // abort the request if has not completed.
        public void Dispose()
        {
            if (m_disposed) { return; }
            m_disposed = true;

            // should not unload the bundle in case of error
            if (m_ownsAsset && string.IsNullOrEmpty(m_error))
            {
                AssetBundleManager.UnloadAssetBundle(m_bundleName);
            }
        }
    }
}
