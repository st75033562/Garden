using System.Collections.Generic;
using LitJson;

namespace DataAccess
{
    public class NodeLegacyIdMapping
    {
        public int id;
        public int templateId;

        private static Dictionary<int, int> s_idToTemplateId = new Dictionary<int, int>();
        private static Dictionary<int, int> s_templateIdToId = new Dictionary<int, int>();

        public static void Load(IDataSource source)
        {
            var mappings = JsonMapper.ToObject<List<NodeLegacyIdMapping>>(source.Get("node_legacy_id_mapping"));
            foreach (var map in mappings)
            {
                s_idToTemplateId.Add(map.id, map.templateId);
                if (!s_templateIdToId.ContainsKey(map.templateId))
                {
                    s_templateIdToId.Add(map.templateId, map.id);
                }
            }
        }

        public static int GetTemplateIdById(int id)
        {
            int templateId;
            s_idToTemplateId.TryGetValue(id, out templateId);
            return templateId;
        }

        public static int GetIdByTemplateId(int templateId)
        {
            int nodeId;
            s_templateIdToId.TryGetValue(templateId, out nodeId);
            return nodeId;
        }
    }
}
