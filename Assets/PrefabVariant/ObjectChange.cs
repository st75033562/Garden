using PrefabVariant.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace PrefabVariant
{
    [Serializable]
    public class PropertyChange
    {
        [SerializeField]
        private string m_path;

        [SerializeField]
        private string m_type;

        public PropertyChange(string path, string type)
        {
            m_path = path;
            m_type = type;
        }

        public string path { get { return m_path; } }

        public string type { get { return m_type; } }
    }

    [Serializable]
    public class ObjectChange : IEnumerable<PropertyChange>
    {
        [SerializeField]
        private long m_fileId;

        [SerializeField]
        private List<PropertyChange> m_changes = new List<PropertyChange>();

        public ObjectChange(long fileId)
        {
            m_fileId = fileId;
        }

        public long fileId
        {
            get { return m_fileId; }
        }

        public void Add(PropertyChange change)
        {
            if (change == null)
            {
                throw new ArgumentNullException("change");
            }

            m_changes.Add(change);
        }

        public void Remove(PropertyChange change)
        {
            m_changes.Remove(change);
        }

        public PropertyChange Get(string path)
        {
            return m_changes.Find(x => x.path == path);
        }

        public IEnumerator<PropertyChange> GetEnumerator()
        {
            return m_changes.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
