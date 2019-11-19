using System.Linq;
using UnityEngine;

namespace Gameboard
{
    public class RobotColorSettings : ScriptableObject
    {
        private Color[] m_colors;

        public Material[] materials;
        public Sprite[] sprites;

        void Awake()
        {
            if (materials != null)
            {
                m_colors = materials.Select(x => x.color).ToArray();
            }
        }

        public int colorCount
        {
            get { return m_colors.Length; }
        }

        public Color GetColor(int index)
        {
            return m_colors[index];
        }
    }
}
