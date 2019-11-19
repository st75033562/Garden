using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Networking;
using UnityEngine.Assertions;

/*  The AssetBundle Manager provides a High-Level API for working with AssetBundles. 
    The AssetBundle Manager will take care of loading AssetBundles and their associated 
    Asset Dependencies.
        Initialize()
            Initializes the AssetBundle manifest object.
        LoadAssetAsync()
            Loads a given asset from a given AssetBundle and handles all the dependencies.
        LoadLevelAsync()
            Loads a given scene from a given AssetBundle and handles all the dependencies.
        LoadDependencies()
            Loads all the dependent AssetBundles for a given AssetBundle.
        BaseDownloadingURL
            Sets the base downloading url which is used for automatic downloading dependencies.
        SimulateAssetBundleInEditor
            Sets Simulation Mode in the Editor.
        Variants
            Sets the active variant.
        RemapVariantName()
            Resolves the correct AssetBundle according to the active variant.
*/

namespace AssetBundles
{
    /// <summary>
    /// Loaded assetBundle contains the references count which can be used to
    /// unload dependent assetBundles automatically.
    /// </summary>
    public class LoadedAssetBundle
    {
        public AssetBundle m_AssetBundle;
        public int m_ReferencedCount;

        internal event Action unload;

        internal void OnUnload()
        {
            m_AssetBundle.Unload(true);
            if (unload != null)
                unload();
        }

        public LoadedAssetBundle()
        {
            m_ReferencedCount = 1;
        }
    }

    /// <summary>
    /// Class takes care of loading assetBundle and its dependencies
    /// automatically, loading variants automatically.
    /// </summary>
    public class AssetBundleManager : MonoBehaviour
    {
        public enum LogMode { All, JustErrors };
        public enum LogType { Info, Warning, Error };

        static AssetBundleManager s_Instance;
        static LogMode m_LogMode = LogMode.All;
        static string m_BaseDownloadingURL = "";
        static string[] m_ActiveVariants =  {};
        static AssetBundleManifest m_AssetBundleManifest = null;
        static string m_ManifestName;

#if UNITY_EDITOR
        static int m_SimulateAssetBundleInEditor = -1;
        const string kSimulateAssetBundles = "SimulateAssetBundles";
        const string kLocalAssetBundleServer = "LocalAssetBundleServer";
#endif

        static Dictionary<string, LoadedAssetBundle> m_LoadedAssetBundles = new Dictionary<string, LoadedAssetBundle>();

        static Dictionary<string, string> m_DownloadingErrors = new Dictionary<string, string>();
        static Dictionary<string, LoadedAssetBundle> m_DownloadingBundles = new Dictionary<string, LoadedAssetBundle>();
        static List<AssetBundleLoadOperation> m_InProgressOperations = new List<AssetBundleLoadOperation>();
        static Dictionary<string, string[]> m_Dependencies = new Dictionary<string, string[]>();
        static string[] m_VariantBundles;

        //public static event Action OnUnloadAssetBundle

        void Awake()
        {
            s_Instance = this;
        }

        void OnDestroy()
        {
            s_Instance = null;
        }

        public static LogMode logMode
        {
            get { return m_LogMode; }
            set { m_LogMode = value; }
        }

        /// <summary>
        /// The base downloading url which is used to generate the full
        /// downloading url with the assetBundle names.
        /// </summary>
        public static string BaseDownloadingURL
        {
            get { return m_BaseDownloadingURL; }
            set { m_BaseDownloadingURL = value; }
        }

        public delegate string OverrideBaseDownloadingURLDelegate(string bundleName);

        /// <summary>
        /// Implements per-bundle base downloading URL override.
        /// The subscribers must return null values for unknown bundle names;
        /// </summary>
        public static event OverrideBaseDownloadingURLDelegate overrideBaseDownloadingURL;

        /// <summary>
        /// Variants which is used to define the active variants.
        /// </summary>
        public static string[] ActiveVariants
        {
            get { return m_ActiveVariants; }
            set { m_ActiveVariants = value; }
        }

