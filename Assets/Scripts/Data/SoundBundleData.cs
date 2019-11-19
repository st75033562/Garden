using LitJson;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DataAccess
{
    public class SoundBundleData : IEnumerable<SoundAssetData>
    {
        public int id;

        // cached assets
        public SoundAssetData[] assets;

        // only for parsing
        private class Entry
        {
            public int id = 0;
            public int assetId = 0;
        }

        private static Dictionary<int, SoundBundleData> s_data = new Dictionary<int, SoundBundleData>();

        public static void Load(IDataSource source)
        {
            var data = JsonMapper.ToObject<List<Entry>>(source.Get("sound_bundles"));
            s_data = data.GroupBy(x => x.id)
                         .ToDictionary(x => x.Key, y => {
                             return new SoundBundleData {
                                 id = y.Key,
                                 assets = y.Select(x => SoundAssetData.Get(x.assetId)).ToArray()
                             };
                         });
        }

        public static SoundBundleData Get(int id)
        {
            SoundBundleData data;
            s_data.TryGetValue(id, out data);
            return data;
        }

        public IEnumerator<SoundAssetData> GetEnumerator()
        {
            return (assets as IEnumerable<SoundAssetData>).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
