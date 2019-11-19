using AssetBundles;
using DataAccess;
using LitJson;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InitializationController : MonoBehaviour {
    public Text progressText;
    public Text hintText;
    public RectTransform rotoRect;
    public RectTransform slideRect;

    public AndroidUpdateService androidUpdateService;
    public GameObject btnUpdateRetry;
    public GameObject btnUpdateSkip;
    public GameObject btnUpdateContinue;
    private int currentLocalBundleVersion = 2;
    private bool loadLocalBundle;

    private enum State
    {
        CheckingVersion,
        DownloadingUpdate,
        CheckWifi,
        DownloadError,
        DownloadingBundles,
        Initializing,
    }

    private enum InitStage
    {
        LoadLocalBundle,
        DownloadBundles,
        InitTemplateCache,
       // InitARCache,
        Num,
    }

    [Flags]
    private enum UpdateButtons
    {
        Retry = 1 << 0,
        Skip  = 1 << 1,
        Continue = 1 << 2,
        All = 0xf,
    }

    private State m_state = State.CheckingVersion;
    private float m_slideTotalLength;
    private InitStage m_currentInitStage;

	// Use this for initialization
	IEnumerator Start () {
        SceneDirector.Init();
        StartCoroutine(GetBaiduToken());

        m_slideTotalLength = slideRect.rect.width;

        androidUpdateService.baseUrl = AppConfig.ApkDirUrl;
        androidUpdateService.onCheckVersionCompleted += OnCheckVersionCompleted;
        androidUpdateService.onDownloadCancelled += OnDownloadUpdateCancelled;
        androidUpdateService.onDownloadError += OnDownloadUpdateError;
        androidUpdateService.onDownloadProgress += OnDownloadUpdateProgressChanged;

        ShowUpdateButtons(UpdateButtons.All, false);
        hintText.text = "";
		
        var commonInit = GetComponent<Initialization>();
        while (!commonInit.isInitialized)
        {
            yield return null;
        }

        //if (Application.platform == RuntimePlatform.Android)
        //{
        //    InitAndroidUpdate();
        //}
        //else if (Application.platform == RuntimePlatform.OSXPlayer)
        //{
        //    yield return CheckMacUpdate();
        //    InitAssetBundleDownload();
        //}
        //else
        //{
        //    InitAssetBundleDownload();
        //}

        yield return CheckVersionUpdate();

        InitAssetBundleDownload();
        yield break;
	}
    IEnumerator CheckVersionUpdate()
    {
        hintText.text = "ui_check_new_version_title".Localize();
        GameObject go = new GameObject("checkVersionUpdate");
        BaseCheckUpdate checkUpdate = null;
#if UNITY_ANDROID && !UNITY_EDITOR
        checkUpdate = go.AddComponent<CheckAndroidUpdate>();
#elif UNITY_STANDALONE_OSX && !UNITY_EDITOR
        checkUpdate = go.AddComponent<CheckMacUpdate>();
#elif UNITY_IOS && !UNITY_EDITOR
        checkUpdate = go.AddComponent<CheckIosUpdate>();
#else
        checkUpdate = go.AddComponent<CheckWinUpdate>();
#endif
        yield return checkUpdate.CheckUpdate();
        Destroy(go);
    }

    void InitAndroidUpdate()
    {
        hintText.text = "ui_check_new_version_title".Localize();
        androidUpdateService.CheckForNewVersion();
        ShowUpdateButtons(UpdateButtons.All, false);
        m_state = State.CheckingVersion;
    }

    IEnumerator CheckMacUpdate()
    {
        hintText.text = "ui_check_new_version_title".Localize();
        using (var www = new TimedWWWRequest(AppConfig.StaticResUrl + "latest-version.txt"))
        {
            yield return www;

            if (string.IsNullOrEmpty(www.error))
            {
                var buildInfo = JsonMapper.ToObject(www.rawRequest.text)["osx"];
                var latestVer = new System.Version((string)buildInfo["version"]);
                var curVer = new System.Version(Application.version);
                if (latestVer > curVer)
                {
                    bool done = false;
                    PopupManager.YesNo("ui_go_download_updated".Localize(latestVer),
                        () => {
                            Application.OpenURL((string)buildInfo["url"]);
                            done = true;
                        },
                        () => done = true);
                    while (!done)
                    {
                        yield return null;
                    }
                }
            }
            else
            {
                Debug.LogWarning("failed to check update: " + www.error);
            }
        }
    }

    void InitAssetBundleDownload()
    {
        string localbundlkey = "localbundlekey";
        if (PlayerPrefs.GetInt(localbundlkey,0) >= currentLocalBundleVersion) {
            StartDownloadBundle();
            return;
        }
        loadLocalBundle = true;
        GameObject go = new GameObject("localbundle");
        StreamingBundle streamingBundle =
#if UNITY_ANDROID && !UNITY_EDITOR
        go.AddComponent<AndroidStreamingBundle>();
#elif UNITY_STANDALONE_OSX && !UNITY_EDITOR
            go.AddComponent<OSXStreamingBundle>();
#elif UNITY_IOS && !UNITY_EDITOR
            go.AddComponent<IOSStreamingBundle>();
#else
    go.AddComponent<WinStreamingBundle>();
#endif
        streamingBundle.LoadBundleManifest((bundleManifest) => {
            m_currentInitStage = InitStage.LoadLocalBundle;
            hintText.text = "ui_text_load_bundle_notice_1".Localize();

            UpdateProgress(0.0f);
            streamingBundle.CacheBundle(bundleManifest.GetAllAssetBundles(), (progress) => {
                if (progress == 1)
                {
                    Destroy(go);
                    PlayerPrefs.SetInt(localbundlkey, currentLocalBundleVersion);
                    StartDownloadBundle();
                }
                else {
                    UpdateProgress(progress);
                }
            });
        });
    }

    void StartDownloadBundle() {
        m_currentInitStage = InitStage.DownloadBundles;
        hintText.text = "ui_text_load_bundle_show".Localize();
        
        UpdateProgress(0.0f);
        ShowUpdateButtons(UpdateButtons.All, false);
        StartCoroutine(DownloadAssetBundles());
        m_state = State.DownloadingBundles;
    }

    IEnumerator DownloadAssetBundles()
    {
        if(!AssetBundleManager.SimulateAssetBundleInEditor &&
            !AssetBundleManager.LocalAssetBundleServer) {
            var versionRequest = TimedUnityWebRequest.Get(AppConfig.AssetBundleBaseUrl + "/version.txt");
            yield return versionRequest;

            if (!string.IsNullOrEmpty(versionRequest.error))
            {
                OnDownloadBundleError(versionRequest.error);
                yield break;
            }

            var versionDB = new VersionDatabase(versionRequest.Text());
            var bundleVersion = versionDB.GetVersionNumber(Application.version);
            Debug.Log("bundle version: " + bundleVersion);
            AssetBundleManager.SetSourceAssetBundleURL(AppConfig.AssetBundleBaseUrl + "/" + bundleVersion);
        }

        var initOp = AssetBundleManager.Initialize();
        // null if simulation is on
        if (initOp == null)
        {
            StartCoroutine(Init());
            yield break;
        }

        yield return initOp;

        if (!string.IsNullOrEmpty(initOp.error))
        {
            OnDownloadBundleError(initOp.error);
        }
        else
        {
            // ignore bundles which are managed by ResDownload temporarily
            var bundles = AssetBundleManager.AssetBundleManifestObject.GetAllAssetBundles();
            var downloadOp = AssetBundleManager.DownloadAllAssetBundles(bundles);
            while (!downloadOp.isDone)
            {
                UpdateProgress(downloadOp.progress);
                yield return null;
            }

            if (!string.IsNullOrEmpty(downloadOp.error))
            {
                OnDownloadBundleError(downloadOp.error);
            }
            else
            {
                StartCoroutine(Init());
            }
        }
    }

    void OnDownloadBundleError(string error)
    {
        Debug.LogError(error);
        hintText.text = "ui_download_asset_bundles_error".Localize();
        ShowUpdateButtons(UpdateButtons.Retry, true);
    }

    void OnDestroy()
    {
        if (androidUpdateService)
        {
            androidUpdateService.onCheckVersionCompleted -= OnCheckVersionCompleted;
            androidUpdateService.onDownloadCancelled -= OnDownloadUpdateCancelled;
            androidUpdateService.onDownloadError -= OnDownloadUpdateError;
            androidUpdateService.onDownloadProgress -= OnDownloadUpdateProgressChanged;
        }
    }

    IEnumerator Init()
    {
        m_state = State.Initializing;
        yield return LoadGameData();

        if (!AppConfig.LoadGameDataFromResource && !AssetBundleManager.SimulateAssetBundleInEditor)
        {
            LocalizationManager.instance.dataSource = new LocalizationBundleDataSource();
            yield return LocalizationManager.instance.loadData();
        }

        hintText.text = "";
        yield return null;
        m_currentInitStage = InitStage.InitTemplateCache;
        NodeTemplateCache.Instance.onInitProgressChanged += UpdateProgress;
        yield return NodeTemplateCache.Instance.Init(false);
        NodeTemplateCache.Instance.onInitProgressChanged -= UpdateProgress;


        hintText.text = "";
        yield return null;
        //m_currentInitStage = InitStage.InitARCache;
        //ARModelCache.Instance.onInitProgressChanged += UpdateProgress;
        //yield return ARModelCache.Instance.Init(false);
        //ARModelCache.Instance.onInitProgressChanged -= UpdateProgress;

        SceneDirector.Push("Lobby", saveCurSceneOnHistory: false);
    }

    IEnumerator LoadGameData()
    {
        DataAccess.Initialization.InitNodeData(DataAccess.ResourceDataSource.instance);

        if (!AppConfig.LoadGameDataFromResource && !AssetBundleManager.SimulateAssetBundleInEditor)
        {
            const string GameDataBundleName = "game_data";
            var op = AssetBundleManager.LoadAssetBundleAsync(GameDataBundleName);
            yield return op;

            if (!string.IsNullOrEmpty(op.error))
            {
                // This should not happen...
                Debug.LogError(op.error);

                hintText.text = "ui_failed_to_load_gamedata".Localize();
                for (; ; )
                {
                    yield return null;
                }
            }
            else
            {
                DataAccess.Initialization.Init(new DataAccess.AssetBundleDataSource(op.assetBundle));
            }

            AssetBundleManager.UnloadAssetBundle(GameDataBundleName);

            yield break;
        }

        DataAccess.Initialization.Init(DataAccess.ResourceDataSource.instance);
    }

    void UpdateProgress(float progressInStage)
    {
        float currentProgress = 0;
        if (m_currentInitStage == InitStage.LoadLocalBundle)
        {
            currentProgress = progressInStage * 0.3f;
        }
        else if (m_currentInitStage == InitStage.DownloadBundles)
        {
            if (loadLocalBundle)
            {
                currentProgress = 0.3f + progressInStage * 0.3f;
            }
            else
            {
                currentProgress = progressInStage * 0.6f;
            }
        }
        else if (m_currentInitStage == InitStage.InitTemplateCache)
        {
            currentProgress = 0.6f + progressInStage * 0.2f;
        }
        else
        {
            currentProgress = 0.8f + progressInStage * 0.2f;
        }
        rotoRect.anchoredPosition = new Vector2(m_slideTotalLength * currentProgress, 0);
        progressText.text = Mathf.CeilToInt(currentProgress * 100) + "%";
    }

    void IosSimulationProgress(float progressInStage) {
        rotoRect.anchoredPosition = new Vector2(m_slideTotalLength * progressInStage, 0);
    }

#region android update

    void OnDownloadUpdateProgressChanged(float progress)
    {
        UpdateProgress(progress);
        if (progress == 1.0f)
        {
            InitAssetBundleDownload();
        }
    }

    void OnCheckVersionCompleted(string error)
    {
        if (error != null)
        {
            Debug.LogError(error);

            hintText.text = "ui_check_new_version_error".Localize();
            ShowUpdateButtons(UpdateButtons.Retry | UpdateButtons.Skip, true);
        }
        else
        {
            if (!androidUpdateService.hasUpdate)
            {
                InitAssetBundleDownload();
            }
            else if (!Utils.IsWifiConnected())
            {
                m_state = State.CheckWifi;

                hintText.text = "ui_check_new_version_wifi_not_enabled".Localize();
                ShowUpdateButtons(UpdateButtons.Continue | UpdateButtons.Skip, true);
            }
            else
            {
                StartDownloadingNewVersion();
            }
        }
    }

    void ShowUpdateButtons(UpdateButtons buttons, bool visible)
    {
        btnUpdateRetry.SetActive((buttons & UpdateButtons.Retry) != 0 && visible);
        btnUpdateSkip.SetActive((buttons & UpdateButtons.Skip) != 0 && visible);
        btnUpdateContinue.SetActive((buttons & UpdateButtons.Continue) != 0 && visible);
    }

    void StartDownloadingNewVersion()
    {
        PopupManager.YesNo("ui_go_download_updated".Localize(androidUpdateService.latestVersion),
                        () => {
                            Application.OpenURL(androidUpdateService.downloadPath);
                            InitAssetBundleDownload();
                        },
                        () => {
                            InitAssetBundleDownload();
                        });
        //hintText.text = "ui_check_new_version_downloading".Localize(androidUpdateService.latestVersion);
        //UpdateProgress(0.0f);

        //ShowUpdateButtons(UpdateButtons.Skip, true);
        //m_state = State.DownloadingUpdate;

        //androidUpdateService.InstallLatestVersion();
    }

    void OnDownloadUpdateCancelled()
    {
        InitAssetBundleDownload();
    }

    void OnDownloadUpdateError(string error)
    {
        Debug.LogError(error);
        hintText.text = "ui_download_android_apk_error".Localize();
        ShowUpdateButtons(UpdateButtons.Retry | UpdateButtons.Skip, true);
        m_state = State.DownloadError;
    }

    public void OnClickRetryUpdate()
    {
        if (m_state == State.CheckingVersion)
        {
            InitAndroidUpdate();
        }
        else if (m_state == State.DownloadError)
        {
            StartDownloadingNewVersion();
        }
        else if (m_state == State.DownloadingBundles)
        {
            InitAssetBundleDownload();
        }
    }

    public void OnClickSkipUpdate()
    {
        if (m_state == State.DownloadingUpdate)
        { 
            androidUpdateService.CancelDownload();
        }
        else if (m_state == State.DownloadError || m_state == State.CheckWifi || m_state == State.CheckingVersion)
        {
            InitAssetBundleDownload();
        }
    }

    public void OnClickContinueUpdate()
    {
        StartDownloadingNewVersion();
    }

#endregion android update

    IEnumerator GetBaiduToken() {
        UnityWebRequest webRequest = UnityWebRequest.Get(AppConfig.BaiduAiSound);
        yield return webRequest.Send();
        if (webRequest.isError)
            Debug.Log(webRequest.error);
        else
        {
            JsonData data = JsonMapper.ToObject(webRequest.downloadHandler.text);
            UserManager.Instance.baiduToken = data["access_token"].ToString();
        }

    }

}