        /// <summary>
        /// AssetBundleManifest object which can be used to load the dependecies
        /// and check suitable assetBundle variants.
        /// </summary>
        public static AssetBundleManifest AssetBundleManifestObject
        {
            set
            {
                m_AssetBundleManifest = value;
                m_VariantBundles = m_AssetBundleManifest.GetAllAssetBundlesWithVariant();
            }
            get { return m_AssetBundleManifest; }
        }

        public static bool IsInitialized
        {
            get
            {
#if UNITY_EDITOR
                if (SimulateAssetBundleInEditor)
                {
                    return true;
                }
#endif
                return m_AssetBundleManifest != null;
            }
        }

#if UNITY_EDITOR
        // for debugging
        public static Dictionary<string, LoadedAssetBundle> LoadedAssetBundles { get { return m_LoadedAssetBundles; } }
        public static Dictionary<string, LoadedAssetBundle> DownloadingAssetBundles { get { return m_DownloadingBundles; } }
        public static Dictionary<string, string[]> Dependencies { get { return m_Dependencies; } }
        public static IEnumerable<AssetBundleLoadOperation> InProgressOperations { get { return m_InProgressOperations; } }
#endif

        private static void Log(LogType logType, string text)
        {
            if (logType == LogType.Error)
                Debug.LogError("[AssetBundleManager] " + text);
            else if (m_LogMode == LogMode.All && logType == LogType.Warning)
                Debug.LogWarning("[AssetBundleManager] " + text);
            else if (m_LogMode == LogMode.All)
                Debug.Log("[AssetBundleManager] " + text);
        }

        /// <summary>
        /// Flag to indicate if we want to simulate assetBundles in Editor without building them actually.
        /// </summary>
        public static bool SimulateAssetBundleInEditor
        {
            get
            {
#if UNITY_EDITOR
                if (m_SimulateAssetBundleInEditor == -1)
                    m_SimulateAssetBundleInEditor = EditorPrefs.GetBool(kSimulateAssetBundles, true) ? 1 : 0;

                return m_SimulateAssetBundleInEditor != 0;
#else
                return false;
#endif
            }
#if UNITY_EDITOR
            set
            {
                int newValue = value ? 1 : 0;
                if (newValue != m_SimulateAssetBundleInEditor)
                {
                    m_SimulateAssetBundleInEditor = newValue;
                    EditorPrefs.SetBool(kSimulateAssetBundles, value);
                }
            }
#endif
        }

        public static bool LocalAssetBundleServer
        {
            get
            {
#if UNITY_EDITOR
                return EditorPrefs.GetBool(kLocalAssetBundleServer, false);
#else
                return false;
#endif
            }
#if UNITY_EDITOR
            set { EditorPrefs.SetBool(kLocalAssetBundleServer, value); }
#endif
        }

        private static string GetStreamingAssetsPath()
        {
            if (Application.isEditor)
                return "file://" +  System.Environment.CurrentDirectory.Replace("\\", "/"); // Use the build output folder directly.
            else if (Application.isWebPlayer)
                return System.IO.Path.GetDirectoryName(Application.absoluteURL).Replace("\\", "/") + "/StreamingAssets";
            else if (Application.isMobilePlatform || Application.isConsolePlatform)
                return Application.streamingAssetsPath;
            else // For standalone player.
                return "file://" +  Application.streamingAssetsPath;
        }

        /// <summary>
        /// Sets base downloading URL to a directory relative to the streaming assets directory.
        /// Asset bundles are loaded from a local directory.
        /// </summary>
        public static void SetSourceAssetBundleDirectory(string relativePath)
        {
            BaseDownloadingURL = GetStreamingAssetsPath() + relativePath;
        }

        /// <summary>
        /// Sets base downloading URL to a web URL. The directory pointed to by this URL
        /// on the web-server should have the same structure as the AssetBundles directory
        /// in the demo project root.
        /// </summary>
        /// <example>For example, AssetBundles/iOS/xyz-scene must map to
        /// absolutePath/iOS/xyz-scene.
        /// <example>
        public static void SetSourceAssetBundleURL(string absolutePath)
        {
            if (!absolutePath.EndsWith("/"))
            {
                absolutePath += "/";
            }

            BaseDownloadingURL = absolutePath + Utility.GetPlatformName() + "/";
        }

