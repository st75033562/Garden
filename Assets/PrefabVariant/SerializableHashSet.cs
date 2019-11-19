using System;
using System.Collections.Generic;
using UnityEngine;

namespace PrefabVariant.Collections
{
    [Serializable]
    public class SerializableHashSet<T> : HashSet<T>, ISerializationCallbackReceiver
    {
        [SerializeField]
        private List<T> m_saveList = new List<T>();

        public void OnAfterDeserialize()
        {
            Clear();
            foreach (var e in m_saveList)
            {
                Add(e);
            }
            m_saveList.Clear();
        }

        public void OnBeforeSerialize()
        {
            m_saveList.Clear();
            m_saveList.AddRange(this);
        }
    }
}
