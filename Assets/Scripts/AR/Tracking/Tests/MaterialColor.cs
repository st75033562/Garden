using UnityEngine;

namespace AR.Tests
{
    public class MaterialColor : MonoBehaviour
    {
        public Color m_color = Color.white;

        void Start()
        {
            GetComponent<Renderer>().material.color = m_color;
        }
    }
}