        /// <summary>
        /// Sets base downloading URL to a local development server URL.
        /// </summary>
        public static void SetDevelopmentAssetBundleServer()
        {
#if UNITY_EDITOR
            // If we're in Editor simulation mode, we don't have to setup a download URL
            if (SimulateAssetBundleInEditor)
                return;
#endif

            TextAsset urlFile = Resources.Load("AssetBundleServerURL") as TextAsset;
            string url = (urlFile != null) ? urlFile.text.Trim() : null;
            if (url == null || url.Length == 0)
            {
                Log(LogType.Error, "Development Server URL could not be found.");
            }
            else
            {
                AssetBundleManager.SetSourceAssetBundleURL(url);
            }
        }

        /// <summary>
        /// Retrieves an asset bundle that has previously been requested via LoadAssetBundle.
        /// Returns null if the asset bundle or one of its dependencies have not been downloaded yet.
        /// </summary>
        static public LoadedAssetBundle GetLoadedAssetBundle(string assetBundleName, out string error)
        {
            if (m_DownloadingErrors.TryGetValue(assetBundleName, out error))
                return null;

            LoadedAssetBundle bundle = null;
            m_LoadedAssetBundles.TryGetValue(assetBundleName, out bundle);
            if (bundle == null)
                return null;

            // No dependencies are recorded, only the bundle itself is required.
            string[] dependencies = null;
            if (!m_Dependencies.TryGetValue(assetBundleName, out dependencies))
            {
                return bundle;
            }

            // Make sure all dependencies are loaded
            foreach (var dependency in dependencies)
            {
                if (m_DownloadingErrors.TryGetValue(dependency, out error))
                    return null;

                // Wait all the dependent assetBundles being loaded.
                LoadedAssetBundle dependentBundle;
                m_LoadedAssetBundles.TryGetValue(dependency, out dependentBundle);
                if (dependentBundle == null)
                    return null;
            }

            return bundle;
        }

        /// <summary>
        /// Returns true if certain asset bundle has been downloaded without checking
        /// whether the dependencies have been loaded.
        /// </summary>
        static public bool IsAssetBundleDownloaded(string assetBundleName)
        {
            return m_LoadedAssetBundles.ContainsKey(assetBundleName);
        }

        /// <summary>
        /// Initializes asset bundle namager and starts download of manifest asset bundle.
        /// Returns the manifest asset bundle downolad operation object.
        /// </summary>
        static public AssetBundleLoadManifestOperation Initialize()
        {
            return Initialize(Utility.GetPlatformName());
        }

        /// <summary>
        /// Initializes asset bundle namager and starts download of manifest asset bundle.
        /// Returns the manifest asset bundle downolad operation object.
        /// </summary>
        static public AssetBundleLoadManifestOperation Initialize(string manifestAssetBundleName)
        {
#if UNITY_EDITOR
            Log(LogType.Info, "Simulation Mode: " + (SimulateAssetBundleInEditor ? "Enabled" : "Disabled"));
#endif

            var go = new GameObject("AssetBundleManager", typeof(AssetBundleManager));
            DontDestroyOnLoad(go);

#if UNITY_EDITOR
            // If we're in Editor simulation mode, we don't need the manifest assetBundle.
            if (SimulateAssetBundleInEditor)
            {
                // we assume all bundles with . are variants
                m_VariantBundles = AssetDatabase.GetAllAssetBundleNames().Where(x => x.Contains('.')).ToArray();
                return null;
            }
#endif

            m_ManifestName = manifestAssetBundleName;
            LoadAssetBundle(manifestAssetBundleName, true);
            var operation = new AssetBundleLoadManifestOperation(manifestAssetBundleName, "AssetBundleManifest", typeof(AssetBundleManifest));
            m_InProgressOperations.Add(operation);
            return operation;
        }

        // Temporarily work around a il2cpp bug
        static protected void LoadAssetBundle(string assetBundleName)
        {
            LoadAssetBundle(assetBundleName, false);
        }
            
