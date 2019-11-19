//#define SIMULATE_NETWORK_ERROR

using LitJson;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using DataAccess;
using AssetBundles;
using System.Collections;
using Robomation;
using Scheduling;
using cn.sharesdk.unity3d;

public class Initialization : MonoBehaviour
{
    public int referenceScreenWidth;
    public int referenceScreenHeight;

    IEnumerator Start()
	{
        isInitialized = false;
        Init();
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        var uwr = SingletonManager.gameObject.AddComponent<UnityWindowResize>();
        uwr.aspect = (float)referenceScreenWidth / referenceScreenHeight;
        uwr.minWidth = referenceScreenWidth;
#elif UNITY_ANDROID && !UNITY_EDITOR
        Screen.fullScreen = false;
#endif
        yield return PostInit();
        isInitialized = true;
    }

    public bool isInitialized
    {
        get;
        private set;
    }

    void Init()
    {
        Environment.SetEnvironmentVariable("MONO_MANAGED_WATCHER", "enabled");

        ApplicationEvent.EnsureInstance();
        // make sure this is the very first registered handler where services get shutdown
        ApplicationEvent.onQuit += OnApplicationQuit;

        FileUtils.cleanupAppTempFiles();

		ServiceLocator.register<FileManager>();
		ServiceLocator.register<SoundResourceManager>();
		ServiceLocator.init();
        var scheduler = SingletonManager.gameObject.AddComponent<TimeSlicedScheduler>();
        scheduler.maxMillisecondsPerFrame = 100;
        Scheduler.instance = scheduler;

        LocalizationManager.instance.embeddedStrings = new[] { "embedded" };
        // TODO: get default language from install setting
        LocalizationManager.instance.onLanguageChanged += OnLanguageChanged;
        if (Preference.language == SystemLanguage.Unknown)
        {
            if (LocalizationManager.instance.isSupported(LocalizationManager.systemLanguage))
            {
                Preference.language = LocalizationManager.systemLanguage;
            }
            else
            {
                Preference.language = LocalizationManager.DefaultLang;
            }
        }
        LocalizationManager.instance.language = Preference.language;

        InputListenerManager.instance.Init();
        PopupManager.EnsureInstance();
        AppTicker.EnsureInstance();
        CallbackQueue.EnsureInstance();

        var shareSDK = SingletonManager.gameObject.AddComponent<ShareSDK>();
        shareSDK.appKey = "2b1f3d178a840";
        shareSDK.appSecret = "5aa695c77f2443521492b3dcaac3ad09";

        SingletonManager.gameObject.AddComponent<ARModelCache>();

        CmdServer.Start();

        NetManager.instance.Run();

        SocketManager.instance.serverAddress = AppConfig.SocketServerAddress;
        SocketManager.instance.serverPort = AppConfig.SocketServerPort;
        Singleton<WebRequestManager>.instance.UrlHost = AppConfig.WebServerUrl;

        NetworkSessionController.instance.Initialize(SocketManager.instance, Singleton<WebRequestManager>.instance);
        NetworkSessionController.instance.onLoggedOut += () => {
            UserManager.Instance.Reset();
            NetManager.instance.sysTaskPools.Clear();
            NetManager.instance.taskPools.Clear();
            PopupManager.CloseAll();
            StackUIBase.Clear();
            PythonScriptAutoUploader.instance.Stop();
            RobotManager.instance.uninitialize();
            Scheduler.instance.CancelAllTasks();

            if (CodeProjectRepository.instance != null)
            {
                CodeProjectRepository.instance.uninitialize();
            }
            if (GameboardRepository.instance != null)
            {
                GameboardRepository.instance.uninitialize();
            }
            if (PythonRepository.instance != null) 
            {
                PythonRepository.instance.uninitialize();
            }
            ApplicationEvent.AbortQuit();
        };

#if SIMULATE_NETWORK_ERROR
        var simulator = SingletonManager.gameObject.AddComponent<NetworkErrorSimulator>();
        simulator.Initialize(SocketManager.instance, Singleton<WebRequestManager>.instance);
#else
        SocketManager.instance.initialize();
#endif

        if (AssetBundleManager.LocalAssetBundleServer)
        {
            AssetBundleManager.SetDevelopmentAssetBundleServer();
            if (!IsInitScene())
            {
                AssetBundleManager.Initialize();
            }
        }
        ResetAssetBundleVariants();

        var voiceRepo = new VoiceRepository();
        CodeProjectRepository.instance = new CodeProjectRepository(voiceRepo);
        GameboardRepository.instance = new GameboardRepository(voiceRepo);
        PythonRepository.instance = new PythonRepository();

        InitializeUserSettings();

        Gameboard.ObjectActionConfigFactory.Initialize();
        VideoThumbnailCache.Init();

        UserIconResource.Initialize();
        ClassIconResource.Initialize();

        InitDebug();

#if DEVELOP
        Application.logMessageReceived += OnLogMessage;
#endif

#if !UNITY_IOS || UNITY_EDITOR
        UIPerformanceHelper.ignoredLayoutLayerMask = 1 << 14;
#endif
    }

