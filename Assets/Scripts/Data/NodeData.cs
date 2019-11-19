using LitJson;
using System.Collections.Generic;

namespace DataAccess
{
    public class NodeData
    {
        public int templateId;
        public int level;
        public int order;

        private static List<NodeData> s_data;

        public static void Load(IDataSource source)
        {
            s_data = JsonMapper.ToObject<List<NodeData>>(source.Get("node"));
        }

        public static IEnumerable<NodeData> Data
        {
            get { return s_data; }
        }
    }
}