        // Starts the download of the asset bundle identified by the given name, and asset bundles
        // that this asset bundle depends on.
        static protected void LoadAssetBundle(string assetBundleName, bool isLoadingAssetBundleManifest)
        {
            Log(LogType.Info, "Loading Asset Bundle " + (isLoadingAssetBundleManifest ? "Manifest: " : ": ") + assetBundleName);

#if UNITY_EDITOR
            // If we're in Editor simulation mode, we don't have to really load the assetBundle and its dependencies.
            if (SimulateAssetBundleInEditor)
                return;
#endif

            if (!isLoadingAssetBundleManifest)
            {
                if (m_AssetBundleManifest == null)
                {
                    Log(LogType.Error, "Please initialize AssetBundleManifest by calling AssetBundleManager.Initialize()");
                    return;
                }
            }

            LoadAssetBundleInternal(assetBundleName, isLoadingAssetBundleManifest);

            // Load dependencies.
            if (!isLoadingAssetBundleManifest)
                LoadDependencies(assetBundleName);
        }

        // Returns base downloading URL for the given asset bundle.
        // This URL may be overridden on per-bundle basis via overrideBaseDownloadingURL event.
        protected static string GetAssetBundleBaseDownloadingURL(string bundleName)
        {
            if (overrideBaseDownloadingURL != null)
            {
                foreach (OverrideBaseDownloadingURLDelegate method in overrideBaseDownloadingURL.GetInvocationList())
                {
                    string res = method(bundleName);
                    if (res != null)
                        return res;
                }
            }
            return m_BaseDownloadingURL;
        }

        // Checks who is responsible for determination of the correct asset bundle variant
        // that should be loaded on this platform. 
        //
        // On most platforms, this is done by the AssetBundleManager itself. However, on
        // certain platforms (iOS at the moment) it's possible that an external asset bundle
        // variant resolution mechanism is used. In these cases, we use base asset bundle 
        // name (without the variant tag) as the bundle identifier. The platform-specific 
        // code is responsible for correctly loading the bundle.
        static protected bool UsesExternalBundleVariantResolutionMechanism(string baseAssetBundleName)
        {
#if ENABLE_IOS_APP_SLICING
            var url = GetAssetBundleBaseDownloadingURL(baseAssetBundleName);
            if (url.ToLower().StartsWith("res://") ||
                url.ToLower().StartsWith("odr://"))
                return true;
#endif
            return false;
        }

        // Remaps the asset bundle name to the best fitting asset bundle variant.
        static protected string RemapVariantName(string assetBundleName)
        {
            // Get base bundle name
            string baseName = assetBundleName.Split('.')[0];

            if (UsesExternalBundleVariantResolutionMechanism(baseName))
                return baseName;

            int bestFit = int.MaxValue;
            int bestFitIndex = -1;
            // Loop all the assetBundles with variant to find the best fit variant assetBundle.
            for (int i = 0; i < m_VariantBundles.Length; i++)
            {
                string[] curSplit = m_VariantBundles[i].Split('.');
                string curBaseName = curSplit[0];
                string curVariant = curSplit[1];

                if (curBaseName != baseName)
                    continue;

                int found = System.Array.IndexOf(m_ActiveVariants, curVariant);

                // If there is no active variant found. We still want to use the first
                if (found == -1)
                    found = int.MaxValue - 1;

                if (found < bestFit)
                {
                    bestFit = found;
                    bestFitIndex = i;
                }
            }

            if (bestFit == int.MaxValue - 1)
            {
                Log(LogType.Warning, "Ambigious asset bundle variant chosen because there was no matching active variant: " + m_VariantBundles[bestFitIndex]);
            }

            if (bestFitIndex != -1)
            {
                return m_VariantBundles[bestFitIndex];
            }
            else
            {
                return assetBundleName;
            }
        }

