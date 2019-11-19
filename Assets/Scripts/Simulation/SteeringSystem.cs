using UnityEngine;
using UnityEngine.Assertions;

namespace RobotSimulation
{
    [RequireComponent(typeof(Rigidbody))]
    public class SteeringSystem : MonoBehaviour
    {
        public const float MinWheelValue = -100;
        public const float MaxWheelValue = 100;

        private const float SpeedTruncThreshold = 0.001f;

        // physical radius of wheels
        public const float PhysicalRadius = 0.015f;
        // wheel value to speed factor
        public const float PhysicalSpeedFactor = 1.053f / 1000.0f;

        public const float MinBalanceValue = -128;
        public const float MaxBalanceValue = 127;
        public const float MinBalancedRatio = 0.2f;

        public Transform[] m_wheels;

        [SerializeField]
        private float m_axleLength;

        private Rigidbody m_rigidBody;
        private Vector3 m_velocity;

        private float[] m_wheelValues = new float[2];
        private float m_balanceValue;

        private class WheelSpeed
        {
            public float linear;
            public float angular;
        }

        private WheelSpeed[] m_speed = new WheelSpeed[2];

        private readonly Vector2[] m_lastPositions = new Vector2[2];
        private readonly float[] m_groundSpeeds = new float[2];

        void Awake()
        {
            m_rigidBody = GetComponent<Rigidbody>();
            
            for (int i = 0; i < 2; ++i)
            {
                m_speed[i] = new WheelSpeed();
                m_lastPositions[i] = GetWheelPosition(i);
            }
        }

        public float axleLength
        {
            get { return m_axleLength; }
            set
            {
                Assert.IsTrue(value > 0.0f);
                m_axleLength = value;
            }
        }

        public float wheelRadius
        {
            get;
            set;
        }

        public float WheelValueToSpeed(float value)
        {
            return WheelValueToAngularSpeed(value) * wheelRadius;
        }

        /// <summary>
        /// speed in radians
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static float WheelValueToAngularSpeed(float value)
        {
            return PhysicalSpeedFactor / PhysicalRadius * value;
        }

        /// <summary>
        /// speed in world
        /// </summary>
        public float GetWheelSpeed(int wheel)
        {
            return m_speed[wheel].linear;
        }

        public float GetWheelAngularSpeed(int wheel)
        {
            return m_speed[wheel].angular;
        }

        public float GetWheelGroundSpeed(int wheel)
        {
            return m_groundSpeeds[wheel];
        }

        /// <summary>
        /// wheel input value to the robot
        /// </summary>
        public void SetWheelValue(int wheel, float value)
        {
            m_wheelValues[wheel] = Mathf.Clamp(value, MinWheelValue, MaxWheelValue);
            UpdateSpeed(wheel);
        }

        public float GetWheelValue(int index)
        {
            return m_wheelValues[index];
        }

        private void UpdateSpeed(int wheel)
        {
            float wheelValue = m_wheelValues[wheel];
            if (balanceValue < 0 && wheel == 0)
            {
                wheelValue *= Mathf.Lerp(1, MinBalancedRatio, balanceValue / MinBalanceValue);
            }
            else if (balanceValue > 0 && wheel == 1)
            {
                wheelValue *= Mathf.Lerp(1, MinBalancedRatio, balanceValue / MaxBalanceValue);
            }

            m_speed[wheel].angular = WheelValueToAngularSpeed(wheelValue);
            m_speed[wheel].linear = m_speed[wheel].angular * wheelRadius;
        }

        public Vector3 velocity
        {
            get { return m_velocity; }
        }

        /// <summary>
        /// if positive, affects the right wheel, otherwise affects the left wheel
        /// </summary>
        public float balanceValue
        {
            get { return m_balanceValue; }
            set
            {
                m_balanceValue = Mathf.Clamp(value, MinBalanceValue, MaxBalanceValue);
                UpdateSpeed(0);
                UpdateSpeed(1);
            }
        }

        void FixedUpdate()
        {
            if (Time.fixedDeltaTime == 0.0f)
            {
                return;
            }

            // calculate the ground speed
            for (int i = 0; i < 2; ++i)
            {
                var newPos = GetWheelPosition(i);
                m_groundSpeeds[i] = Mathf.Sign(GetWheelValue(i)) * (newPos - m_lastPositions[i]).magnitude / Time.fixedDeltaTime;
                if (Mathf.Abs(m_groundSpeeds[i]) < SpeedTruncThreshold)
                {
                    m_groundSpeeds[i] = 0.0f;
                }
                m_lastPositions[i] = newPos;
            }

            // we use Euler method to integrate
            // See http://rossum.sourceforge.net/papers/DiffSteer/DiffSteer.html for the formulae

            if (m_wheelValues[0] != 0 || m_wheelValues[1] != 0)
            {
                float deltaAngle = -(GetWheelSpeed(1) - GetWheelSpeed(0)) / m_axleLength * Time.fixedDeltaTime;
                var rotation = m_rigidBody.rotation;
                m_rigidBody.MoveRotation(Quaternion.AngleAxis(deltaAngle * Mathf.Rad2Deg, Vector3.up) * rotation);

                m_velocity = rotation * Vector3.forward * (GetWheelSpeed(1) + GetWheelSpeed(0)) * 0.5f;
                m_rigidBody.MovePosition(m_rigidBody.position + m_velocity * Time.fixedDeltaTime);
            }
        }

        Vector2 GetWheelPosition(int wheel)
        {
            var pos = m_rigidBody.position + m_rigidBody.rotation * m_wheels[wheel].localPosition;
            return pos.xz();
        }
    }
}