    private IEnumerator PostInit()
    {
        yield return VideoThumbnailCache.PostInit();
        yield return LocalizationManager.instance.loadData();
    }

    public static void ResetAssetBundleVariants()
    {
        string localeDir = LocalizationManager.instance.currentLocaleDir ?? LocalizationManager.defaultLocaleDir;
        AssetBundleManager.ActiveVariants = new[] { localeDir };
    }

    static void OnLanguageChanged()
    {
        WindowUtils.SetMainWindowTitle("ui_app_name".Localize());
    }

    static bool IsInitScene()
    {
        // check if the staring scene is the first scene
        return SceneManager.GetActiveScene().buildIndex == 0;
    }

    private static void InitializeUserSettings()
    {
        var factory = new SimpleUserSettingFactory();
        factory.Register(CourseCodeGroupsKey.Prefix, key => new GameboardCodeGroups(key));
        factory.Register(CompetitionCodeGroupsKey.Prefix, key => new GameboardCodeGroups(key));
        factory.Register(SinglePkSortSetting.Key, UISortSetting.GetFactory(SinglePkSortSetting.Default));
        factory.Register(CodeProjectSortSetting.Prefix, UISortSetting.GetFactory(CodeProjectSortSetting.Default));
        factory.Register(GameboardSortSetting.Prefix, UISortSetting.GetFactory(GameboardSortSetting.Default));
        factory.Register(PythonSortSetting.Prefix, UISortSetting.GetFactory(PythonSortSetting.Default));
        factory.Register(PKSortSetting.Key, UISortSetting.GetFactory(PKSortSetting.Default));
        factory.Register(PKSortSetting.AnswerPrefix, UISortSetting.GetFactory(PKSortSetting.AnswerDefault));
        factory.Register(TeacherClassSortSetting.keyName, UISortSetting.GetFactory(TeacherClassSortSetting.Default));
        factory.Register(OnlineCTSortSeeting.keyName, UISortSetting.GetFactory(OnlineCTSortSeeting.Default));
        factory.Register(TeacherTaskSortSetting.keyName, UISortSetting.GetFactory(TeacherTaskSortSetting.Default));
        factory.Register(TeacherGradeSortSetting.keyName, UISortSetting.GetFactory(TeacherGradeSortSetting.Default));
        factory.Register(TeacherPoolSortSetting.keyName, UISortSetting.GetFactory(TeacherPoolSortSetting.Default));
        factory.Register(SystemPoolSortSetting.keyName, UISortSetting.GetFactory(SystemPoolSortSetting.Default));
        factory.Register(MyClassSortSetting.keyName, UISortSetting.GetFactory(MyClassSortSetting.Default));
        factory.Register(OnlineCSSortSetting.keyName, UISortSetting.GetFactory(OnlineCSSortSetting.Default));
        factory.Register(TaskSortSetting.keyName, UISortSetting.GetFactory(TaskSortSetting.Default));
        factory.Register(HonourSortSettings.TrophyKeyName, UISortSetting.GetFactory(TaskSortSetting.Default));
        factory.Register(ExerciseTeaSetting.keyName, UISortSetting.GetFactory(ExerciseTeaSetting.Default));
        factory.Register(ExerciseTeaSettingStu.keyName, UISortSetting.GetFactory(ExerciseTeaSettingStu.Default));
        factory.Register(BankCTSortSeeting.keyName, UISortSetting.GetFactory(BankCTSortSeeting.Default));

        UserManager.Instance.userSettings = new UserSettings(SocketManager.instance, factory);
    }

    static void InitDebug()
    {
        AR.Debug.Init();
        LogListener.Init();
    }

    static void OnApplicationQuit(ApplicationQuitEvent quit)
    {
        if (PythonScriptAutoUploader.instance.isUploading)
        {
            PopupManager.YesNo("ui_warning_quit_lose_python_changes".Localize(),
                () => {
                    Shutdown();
                    quit.Accept();
                },
                quit.Ignore);
            return;
        }

        Shutdown();
        quit.Accept();
    }

    static void Shutdown()
    {
        LogListener.Shutdown();
        CmdServer.Shutdown();
    }

    static void OnLogMessage(string condition, string stackTrace, LogType type)
    {
        // ignore logs from Debug.LogException calls
        if (type == LogType.Exception && !stackTrace.Contains("UnityEngine.Debug:LogException"))
        {
            PopupManager.Exception(stackTrace);
        }
    }
}
