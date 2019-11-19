using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DataAccess
{
    public class GameboardThemeBundleData : IEnumerable<BundleAssetData>
    {
        public int id;

        // cached assets
        public BundleAssetData[] assets;

        // only for parsing
        private class Entry
        {
            public int id = 0;
            public int assetId = 0;
        }

        private static Dictionary<int, GameboardThemeBundleData> s_data = new Dictionary<int, GameboardThemeBundleData>();

        public static void Load(IDataSource source)
        {
            var data = JsonMapper.ToObject<List<Entry>>(source.Get("gameboard_theme_bundles"));
            s_data = data.GroupBy(x => x.id)
                         .ToDictionary(x => x.Key, y => {
                             return new GameboardThemeBundleData {
                                 id = y.Key,
                                 assets = y.Select(x => BundleAssetData.Get(x.assetId)).ToArray()
                             };
                         });

            Global3DBundle = Get(Constants.GameboardGlobal3DBundleId);
            Global2DBundle = Get(Constants.GameboardGlobal2DBundleId);
        }

        public static GameboardThemeBundleData Get(int id)
        {
            GameboardThemeBundleData data;
            s_data.TryGetValue(id, out data);
            return data;
        }

        public static GameboardThemeBundleData Global3DBundle
        {
            get;
            private set;
        }

        public static GameboardThemeBundleData Global2DBundle
        {
            get;
            private set;
        }

        public BundleAssetData GetAsset(int id)
        {
            return Array.Find(assets, x => x.id == id);
        }

        public BundleAssetData GetAsset(string name)
        {
            return Array.Find(assets, x => x.localizedName == name);
        }

        public IEnumerator<BundleAssetData> GetEnumerator()
        {
            return (assets as IEnumerable<BundleAssetData>).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
