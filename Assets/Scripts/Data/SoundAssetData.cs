using System.Collections.Generic;

namespace DataAccess
{
    public class SoundAssetData
    {
        public int id;
        public string bundleName;
        public string assetName;
        public string localizedName;
        public float volume = 1;

        private static Dictionary<int, SoundAssetData> s_data;

        public static void Load(IDataSource source)
        {
            s_data = JsonMapperUtils.ToDictFromList<int, SoundAssetData>(source.Get("sound_assets"), x => x.id);
        }

        public static SoundAssetData Get(int id)
        {
            SoundAssetData asset;
            s_data.TryGetValue(id, out asset);
            return asset;
        }
    }
}