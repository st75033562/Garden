using DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;

public class BundleObjectDataSource : IObjectResourceDataSource
{
    private readonly List<BundleAssetData> m_assets = new List<BundleAssetData>();

    public void Initialize(GameboardThemeBundleData current, GameboardThemeBundleData defaultBundle)
    {
        m_assets.Clear();
        if (current != null)
        {
            m_assets.AddRange(current.assets);
        }

        if (defaultBundle != null)
        {
            if (current != null)
            {
                var overrideAssets = new HashSet<string>(current.assets.Select(x => x.assetName));
                m_assets.AddRange(defaultBundle.assets.Where(x => !overrideAssets.Contains(x.assetName)));
            }
            else
            {
                m_assets.AddRange(defaultBundle.assets);
            }
        }
    }

    public IEnumerable<BundleAssetData> objectResources
    {
        get { return m_assets; }
    }

    public BundleAssetData GetAsset(string localizedName)
    {
        return m_assets.Find(x => x.localizedName == localizedName);
    }

    public BundleAssetData GetAsset(int id)
    {
        return m_assets.Find(x => x.id == id);
    }
}
