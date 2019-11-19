using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Gameboard
{
    public enum GizmoType
    {
        None,
        ParticleSystem
    }

    public class GizmoManager : MonoBehaviour
    {
        public Sprite[] m_gizmoSprites;
        public GraphicRaycaster m_raycaster;
        public GameObject m_gizmoTemplate;
        public Transform m_container;
        // gizmo will be scaled up to make it more recognizable if the distance to camera is larger than the threshold
        public float m_minScaleDistance;

        private class Entry
        {
            public Transform target;
            public Transform gizmo;
        }

        private readonly List<Entry> m_entries = new List<Entry>();
        private Camera m_worldCamera;

        void Awake()
        {
            m_worldCamera = Camera.main;
        }

        // world camera for calculating the distance to target objects
        public Camera worldCamera
        {
            get { return m_worldCamera; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                m_worldCamera = value;

                foreach (var entry in m_entries)
                {
                    entry.gizmo.gameObject.SetActive(true);
                }
            }
        }

        public bool HasGizmo(Transform target)
        {
            return m_entries.Find(x => x.target == target) != null;
        }

        public void AddGizmo(Transform target, GizmoType type)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }
            if (type == GizmoType.None)
            {
                throw new ArgumentOutOfRangeException("type");
            }

            var instance = Instantiate(m_gizmoTemplate, m_container, false);
            instance.GetComponent<Image>().sprite = m_gizmoSprites[(int)type - 1];

            var entry = new Entry {
                target = target,
                gizmo = instance.transform
            };

            if (m_worldCamera)
            {
                UpdateEntry(entry);
                entry.gizmo.gameObject.SetActive(true);
            }
            m_entries.Add(entry);
        }

        private void UpdateEntry(Entry entry)
        {
            if (!entry.target)
            {
                return;
            }

            entry.gizmo.position = entry.target.position;
            entry.gizmo.rotation = m_worldCamera.transform.rotation;
            if (m_minScaleDistance > 0)
            {
                var dist = Vector3.Dot(entry.target.position - m_worldCamera.transform.position, m_worldCamera.transform.forward);
                entry.gizmo.localScale = Vector3.one * Mathf.Max(dist / m_minScaleDistance, 1);
            }
            else
            {
                entry.gizmo.localScale = Vector3.one;
            }
        }

        public GameObject PickGizmo(PointerEventData eventData)
        {
            var results = new List<RaycastResult>();
            m_raycaster.Raycast(eventData, results);
            return results.Count > 0 ? results[0].gameObject : null;
        }

        public void RemoveGizmo(Transform target)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }

            var index = m_entries.FindIndex(x => x.target == target);
            if (index != -1)
            {
                var entry = m_entries[index];
                Destroy(entry.gizmo.gameObject);
                m_entries.RemoveAt(index);
            }
        }

        public void RemoveGizmos()
        {
            foreach (var entry in m_entries)
            {
                Destroy(entry.gizmo.gameObject);
            }
            m_entries.Clear();
        }

        void Update()
        {
            if (!m_worldCamera) { return; }

            foreach (var entry in m_entries)
            {
                UpdateEntry(entry);
            }
        }
    }
}
