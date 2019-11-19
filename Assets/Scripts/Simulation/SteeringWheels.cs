using UnityEngine;
using UnityEngine.Assertions;

namespace RobotSimulation
{
    public class SteeringWheels : MonoBehaviour
    {
        public float m_radius;
        public SteeringSystem m_steeringSystem;
        // 0 left, 1 right
        public Transform[] m_wheels;

        void Awake()
        {
            Assert.IsTrue(m_radius > 0);
            var dir = m_wheels[0].transform.position - m_wheels[1].transform.position;
            m_steeringSystem.axleLength = dir.magnitude;
            m_steeringSystem.wheelRadius = m_radius;
        }

        void Update()
        {
            for (int i = 0; i < 2; ++i)
            {
                m_wheels[i].Rotate(Vector3.back, m_steeringSystem.GetWheelAngularSpeed(i) * Mathf.Rad2Deg * Time.deltaTime);
            }
        }
    }
}
