using System.Collections;
using UnityEngine;

public interface ILocalizationDataSource
{
    void setLanguage(SystemLanguage language);

    void uninitialize();

    AsyncRequest<string> loadString();
}
