using RobotSimulation;
using System;
using UnityEngine;

namespace Gameboard
{
    public class PointLightSource : LightSource
    {
        private const float BaseFlux = 1000;

        [SerializeField]
        private float m_flux;

        private Light m_light;
        private Canvas m_spriteCanvas;

        void Awake()
        {
            m_light = GetComponent<Light>();
            m_spriteCanvas = GetComponentInChildren<Canvas>();
            UpdateIntensity();
        }

        private void UpdateIntensity()
        {
            if (m_light)
            {
                m_light.intensity = Mathf.Log10(m_flux / BaseFlux);
            }
        }

        public override float flux
        {
            get { return m_flux; }
            set
            {
                m_flux = Mathf.Max(0, value);
                UpdateIntensity();
            }
        }
        
        public override float ComputeIlluminance(Vector3 position, Vector3 normal)
        {
            var dirToLight = transform.position - position;
            float cosine = Vector3.Dot(normal, dirToLight.normalized);
            // if the surface is backward facing
            if (cosine <= 0)
            {
                return 0.0f;
            }
            // assume the surface is small enough
            if (!Physics.Linecast(transform.position, position))
            {
                return flux * cosine / (4 * Mathf.PI * dirToLight.sqrMagnitude);
            }
            return 0.0f;
        }

        public override void OnVisibilityChanged(bool visible)
        {
            if (m_light)
            {
                m_light.enabled = visible;
            }

            if (m_spriteCanvas)
            {
                m_spriteCanvas.enabled = visible;
            }
        }
    }
}
