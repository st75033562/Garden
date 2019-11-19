using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using UnityEngine;
using Object = UnityEngine.Object;

public class LocalizationManager : Singleton<LocalizationManager>
{
    private static readonly Dictionary<SystemLanguage, string> s_localeDirs 
        = new Dictionary<SystemLanguage, string>();

    public class Locale
    {
        public string name;
        public SystemLanguage lang;
    }

    public event Action onLanguageChanged;

    // resources for the default language must exist
    public const SystemLanguage DefaultLang = SystemLanguage.English;
    private SystemLanguage m_currentLang = SystemLanguage.Unknown;

    private Locale[] m_supportedLocales;
    private Dictionary<string, string> m_stringIds = new Dictionary<string, string>();
    private ILocalizationDataSource m_dataSource = new LocalizationResourceDataSource();
    private bool m_loading;

    static LocalizationManager()
    {
        s_localeDirs[SystemLanguage.Korean] = "ko";
        s_localeDirs[SystemLanguage.Chinese] = "sch";
        s_localeDirs[SystemLanguage.ChineseSimplified] = "sch";
        s_localeDirs[SystemLanguage.English] = "en";
    }

    public static SystemLanguage systemLanguage
    {
        get
        {
            // don't know when Chinese is returned, defaults to simplified Chinese
            if (Application.systemLanguage == SystemLanguage.Chinese)
            {
                return SystemLanguage.ChineseSimplified;
            }
            return Application.systemLanguage;
        }
    }

    void Awake()
    {
        loadSupportedLocales();
    }

    private void loadSupportedLocales()
    {
        var text = Resources.Load<TextAsset>("locales").text;
        List<Locale> locales = new List<Locale>();
        foreach (var line in text.Split(new[] { "=>" }, StringSplitOptions.RemoveEmptyEntries))
        {
            if (line.StartsWith("#"))
            {
                continue;
            }

            string[] tokens = line.Split(',');
            locales.Add(new Locale {
                name = tokens[0],
                lang = (SystemLanguage)Enum.Parse(typeof(SystemLanguage), tokens[1])
            });

            Debug.Assert(locales.Last().lang != SystemLanguage.Chinese, "Use ChineseSimplified instead");
        }
        m_supportedLocales = locales.ToArray();
    }

    public void reset()
    {
        m_stringIds.Clear();
        m_loading = false;
        m_currentLang = SystemLanguage.Unknown;
        m_dataSource.uninitialize();
        StopAllCoroutines();
    }

    public Locale[] supportedLocales
    {
        get { return m_supportedLocales; }
    }

    public bool isSupported(SystemLanguage lang)
    {
        Debug.Assert(lang != SystemLanguage.Chinese, "Use SimplifiedChinese instead");

        foreach (var locale in m_supportedLocales)
        {
            if (locale.lang == lang)
            {
                return true;
            }
        }
        return false;
    }

    public int currentLocaleIndex
    {
        get { return Array.FindIndex(m_supportedLocales, x => x.lang == m_currentLang); }
        set
        {
            if (value < 0 && value >= m_supportedLocales.Length)
            {
                throw new ArgumentOutOfRangeException();
            }

            language = m_supportedLocales[value].lang;
        }
    }

    // if language is Unknown, the default language is used
    // call this once to initialize
    // Chinese is an alias of ChineseSimplified
    public SystemLanguage language
    {
        get { return m_currentLang; }
        set
        {
            Debug.Assert(value != SystemLanguage.Chinese, "Use SimplifiedChinese instead");

            if (value == SystemLanguage.Unknown)
            {
                value = DefaultLang;
            }

            if (m_currentLang == value)
            {
                return;
            }

            m_currentLang = value;
            m_dataSource.setLanguage(m_currentLang);
        }
    }

    public ILocalizationDataSource dataSource
    {
        get { return m_dataSource; }
        set
        {
            if (value == null)
            {
                throw new ArgumentNullException();
            }
            if (m_loading)
            {
                throw new InvalidOperationException();
            }
            m_dataSource.uninitialize();
            m_dataSource = value;
            m_dataSource.setLanguage(m_currentLang);
        }
    }

    public string[] embeddedStrings
    {
        get;
        set;
    }

    public Coroutine loadData()
    {
        return StartCoroutine(loadDataImpl());
    }

    private IEnumerator loadDataImpl()
    {
        m_loading = true;
        var request = m_dataSource.loadString();
        yield return request;
        if (request.result != null)
        {
            m_stringIds.Clear();
            loadLocalizations(request.result);
            // load all embeded strings
            if (embeddedStrings != null)
            {
                foreach (var name in embeddedStrings)
                {
                    var asset = loadResource<TextAsset>(LocalizedResType.String, name);
                    if (asset)
                    {
                        loadLocalizations(asset.text);
                    }
                    else
                    {
                        Debug.LogErrorFormat("failed to load {0}: {1}", LocalizedResType.String, name);
                    }
                }
            }
        }
        m_loading = false;
        UpdateUICulture();

        if (onLanguageChanged != null)
        {
            onLanguageChanged();
        }
    }

    private void UpdateUICulture()
    {
        string cultureName;
        switch (m_currentLang)
        {
        case SystemLanguage.ChineseSimplified:
            cultureName = "zh-CN";
            break;

        case SystemLanguage.English:
        default:
            cultureName = "en-US";
            break;
        }

        var cultureInfo = CultureInfo.GetCultureInfo(cultureName);
        Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture = cultureInfo;
    }

    /// <summary>
    /// return the current locale directory name for assets.
    /// </summary>
    public string currentLocaleDir
    {
        get { return getLocaleDir(language, false); }
    }

    public static string defaultLocaleDir
    {
        get { return getLocaleDir(DefaultLang, false); }
    }

    public static string getLocaleDir(SystemLanguage language, bool fallback = true)
    {
        string dir;
        s_localeDirs.TryGetValue(language, out dir);
        return dir ?? (fallback ? defaultLocaleDir : null);
    }

    public static string getFilePath(string type, string name, SystemLanguage language = SystemLanguage.Unknown)
    {
        string subDir = getLocaleDir(language);
        return type + "/" + subDir + "/" + name;
    }

    private void loadLocalizations(string text)
    {
        foreach (var line in text.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries))
        {
            var tokens = line.Split('\t');
            if (m_stringIds.ContainsKey(tokens[0]))
            {
                Debug.LogWarning("duplicate localization id: " + tokens[0]);
                continue;
            }
            m_stringIds[tokens[0]] = tokens[1].Replace(@"\n", "\n");
        }
    }

    public string getString(string id)
    {
        string localization;
        if (!m_stringIds.TryGetValue(id, out localization))
        {
            localization = id;
        }
        return localization;
    }

    public T loadResource<T>(string type, string name) where T : Object
    {
        string path = getFilePath(type, name, m_currentLang);
        T res = Resources.Load<T>(path);
        if (!res)
        {
            string defaultPath = getFilePath(type, name);
            res = Resources.Load<T>(defaultPath);
        }
        return res;
    }
}
