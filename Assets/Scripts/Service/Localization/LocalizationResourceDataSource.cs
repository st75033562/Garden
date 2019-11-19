using UnityEngine;

public class LocalizationResourceDataSource : LocalizationDataSourceBase
{
    public override AsyncRequest<string> loadString()
    {
        var request = new SimpleAsyncRequest<string>();
        var path = LocalizationManager.getFilePath(LocalizedResType.String, StringFileName, m_language);
        var asset = Resources.Load<TextAsset>(path);
        if (!asset)
        {
            Debug.LogFormat("no localization asset for {0}", path);
            request.SetResult(null);
            return request;
        }
        request.SetResult(asset.text);
        return request;
    }
}
