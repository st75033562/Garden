using System.Collections.Generic;

namespace DataAccess
{
    public class GameboardThemeData
    {
        public int id;
        public string name;
        public string thumbnailBundleName;
        public int objectsBundleId;
        public int objects2dBundleId;
        public int soundBundleId;
        public bool enabled;

        private static Dictionary<int, GameboardThemeData> s_data;

        public static void Load(IDataSource source)
        {
            s_data = JsonMapperUtils.ToDictFromList<int, GameboardThemeData>(source.Get("gameboard_themes"), x => x.id);
        }

        public static GameboardThemeData Get(int id)
        {
            GameboardThemeData data;
            s_data.TryGetValue(id, out data);
            return data;
        }

        public static IEnumerable<GameboardThemeData> Data
        {
            get { return s_data.Values; }
        }
    }
}
