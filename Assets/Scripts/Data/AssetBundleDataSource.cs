using UnityEngine;

namespace DataAccess
{
    public class AssetBundleDataSource : IDataSource
    {
        private readonly AssetBundle m_bundle;

        public AssetBundleDataSource(AssetBundle bundle)
        {
            m_bundle = bundle;
        }

        public string Get(string tableName)
        {
            return m_bundle.LoadAsset<TextAsset>(tableName).text;
        }
    }
}
