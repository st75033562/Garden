using System.Collections.Generic;
using DataAccess;

public interface IObjectResourceDataSource
{
    IEnumerable<BundleAssetData> objectResources { get; }

    BundleAssetData GetAsset(string localizedName);

    BundleAssetData GetAsset(int id);
}