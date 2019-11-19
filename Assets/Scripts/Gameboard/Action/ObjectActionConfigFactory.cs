using LitJson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Gameboard
{
    public static class ObjectActionConfigFactory
    {
        delegate object ConfigDeserializer(string s);

        private static readonly Dictionary<Type, ConfigDeserializer> s_deserializers = new Dictionary<Type, ConfigDeserializer>();

        static ObjectActionConfigFactory()
        {
            InitializeImpl();
        }

        static void InitializeImpl()
        {
            var deserializeMethodInfo = typeof(ObjectActionConfigFactory).GetMethod("Deserialize", BindingFlags.Static | BindingFlags.NonPublic);

            var actionClasses = Assembly.GetAssembly(typeof(ObjectAction)).GetTypes()
                                        .Where(x => x.IsSubclassOf(typeof(ObjectAction)));
            foreach (var actionClass in actionClasses)
            {
                var configType = actionClass.GetNestedType("Config");
                if (configType != null)
                {
                    var deserializer = (ConfigDeserializer)
                        Delegate.CreateDelegate(typeof(ConfigDeserializer), null, deserializeMethodInfo.MakeGenericMethod(configType));
                    s_deserializers.Add(actionClass, deserializer);
                }
            }
        }

        public static void Initialize() { }

        private static object Deserialize<ConfigT>(string data)
        {
            return JsonMapper.ToObject<ConfigT>(data);
        }

        public static string Serialize(object config)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }
            return JsonMapper.ToJson(config);
        }

        public static object Deserialize(Type type, string config)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            ConfigDeserializer serializer;
            if (s_deserializers.TryGetValue(type, out serializer))
            {
                return serializer(config);
            }
            return null;
        }
    }
}
