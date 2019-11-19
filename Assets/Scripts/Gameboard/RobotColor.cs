using System;
using UnityEngine;

namespace Gameboard
{
    public class RobotColor : MonoBehaviour
    {
        [SerializeField]
        private Renderer m_renderer;

        public int materialIndex;

        private int m_colorId;
        private RobotColorSettings m_colorSettings;

        public void Initialize(RobotColorSettings colorSettings)
        {
            m_colorSettings = colorSettings;
            colorId = 0;
        }

        public int colorId
        {
            get { return m_colorId; }
            set
            {
                m_colorId = value;
                UpdateMaterial();
            }
        }

        public void SetColor(int colorId, bool updateMaterial)
        {
            m_colorId = colorId;
            if (updateMaterial)
            {
                UpdateMaterial();
            }
        }

        public void UpdateMaterial()
        {
            var materials = m_renderer.sharedMaterials;
            materials[materialIndex] = m_colorSettings.materials[m_colorId];
            m_renderer.sharedMaterials = materials;
        }
    }
}
