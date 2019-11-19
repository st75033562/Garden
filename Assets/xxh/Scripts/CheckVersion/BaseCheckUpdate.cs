using LitJson;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseCheckUpdate : MonoBehaviour
{
    protected string platformName;

    public virtual IEnumerator CheckUpdate()
    {
        using (var www = new TimedWWWRequest(AppConfig.StaticResUrl + "latest-version.txt"))
        {
            yield return www;

            if (string.IsNullOrEmpty(www.error))
            {
                var buildInfo = JsonMapper.ToObject(www.rawRequest.text)[platformName];
                var latestVer = new System.Version((string)buildInfo["version"]);
                var curVer = new System.Version(Application.version);
                if (latestVer > curVer)
                {
                    bool done = false;
                    PopupManager.YesNo("ui_go_download_updated".Localize(latestVer),
                        () =>
                        {
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
}
