using LitJson;
using System.Collections.Generic;
using System.Linq;

namespace DataAccess
{
    public class ARObjectData
    {
        public int id;
        public string bundleName;
        public string assetName;
        public string localizedName;
        public int categoryId;
        public int price;

        public string thumbnailName
        {
            get { return assetName + "-thumbnail"; }
        }
    }

    public class ARObjectCategory
    {
        public int id;
        public string name;
        public int order;
    }

    public class ARObjectDataSource
    {
        private static Dictionary<int, ARObjectData> s_objects = new Dictionary<int, ARObjectData>();
        private static List<ARObjectCategory> s_categories = new List<ARObjectCategory>();

        public static void Load(IDataSource source)
        {
            s_objects = JsonMapperUtils.ToDictFromList<int, ARObjectData>(source.Get("ar_object"), x => x.id);
            s_categories = JsonMapper.ToObject<List<ARObjectCategory>>(source.Get("ar_object_category"));
            s_categories.Sort((x, y) => x.order.CompareTo(y.order));
        }

        public static IEnumerable<ARObjectData> allObjects
        {
            get { return s_objects.Values; }
        }

        public static IEnumerable<ARObjectData> freeObjects
        {
            get { return allObjects.Where(x => x.price == 0); }
        }

        public static int objectCount
        {
            get { return s_objects.Count; }
        }

        public static ARObjectData GetObject(int id)
        {
            ARObjectData data;
            s_objects.TryGetValue(id, out data);
            return data;
        }

        public static IEnumerable<ARObjectCategory> categories
        {
            get { return s_categories; }
        }
    }
}
