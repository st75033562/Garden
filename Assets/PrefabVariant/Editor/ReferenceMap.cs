using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace PrefabVariant
{
    public class ReferenceMap
    {
        private readonly Dictionary<long, Object> m_parentIdToComps = new Dictionary<long, Object>();

        public void AddReference(ReferenceCollection refs)
        {
            foreach (var comp in refs.GetComponents<Component>())
            {
                if (comp is IPrefabComponent)
                {
                    continue;
                }

                var fileId = SerializedObjectUtils.GetFileId(comp);
                if (fileId != 0)
                {
                    var parentId = refs.GetParentId(fileId);
                    if (parentId != 0)
                    {
                        m_parentIdToComps.Add(parentId, comp);
                    }
                }
            }

            m_parentIdToComps.Add(SerializedObjectUtils.GetFileId(refs.parentObject), refs.gameObject);
        }

        public Object GetObjectByParentId(long p)
        {
            Object comp;
            m_parentIdToComps.TryGetValue(p, out comp);
            return comp;
        }

        public static ReferenceMap BuildFrom(GameObject go)
        {
            var refReg = new ReferenceMap();
            foreach (var refs in go.GetComponentsInChildren<ReferenceCollection>(true))
            {
                refReg.AddReference(refs);
            }
            return refReg;
        }
    }
}
