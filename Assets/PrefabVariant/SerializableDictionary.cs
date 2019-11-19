using System;
using System.Collections.Generic;
using UnityEngine;

namespace PrefabVariant.Collections
{
    [Serializable]
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [SerializeField]
        private List<TKey> m_savedKeys = new List<TKey>();

        [SerializeField]
        private List<TValue> m_savedValues = new List<TValue>();

        public void OnAfterDeserialize()
        {
            Clear();
            for (int i = 0; i < m_savedKeys.Count; ++i)
            {
                Add(m_savedKeys[i], m_savedValues[i]);
            }
            m_savedKeys.Clear();
            m_savedValues.Clear();
        }

        public void OnBeforeSerialize()
        {
            m_savedKeys.Clear();
            m_savedValues.Clear();
            m_savedKeys.Capacity = m_savedValues.Capacity = Count;

            foreach (var kv in this)
            {
                m_savedKeys.Add(kv.Key);
                m_savedValues.Add(kv.Value);
            }
        }
    }
}
