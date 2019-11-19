using UnityEngine;

public abstract class LocalizationDataSourceBase : ILocalizationDataSource
{
    protected const string StringFileName = "strings";

    protected SystemLanguage m_language = SystemLanguage.Unknown;

    public virtual void setLanguage(SystemLanguage language)
    {
        m_language = language;
    }

    public virtual void uninitialize() { }

    public abstract AsyncRequest<string> loadString();
}
