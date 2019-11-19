using LitJson;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_EDITOR
[InitializeOnLoad]
#endif
public class JsonMapperUtils
{
    private static bool s_registered;

    static JsonMapperUtils()
    {
        RegisterCustomSerializers();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void RegisterCustomSerializers()
    {
        if (s_registered)
        {
            return;
        }
        s_registered = true;

        JsonMapper.RegisterExporter<float>((obj, writer) => writer.Write(Convert.ToDouble(obj)));
        JsonMapper.RegisterImporter<double, float>(input => Convert.ToSingle(input));
        JsonMapper.RegisterImporter<long, uint>(input => Convert.ToUInt32(input));
        JsonMapper.RegisterImporter<int, long>(input => (long)input);
        JsonMapper.RegisterImporter<int, bool>(input => input != 0);
        JsonMapper.RegisterImporter<string, Color>(input => {
            Color color;
            if (ColorUtility.TryParseHtmlString(input, out color))
            {
                return color;
            }
            throw new FormatException();
        });
        JsonMapper.RegisterExporter<Color>((obj, writer) => ColorUtility.ToHtmlStringRGB(obj));
    }

    public static Dictionary<TKey, TValue> ToDictFromList<TKey, TValue>(string data, Func<TValue, TKey> keySelector)
    {
        var list = JsonMapper.ToObject<List<TValue>>(data);
        return list.ToDictionary(keySelector);
    }

    public static JsonData ToJson(Dictionary<string, string> dict)
    {
        var data = new JsonData();
        foreach (var kv in dict)
        {
            data[kv.Key] = kv.Value;
        }
        return data;
    }
}
