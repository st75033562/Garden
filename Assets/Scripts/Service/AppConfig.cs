using UnityEngine;

/// <summary>
/// some global app constants
/// </summary>
public static class AppConfig
{
    public const string WebServerProdUrl = "http://turtle.pocketurtle.cn";
#if UNITY_EDITOR || DEVELOP
    public const string WebServerTestUrl = "http://turtle.pocketurtle.cn:8080";
    public const string StaticResUrl = WebServerTestUrl + "/";
#else
    public const string StaticResUrl = "http://turtle-static.pocketurtle.cn/";
#endif

    public const string ApkDirUrl = StaticResUrl + "android_build";
    public const string AssetBundleDir = "asset_bundles";

    public const string SocketServerAddress = "turtle.pocketurtle.cn";
    public const int SocketServerProdPort = 2345;
#if UNITY_EDITOR || DEVELOP
    public const int SocketServerTestPort = 2346;
#endif

    public const string BaiduAiSound = "https://openapi.baidu.com/oauth/2.0/token?grant_type=client_credentials&client_id=wBRrm8l5x1kFsjgEKKLaj4Xh&client_secret=yfpb27zcZsMXhvVKAGqe8ob3fMuCNwOP";

    private const string ServerModeKey = "AppConfig_TestServer";

    public static bool DebugOn
    {
#if UNITY_EDITOR || ENABLE_DEBUG_OPTION
        get { return true; }
#else
        get { return false; }
#endif
    }

    /// <summary>
    /// true for using test server config, otherwise production server config
    /// </summary>
    public static bool TestServer
    {
#if USE_TEST_SERVER
        get { return true; }
        set { }
#elif UNITY_EDITOR || DEVELOP
        get { return PlayerPrefs.GetInt(ServerModeKey, 0) != 0; }
        set { PlayerPrefs.SetInt(ServerModeKey, value ? 1 : 0); }
#else
        get { return false; }
        set { }
#endif
    }

    public static string WebServerUrl
    {
#if UNITY_EDITOR || DEVELOP
        get { return TestServer ? WebServerTestUrl : WebServerProdUrl; }
#else
        get { return WebServerProdUrl; }
#endif
    }

    public static int SocketServerPort
    {
#if UNITY_EDITOR || DEVELOP
        get { return TestServer ? SocketServerTestPort : SocketServerProdPort; }
#else
        get { return SocketServerProdPort; }
#endif
    }

    public static string AssetBundleBaseUrl
    {
        get
        {
            return StaticResUrl + AssetBundleDir;
        }
    }

    public static bool LoadGameDataFromResource
    {
        get
        {
#if (UNITY_EDITOR || LOAD_DATA_FROM_RESOURCE)
            return true;
#else
            return false;
#endif
        }
    }
}
