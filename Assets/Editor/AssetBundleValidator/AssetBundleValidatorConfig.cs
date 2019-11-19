using LitJson;
using System;
using System.Collections.Generic;
using System.IO;

namespace AssetBundleValidator
{
    public class AnimatorSettings
    {
        private readonly Dictionary<string, HashSet<string>> m_ignoredStates 
            = new Dictionary<string, HashSet<string>>();

        public bool IsIgnored(string name, string stateName)
        {
            HashSet<string> states;
            m_ignoredStates.TryGetValue(name, out states);
            return states != null ? states.Contains(stateName) : false;
        }

        public void Load(JsonData data)
        {
            foreach (var key in data.Keys)
            {
                var states = new HashSet<string>();
                states.UnionWith(data[key].GetStringArray());
                m_ignoredStates.Add(key, states);
            }
        }
    }

    public class Configuration
    {
        public Configuration()
        {
            animatorSettings = new AnimatorSettings();
        }

        public AnimatorSettings animatorSettings
        {
            get;
            private set;
        }

        public void Load(string path)
        {
            var jsonData = JsonMapper.ToObject(File.ReadAllText(path));
            foreach (var key in jsonData.Keys)
            {
                if (key == "animator")
                {
                    animatorSettings.Load(jsonData[key]);
                }
            }
        }
    }
}
