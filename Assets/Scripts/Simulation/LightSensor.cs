using UnityEngine;

namespace RobotSimulation
{
    public class LightSensor : MonoBehaviour
    {
        private const int MaxValue = 65000;

        public ILighting lighting
        {
            get;
            set;
        }

        public int value
        {
            get;
            private set;
        }

        void Update()
        {
            if (lighting != null)
            {
                value = (int)lighting.ComputeIlluminance(transform.position, transform.forward);
                value = Mathf.Min(value, MaxValue);
            }
        }
    }
}
