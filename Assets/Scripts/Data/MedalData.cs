using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;

namespace DataAccess {
    public class MedalData {
        public string iconName;
        public string name;


        public static List<MedalData> s_data;

        public static void Load(IDataSource source) {
            s_data = JsonMapper.ToObject<List<MedalData>>(source.Get("medal_data"));
        }

        public static List<MedalData> getDatas() {
            return s_data;
        }
    }
}
