using System.Collections.Generic;
using UnityEngine;

namespace Gameboard
{
    public class ObjectActionManager : MonoBehaviour
    {
        public ObjectActionConfig m_config;

        // key is class name
        private readonly Dictionary<string, ObjectAction> m_actionMapping = new Dictionary<string, ObjectAction>();

        void Awake()
        {
            foreach (var action in GetComponents<ObjectAction>())
            {
                m_actionMapping.Add(action.GetType().FullName, action);
            }

            Stop();
        }

        public void Execute(int actionId, params string[] args)
        {
            if (m_config)
            {
                Execute(m_config, actionId, args);
            }
        }

        private void Execute(ObjectActionConfig config, int actionId, params string[] args)
        {
            if (actionId >= 0 && actionId < config.m_entries.Length)
            {
                var entry = config.m_entries[actionId];
                // ignore placeholders
                if (string.IsNullOrEmpty(entry.className))
                {
                    return;
                }

                ObjectAction action;
                if (m_actionMapping.TryGetValue(entry.className, out action))
                {
                    object actionConfig = null;
                    if (!string.IsNullOrEmpty(entry.jsonConfig))
                    {
                        actionConfig = ObjectActionConfigFactory.Deserialize(action.GetType(), entry.jsonConfig);
                    }
                    action.Execute(actionConfig, args);
                }
                else
                {
                    Debug.LogError("invalid action id: " + actionId, this);
                }
            }
            else
            {
                Debug.LogError("action id out of range: " + actionId, this);
            }
        }

        public void Stop()
        {
            foreach (var action in m_actionMapping.Values)
            {
                action.Stop();
            }
        }
    }
}
