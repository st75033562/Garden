using System;
using UnityEngine;

public enum AutoStop
{
    Immediate,
    AfterOneSec,
    SustainPlaying,
}

public enum BlockLevel
{
    Beginner = 1,
    Intermediate,
    Advanced,

    Num = Advanced
}

public static class Preference
{
    public const BlockLevel DefaultBlockLevel = BlockLevel.Advanced;
    public const AutoStop DefaultAutoStop = AutoStop.AfterOneSec;
    public const bool DefaultSoundEffect = true;

    private const string KeyBlockLevel = "block_level";
    private const string KeySoundEffect = "sound_effect";
    private const string KeyAutoStop = "auto_stop";
    private const string KeyLanguage = "pref_language";
    private const string ScriptLanguageKey = "programing_model";

    private static uint s_userId;

    public static event Action onScriptLanguageChanged;

    public static void SetUserId(uint userId)
    {
        s_userId = userId;
        // the new highest level is Advanced
        if (blockLevel > BlockLevel.Advanced)
        {
            blockLevel = BlockLevel.Advanced;
        }
    }

    private static string GetKey(string key)
    {
        return key + "_" + s_userId;
    }

    public static BlockLevel blockLevel
    {
        get
        {
            return (BlockLevel)PlayerPrefs.GetInt(GetKey(KeyBlockLevel), (int)DefaultBlockLevel);
        }
        set
        {
            PlayerPrefs.SetInt(GetKey(KeyBlockLevel), (int)value);
        }
    }

    public static bool soundEffectEnabled
    {
        get
        {
            return PlayerPrefs.GetInt(GetKey(KeySoundEffect), DefaultSoundEffect ? 1 : 0) != 0;
        }
        set
        {
            PlayerPrefs.SetInt(GetKey(KeySoundEffect), value ? 1 : 0);
        }
    }

    public static AutoStop autoStop
    {
        get
        {
            return (AutoStop)PlayerPrefs.GetInt(GetKey(KeyAutoStop), (int)DefaultAutoStop);
        }
        set
        {
            PlayerPrefs.SetInt(GetKey(KeyAutoStop), (int)value);
        }
    }

    // NOTE: language is a app-wide setting
    public static SystemLanguage language
    {
        get { return (SystemLanguage)PlayerPrefs.GetInt(KeyLanguage, (int)SystemLanguage.Unknown); }
        set
        {
            PlayerPrefs.SetInt(KeyLanguage, (int)value);
        }
    }

    public static ScriptLanguage scriptLanguage
    {
        get
        {
            return (ScriptLanguage)PlayerPrefs.GetInt(GetKey(ScriptLanguageKey), (int)ScriptLanguage.Visual);
        }
        set
        {
            if (value != scriptLanguage)
            {
                PlayerPrefs.SetInt(GetKey(ScriptLanguageKey), (int)value);

                if (onScriptLanguageChanged != null)
                {
                    onScriptLanguageChanged();
                }
            }
        }
    }
}
