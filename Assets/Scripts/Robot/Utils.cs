using System;
using System.Collections.Generic;
using UnityEngine;

namespace Robomation
{
    [Serializable]
    public struct ListSerializer<T>
    {
        public List<T> data;

        public static string Serialize(List<T> l)
        {
            ListSerializer<T> ser = new ListSerializer<T> { data = l };
            return JsonUtility.ToJson(ser);
        }

        public static List<T> Deserialize(string s)
        {
            var l = JsonUtility.FromJson<ListSerializer<T>>(s);
            return l.data;
        }
    }
}
