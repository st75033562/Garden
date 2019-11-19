using PrefabVariant.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace PrefabVariant
{
    [DisallowMultipleComponent]
    public class ReferenceCollection : MonoBehaviour, IPrefabComponent, ISerializationCallbackReceiver
    {
        [Serializable]
        class LongLongDict : SerializableDictionary<long, long> { }

        [SerializeField]
        private GameObject m_parentObject;

        [SerializeField]
        private LongLongDict m_childToParentRefs = new LongLongDict();

        private readonly Dictionary<long, long> m_parentToChildRefs = new Dictionary<long, long>();

        [SerializeField]
        private List<long> m_removedComponents = new List<long>();

        public void Add(long fileId, long parentFileId)
        {
            m_childToParentRefs.Add(fileId, parentFileId);
        }

        public long GetParentId(long fileId)
        {
            long parentFileId;
            m_childToParentRefs.TryGetValue(fileId, out parentFileId);
            return parentFileId;
        }

        public void Remove(long fileId)
        {
            var parentId = GetParentId(fileId);
            m_childToParentRefs.Remove(fileId);
            m_parentToChildRefs.Remove(parentId);
        }

        public void SetParentRemoved(long parentFileId)
        {
            long fileId = GetIdByParentId(parentFileId);
            if (fileId != 0)
            {
                m_parentToChildRefs.Remove(parentFileId);
                m_removedComponents.Add(parentFileId);
                m_childToParentRefs.Remove(fileId);
            }
        }

        public void ClearParentRemoved(long parentFileId)
        {
            m_removedComponents.Remove(parentFileId);
        }

        public bool IsParentRemoved(long parentFileId)
        {
            return m_removedComponents.Contains(parentFileId);
        }

        public bool IsParentReferenced(long parentFileId)
        {
            return m_parentToChildRefs.ContainsKey(parentFileId);
        }

        public long GetIdByParentId(long parentFileId)
        {
            long fileId;
            m_parentToChildRefs.TryGetValue(parentFileId, out fileId);
            return fileId;
        }

        public IEnumerable<KeyValuePair<long, long>> references
        {
            get { return m_childToParentRefs; }
        }

        public IEnumerable<long> removedComponents
        {
            get { return m_removedComponents; }
        }

        /// <summary>
        /// the inherited parent object
        /// </summary>
        public GameObject parentObject
        {
            get { return m_parentObject; }
            set { m_parentObject = value; }
        }

        public void OnAfterDeserialize()
        {
            m_parentToChildRefs.Clear();
            foreach (var refPair in references)
            {
                m_parentToChildRefs.Add(refPair.Value, refPair.Key);
            }
        }

        public void OnBeforeSerialize()
        {
        }
    }
}
