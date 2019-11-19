using PrefabVariant.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace PrefabVariant
{
    [DisallowMultipleComponent]
    public class ObjectChangeCollection : MonoBehaviour, IPrefabComponent, IEnumerable<ObjectChange>
    {
        [Serializable]
        class LongObjectChangesDict : SerializableDictionary<long, ObjectChange> { }

        [SerializeField]
        private LongObjectChangesDict m_changes = new LongObjectChangesDict();

        public ObjectChange Get(long fileId)
        {
            ObjectChange changes;
            if (!m_changes.TryGetValue(fileId, out changes))
            {
                changes = new ObjectChange(fileId);
                m_changes.Add(fileId, changes);
            }
            return changes;
        }

        public void Remove(long fileId)
        {
            m_changes.Remove(fileId);
        }

        public IEnumerator<ObjectChange> GetEnumerator()
        {
            return m_changes.Values.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();            
        }
    }
}