        // Sets up download operation for the given asset bundle if it's not downloaded already.
        static protected bool LoadAssetBundleInternal(string assetBundleName, bool isLoadingAssetBundleManifest)
        {
            // Already loaded.
            LoadedAssetBundle bundle = null;
            m_LoadedAssetBundles.TryGetValue(assetBundleName, out bundle);
            if (bundle != null)
            {
                bundle.m_ReferencedCount++;
                return true;
            }

            if (m_DownloadingBundles.TryGetValue(assetBundleName, out bundle))
            {
                bundle.m_ReferencedCount++;
                return true;
            }

            bundle = new LoadedAssetBundle();
            string bundleBaseDownloadingURL = GetAssetBundleBaseDownloadingURL(assetBundleName);

            if (bundleBaseDownloadingURL.ToLower().StartsWith("odr://"))
            {
#if ENABLE_IOS_ON_DEMAND_RESOURCES
                Log(LogType.Info, "Requesting bundle " + assetBundleName + " through ODR");
                m_InProgressOperations.Add(new AssetBundleDownloadFromODROperation(assetBundleName, bundle));
#else
                new ApplicationException("Can't load bundle " + assetBundleName + " through ODR: this Unity version or build target doesn't support it.");
#endif
            }
            else if (bundleBaseDownloadingURL.ToLower().StartsWith("res://"))
            {
#if ENABLE_IOS_APP_SLICING
                Log(LogType.Info, "Requesting bundle " + assetBundleName + " through asset catalog");
                m_InProgressOperations.Add(new AssetBundleOpenFromAssetCatalogOperation(assetBundleName, bundle));
#else
                new ApplicationException("Can't load bundle " + assetBundleName + " through asset catalog: this Unity version or build target doesn't support it.");
#endif
            }
            else
            {
                UnityWebRequest download = null;

                if (!bundleBaseDownloadingURL.EndsWith("/"))
                {
                    bundleBaseDownloadingURL += "/";
                }

                string url = bundleBaseDownloadingURL + assetBundleName;

                // For manifest assetbundle, always download it as we don't have hash for it.
                if (isLoadingAssetBundleManifest)
                    download = UnityWebRequest.GetAssetBundle(url);
                else
                    download = UnityWebRequest.GetAssetBundle(url, m_AssetBundleManifest.GetAssetBundleHash(assetBundleName), 0);

                m_InProgressOperations.Add(new AssetBundleDownloadFromWebOperation(assetBundleName, bundle, download));
            }
            m_DownloadingErrors.Remove(assetBundleName);
            m_DownloadingBundles.Add(assetBundleName, bundle);

            return false;
        }

        // Where we get all the dependencies and load them all.
        static protected void LoadDependencies(string assetBundleName)
        {
            if (m_AssetBundleManifest == null)
            {
                Log(LogType.Error, "Please initialize AssetBundleManifest by calling AssetBundleManager.Initialize()");
                return;
            }

            if (m_Dependencies.ContainsKey(assetBundleName))
            {
                return;
            }

            // Get dependecies from the AssetBundleManifest object..
            string[] dependencies = m_AssetBundleManifest.GetAllDependencies(assetBundleName);
            if (dependencies.Length == 0)
                return;

            for (int i = 0; i < dependencies.Length; i++)
                dependencies[i] = RemapVariantName(dependencies[i]);

            // Record and load all dependencies.
            m_Dependencies.Add(assetBundleName, dependencies);
            for (int i = 0; i < dependencies.Length; i++)
                LoadAssetBundleInternal(dependencies[i], false);
        }

        /// <summary>
        /// Unloads assetbundle and its dependencies.
        /// </summary>
        static public void UnloadAssetBundle(string assetBundleName, bool immediate = true)
        {
#if UNITY_EDITOR
            // If we're in Editor simulation mode, we don't have to load the manifest assetBundle.
            if (SimulateAssetBundleInEditor)
                return;
#endif
            assetBundleName = RemapVariantName(assetBundleName);
            UnloadAssetBundleWithVariant(assetBundleName, immediate);
        }

        static protected void UnloadAssetBundleWithVariant(string assetBundleName, bool immediate)
        {
            if (UnloadAssetBundleInternal(assetBundleName, immediate))
            {
                UnloadDependencies(assetBundleName, immediate);
            }
        }

        static public void UnloadUnusedBundles()
        {
            var unusedBundles = m_LoadedAssetBundles.Where(x => x.Value.m_ReferencedCount == 0).ToArray();
            foreach (var bundle in unusedBundles)
            {
                bundle.Value.OnUnload();
                m_LoadedAssetBundles.Remove(bundle.Key);

                Log(LogType.Info, "Unloaded asset bundle " + bundle.Key);
            }
        }

