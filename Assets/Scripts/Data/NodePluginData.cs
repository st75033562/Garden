using LitJson;
using System.Collections.Generic;

namespace DataAccess
{
    public class NodePluginData
    {
        public int id;
        public int resId;
        public string extension;
        public string clickAction;

        private static Dictionary<int, NodePluginData> s_data;

        public static void Load(IDataSource source)
        {
            s_data = JsonMapperUtils.ToDictFromList<int, NodePluginData>(source.Get("node_plugin"), x => x.id);
        }

        public static NodePluginData Get(int id)
        {
            NodePluginData data;
            if (!s_data.TryGetValue(id, out data))
            {
                UnityEngine.Debug.LogError(id);
            }
            return data;
        }
    }
}
