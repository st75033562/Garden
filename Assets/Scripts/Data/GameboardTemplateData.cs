using System.Collections.Generic;

namespace DataAccess
{
    public class GameboardTemplateData
    {
        public const string Thumbnail = "thumbnail";

        public int id;
        public int themeId;
        public string name;
        public string sceneBundleName;
        public string thumbnailBundleName;
        public bool enabled;

        public string sceneName
        {
            get
            {
                int index = sceneBundleName.IndexOf('-');
                return sceneBundleName.Substring(index + 1);
            }
        }

        public GameboardThemeData theme
        {
            get { return GameboardThemeData.Get(themeId); }
        }

        public GameboardThemeBundleData objectsBundle
        {
            get
            {
                var theme = this.theme;
                return theme != null ? GameboardThemeBundleData.Get(theme.objectsBundleId) : null;
            }
        }

        public GameboardThemeBundleData objects2dBundle
        {
            get
            {
                var theme = this.theme;
                return theme != null ? GameboardThemeBundleData.Get(theme.objects2dBundleId) : null;
            }
        }

        public SoundBundleData soundBundle
        {
            get
            {
                var theme = this.theme;
                return theme != null ? SoundBundleData.Get(theme.soundBundleId) : null;
            }
        }

        private static Dictionary<int, GameboardTemplateData> s_themes = new Dictionary<int, GameboardTemplateData>();

        public static void Load(IDataSource source)
        {
            s_themes = JsonMapperUtils.ToDictFromList<int, GameboardTemplateData>(source.Get("gameboard_templates"), x => x.id);
        }

        public static GameboardTemplateData Get(int id)
        {
            GameboardTemplateData theme;
            s_themes.TryGetValue(id, out theme);
            return theme;
        }

        public static IEnumerable<GameboardTemplateData> Data
        {
            get { return s_themes.Values; }
        }
    }
}
