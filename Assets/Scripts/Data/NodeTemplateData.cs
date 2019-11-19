using LitJson;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DataAccess
{
    public class NodeTemplateData
    {
        public int id;
        public string name;
        public int resId;
        public int type;
        public string pluginKey;
        public string scriptName;
        public bool enabled;
        public int filter;

        private static readonly NodeData[] s_emptyLevelData = new NodeData[0];
        private NodeData[] m_levelData = s_emptyLevelData;

        private static Dictionary<int, NodeTemplateData> s_data = new Dictionary<int, NodeTemplateData>();

        public static void Load(IDataSource source)
        {
            s_data = JsonMapperUtils.ToDictFromList<int, NodeTemplateData>(source.Get("node_template"), x => x.id);
        }

        public static IEnumerable<NodeTemplateData> Data
        {
            get { return s_data.Values; }
        }

        public static IEnumerable<NodeTemplateData> GetAllByFilter(int filter)
        {
            return s_data.Values.Where(x => (x.filter & filter) != 0);
        }

        public static int Count
        {
            get { return s_data.Count; }
        }

        public static void Cache()
        {
            foreach (var group in NodeData.Data.GroupBy(x => x.templateId))
            {
                Get(group.Key).m_levelData = group.ToArray();
            }
        }

        public static NodeTemplateData Get(int id)
        {
            return s_data[id];
        }

        public NodeData GetLevelData(int level)
        {
            return Array.Find(m_levelData, x => x.level == level);
        }

        public IEnumerable<NodeData> allLevelData
        {
            get { return m_levelData; }
        }

        public string pluginConfig
        {
            get { return pluginKey.Localize(); }
        }

        public IEnumerable<short> GetPluginIds()
        {
            var config = pluginConfig;
            return config != "" ? config.Split('-').Select(x => short.Parse(x)) : Enumerable.Empty<short>();
        }
    }
}