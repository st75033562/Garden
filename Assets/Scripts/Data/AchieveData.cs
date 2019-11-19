using DataAccess;
using LitJson;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DataAccess {
    public class AchieveData {
        public string iconName;
        public string name;


        public static List<AchieveData> s_data;

        public static void Load(IDataSource source) {
            s_data = JsonMapper.ToObject<List<AchieveData>>(source.Get("achive_data"));
        }

        public static List<AchieveData> getDatas() {
            return s_data;
        }
    }
}
