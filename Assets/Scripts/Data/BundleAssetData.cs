using System.Collections.Generic;
using Gameboard;
using LitJson;

namespace DataAccess
{
    public class BundleAssetData
    {
        public int id;
        public string bundleName;
        public string assetName;
        public string localizedName;
        public bool threeD;
        public bool runtimeCollision;
        public GizmoType gizmo;

        public string thumbnailAssetName { get { return assetName + "-thumbnail"; } }

        private static Dictionary<int, BundleAssetData> s_data;

        public static void Load(IDataSource source)
        {
            s_data = JsonMapperUtils.ToDictFromList<int, BundleAssetData>(source.Get("bundle_assets"), x => x.id);
        }

        public static BundleAssetData Get(int id)
        {
            BundleAssetData asset;
            s_data.TryGetValue(id, out asset);
            return asset;
        }
    }
}
