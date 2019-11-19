using UnityEngine;
using System;
using System.Collections;

namespace RobotSimulation
{
    public class TestMovement : MonoBehaviour
    {
        [Serializable]
        public class Move
        {
            public float left;
            public float right;
            public float wait;
        }

        public Robot m_robot;
        public Move[] m_moves;

        private Rigidbody m_rigidBody;

        Vector3 m_initialPos;
        Quaternion m_initialRot;


        void Awake()
        {
            m_rigidBody = m_robot.GetComponent<Rigidbody>();
            m_initialPos = m_rigidBody.position;
            m_initialRot = m_rigidBody.rotation;
        }

        IEnumerator DoFixedUpdate()
        {
            foreach (var m in m_moves)
            {
                m_robot.steeringSystem.SetWheelValue(0, m.left);
                m_robot.steeringSystem.SetWheelValue(1, m.right);

                float time = m.wait;
                while (time > 0)
                {
                    yield return new WaitForFixedUpdate();
                    time -= Time.fixedDeltaTime;
                }
            }

            Debug.LogFormat("rigid position: {0}, rotation: {1}", m_rigidBody.position.Format(4), m_rigidBody.rotation.eulerAngles.y);
            Debug.LogFormat("transform position: {0}, rotation: {1}", m_rigidBody.transform.position.Format(4), m_rigidBody.transform.rotation.eulerAngles.y);

            m_robot.steeringSystem.SetWheelValue(0, 0);
            m_robot.steeringSystem.SetWheelValue(1, 0);
        }

        // Use this for initialization
        IEnumerator Start()
        {
            while (true)
            {
                yield return StartCoroutine(DoFixedUpdate());
                yield return new WaitForSeconds(3.0f);
                yield return new WaitForFixedUpdate();

                m_rigidBody.position = m_initialPos;
                m_rigidBody.rotation = m_initialRot;
            }
        }
    }
}