        static public bool hasPendingOperations
        {
            get { return m_InProgressOperations.Count > 0; }
        }

        static public void UnloadVariantBundles()
        {
            var bundles = m_LoadedAssetBundles.Keys.Where(x => Array.IndexOf(m_VariantBundles, x) != -1).ToArray();
            foreach (var bundle in bundles)
            {
                UnloadAssetBundleWithVariant(bundle, true);
            }
        }

        /// <summary>
        /// unload all loaded asset bundles except the manifest
        /// </summary>
        static public void UnloadAllAssetBundles()
        {
            var bundles = m_LoadedAssetBundles.Keys.Where(x => x != m_ManifestName).ToArray();
            foreach (var bundle in bundles)
            {
                UnloadAssetBundleWithVariant(bundle, true);
            }
        }

        static protected void UnloadDependencies(string assetBundleName, bool immediate)
        {
            string[] dependencies = null;
            if (!m_Dependencies.TryGetValue(assetBundleName, out dependencies))
                return;

            // Loop dependencies.
            foreach (var dependency in dependencies)
            {
                UnloadAssetBundleInternal(dependency, immediate);
            }

            m_Dependencies.Remove(assetBundleName);
        }

        static protected bool UnloadAssetBundleInternal(string assetBundleName, bool immediate)
        {
            LoadedAssetBundle bundle;
            m_LoadedAssetBundles.TryGetValue(assetBundleName, out bundle);
            if (bundle == null)
            {
                m_DownloadingBundles.TryGetValue(assetBundleName, out bundle);
            }
            if (bundle == null)
            {
                return false;
            }

            if (bundle.m_ReferencedCount > 0)
            {
                --bundle.m_ReferencedCount;
            }

            if (bundle.m_ReferencedCount == 0)
            {
                if (immediate)
                {
                    Log(LogType.Info, assetBundleName + " has been unloaded successfully");

                    // if null, the bundle is still being downloaded
                    if (bundle.m_AssetBundle != null)
                    {
                        bundle.OnUnload();
                        m_LoadedAssetBundles.Remove(assetBundleName);
                    }
                }
                return true;
            }

            return false;
        }

        void Update()
        {
            // Update all in progress operations
            for (int i = 0; i < m_InProgressOperations.Count;)
            {
                var operation = m_InProgressOperations[i];
                if (operation.Update())
                {
                    i++;
                }
                else
                {
                    m_InProgressOperations.RemoveAt(i);
                    ProcessFinishedOperation(operation);
                }
            }
        }

        void ProcessFinishedOperation(AssetBundleLoadOperation operation)
        {
            AssetBundleDownloadOperation download = operation as AssetBundleDownloadOperation;
            if (download == null)
                return;

            if (string.IsNullOrEmpty(download.error))
            {
                m_LoadedAssetBundles.Add(download.assetBundleName, download.assetBundle);
            }
            else
            {
                string msg = string.Format("Failed downloading bundle {0} from {1}: {2}",
                        download.assetBundleName, download.GetSourceURL(), download.error);
                m_DownloadingErrors.Add(download.assetBundleName, msg);

                // need to unload all dependencies
                UnloadDependencies(download.assetBundleName, true);
            }

            m_DownloadingBundles.Remove(download.assetBundleName);
        }

        /// <summary>
        /// load the given asset bundle
        /// </summary>
        /// <param name="assetBundleName"></param>
        /// <returns></returns>
        static public AssetBundleLoadBundleOperation LoadAssetBundleAsync(string assetBundleName)
        {
            Log(LogType.Info, "Loading " + assetBundleName + " bundle");

            LoadAssetBundle(assetBundleName);

            var op = new AssetBundleLoadBundleOperation(assetBundleName);
            m_InProgressOperations.Add(op);
            return op;
        }

