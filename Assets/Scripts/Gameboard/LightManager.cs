using UnityEngine;
using RobotSimulation;
using System.Collections.Generic;
using System;

namespace Gameboard
{
    public class LightManager : ILighting
    {
        private float m_ambientIlluminance;
        private const string LightTag = "Light";

        private readonly List<LightSource> m_lightSources = new List<LightSource>();

        public LightManager(ObjectManager objManager)
        {
            objManager.onEntityActivated += OnEntityActivated;
            objManager.onEntityRemoved += OnEntityRemoved;
        }

        private void OnEntityActivated(Entity entity)
        {
            if (entity.gameObject.CompareTag(LightTag) && entity.isVisibleInHierarchy)
            {
                var light = entity.GetComponent<LightSource>();
                m_lightSources.Add(light);
                entity.onVisibilityChanged += OnEntityVisibilityChanged;
            }
        }

        private void OnEntityVisibilityChanged(Entity entity)
        {
            if (!entity.gameObject.CompareTag(LightTag))
            {
                return;
            }

            var light = entity.GetComponent<LightSource>();
            if (entity.isVisibleInHierarchy)
            {
                if (!m_lightSources.Contains(light))
                {
                    m_lightSources.Add(light);
                }
            }
            else
            {
                m_lightSources.Remove(light);
            }
        }

        private void OnEntityRemoved(Entity entity)
        {
            if (entity.gameObject.CompareTag(LightTag))
            {
                var light = entity.GetComponent<LightSource>();
                if (light != null)
                {
                    m_lightSources.Remove(light);
                    entity.onVisibilityChanged -= OnEntityVisibilityChanged;
                }
            }
        }

        public void RemoveLights()
        {
            foreach (var light in m_lightSources)
            {
                if (light)
                {
                    light.GetComponent<Entity>().onVisibilityChanged -= OnEntityVisibilityChanged;
                }
            }
            m_lightSources.Clear();
        }

        public float ambientIlluminance
        {
            get { return m_ambientIlluminance; }
            set
            {
                if (m_ambientIlluminance < 0)
                {
                    throw new ArgumentOutOfRangeException();
                }
                m_ambientIlluminance = value;
            }
        }

        public float ComputeIlluminance(Vector3 position, Vector3 normal)
        {
            float totalIllum = ambientIlluminance;
            foreach (var light in m_lightSources)
            {
                totalIllum += light.ComputeIlluminance(position, normal);
            }
            return totalIllum;
        }
    }
}
