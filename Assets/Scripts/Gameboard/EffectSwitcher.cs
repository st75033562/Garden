using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Gameboard
{
    public class EffectSwitcher : MonoBehaviour
    {
        public Material replacementMat;
        private readonly Dictionary<Renderer, Material[]> m_savedMaterials = new Dictionary<Renderer, Material[]>();

        public GameObject target
        {
            get;
            set;
        }

        public void Apply()
        {
            var renderers = target.GetComponentsInChildren<Renderer>();
            foreach (var render in renderers)
            {
                var oldMaterials = render.sharedMaterials;
                m_savedMaterials.Add(render, oldMaterials);

                render.sharedMaterials = Enumerable.Repeat(replacementMat, oldMaterials.Length).ToArray();
            }
        }

        public void Reset()
        {
            m_savedMaterials.Clear();
            target = null;
        }

        public void Restore()
        {
            foreach (var kv in m_savedMaterials)
            {
                kv.Key.sharedMaterials = kv.Value;
            }
            m_savedMaterials.Clear();
        }
    }
}