        /// <summary>
        /// Starts a load operation for an asset from the given asset bundle.
        /// </summary>
        static public AssetBundleLoadAssetOperation LoadAssetAsync(string assetBundleName, string assetName, System.Type type)
        {
            Log(LogType.Info, "Loading " + assetName + " from " + assetBundleName + " bundle");

            AssetBundleLoadAssetOperation operation = null;
            assetBundleName = RemapVariantName(assetBundleName);
#if UNITY_EDITOR
            if (SimulateAssetBundleInEditor)
            {
                string[] assetPaths = AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName(assetBundleName, assetName);
                if (assetPaths.Length == 0)
                {
                    Log(LogType.Error, "There is no asset with name \"" + assetName + "\" in " + assetBundleName);
                    return null;
                }

                // @TODO: Now we only get the main object from the first asset. Should consider type also.
                UnityEngine.Object target = AssetDatabase.LoadAssetAtPath(assetPaths[0], type);
                operation = new AssetBundleLoadAssetOperationSimulation(target);
            }
            else
#endif
            {
                LoadAssetBundle(assetBundleName);
                operation = new AssetBundleLoadAssetOperationFull(assetBundleName, assetName, type);

                m_InProgressOperations.Add(operation);
            }

            return operation;
        }

        /// <summary>
        /// Starts a load operation for a level from the given asset bundle.
        /// </summary>
        static public AssetBundleLoadOperation LoadLevelAsync(string assetBundleName, string levelName, bool isAdditive)
        {
            Log(LogType.Info, "Loading " + levelName + " from " + assetBundleName + " bundle");

            AssetBundleLoadOperation operation = null;
#if UNITY_EDITOR
            if (SimulateAssetBundleInEditor)
            {
                operation = new AssetBundleLoadLevelSimulationOperation(assetBundleName, levelName, isAdditive);
            }
            else
#endif
            {
                assetBundleName = RemapVariantName(assetBundleName);
                LoadAssetBundle(assetBundleName);
                operation = new AssetBundleLoadLevelOperation(assetBundleName, levelName, isAdditive);

                m_InProgressOperations.Add(operation);
            }

            return operation;
        }

        // download all assets bundles, make sure this is only called when not loading assets
        static public AssetBundleDownloadAllOperation DownloadAllAssetBundles(string[] bundleNames)
        {
            if (m_AssetBundleManifest == null)
            {
                Log(LogType.Error, "asset bundle manifest was not loaded");
                return null;
            }

            var op = new AssetBundleDownloadAllOperation();
#if UNITY_EDITOR
            if (SimulateAssetBundleInEditor)
            {
                op.progress = 1.0f;
                return op;
            }
#endif
            s_Instance.StartCoroutine(DownloadAllAssetBundlesImpl(bundleNames, op));
            return op;
        }

        static IEnumerator DownloadAllAssetBundlesImpl(string[] bundleNames, AssetBundleDownloadAllOperation downloadAllOp)
        {
            float totalProgress = 0.0f;
            if (bundleNames == null)
            {
                bundleNames = m_AssetBundleManifest.GetAllAssetBundles();
            }

            foreach (var bundleName in bundleNames)
            {
                Log(LogType.Info, "Loading " + bundleName);

                // do not load dependencies so that we can calculate the progress
                if (LoadAssetBundleInternal(bundleName, false))
                {
                    Debug.Log("bundle already loaded" + bundleName);
                }
                else
                {
                    var bundleOp = (AssetBundleDownloadOperation)m_InProgressOperations.Find(x => {
                        var op = x as AssetBundleDownloadOperation;
                        return op.assetBundleName == bundleName;
                    });
                    Assert.IsNotNull(bundleOp);

                    while (!bundleOp.IsDone())
                    {
                        downloadAllOp.progress = (totalProgress + bundleOp.progress) / bundleNames.Length;
                        yield return null;
                    }

                    if (!string.IsNullOrEmpty(bundleOp.error))
                    {
                        downloadAllOp.error = bundleOp.error;
                        yield break;
                    }
                }

                // bundleName is with variant, we don't want it to be remapped
                UnloadAssetBundleWithVariant(bundleName, true);

                totalProgress += 1.0f;
                downloadAllOp.progress = totalProgress / bundleNames.Length;
            }

            downloadAllOp.progress = 1.0f;
        }

    } // End of AssetBundleManager.
}
