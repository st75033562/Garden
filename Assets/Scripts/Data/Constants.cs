using LitJson;
using System.Collections.Generic;

namespace DataAccess
{
    public static class Constants
    {
        public static void Load(IDataSource source)
        {
            var jsonData = JsonMapper.ToObject(source.Get("constants"));

            GameboardGlobal3DBundleId = jsonData["gameboard_global_3d_bundle_id"].GetInt();
            GameboardGlobal2DBundleId = jsonData["gameboard_global_2d_bundle_id"].GetInt();
            GameboardLocal2DBundleID = jsonData["gameboard_tortoise_2d_bundle_id"].GetInt();

            DefaultARObjectId = jsonData["default_ar_object_id"].GetInt();
            GameboardObjectNameFormat = (string)jsonData["gameboard_object_name_format"];
            CompetitionCoverRefHeight = jsonData["competition_cover_ref_height"].GetInt();
            CallStackMaxLimit = jsonData["call_stack_max_limit"].GetInt();
            MaxNumSounds = jsonData["max_num_sounds"].GetInt();
        }

        public static int GameboardGlobal3DBundleId
        {
            get;
            private set;
        }

        public static int GameboardGlobal2DBundleId
        {
            get;
            private set;
        }

        public static int GameboardLocal2DBundleID
        {
            get;
            private set;
        }

        public static int DefaultARObjectId
        {
            get;
            private set;
        }

        public static string GameboardObjectNameFormat
        {
            get;
            private set;
        }

        public static int CompetitionCoverRefHeight
        {
            get;
            private set;
        }

        public static int CallStackMaxLimit
        {
            get;
            private set;
        }

        public static int MaxNumSounds
        {
            get;
            private set;
        }
    }
}
