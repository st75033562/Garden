using UnityEngine;

namespace Gameboard
{
    public class Motor : MonoBehaviour
    {
        private Rigidbody m_rigidBody;

        void Awake()
        {
            m_rigidBody = GetComponent<Rigidbody>();
            if (m_rigidBody)
            {
                GetComponent<Entity>().onParentChanged += OnParentChanged;
            }
        }

        private void OnParentChanged(Entity entity)
        {
            m_rigidBody = GetComponent<Rigidbody>();
        }

        public float angularSpeed { get; set; }

        public float linearSpeed { get; set; }

        public Entity target { get; set; }

        /// <summary>
        /// stop the motor, angular/linear speed are set to 0
        /// </summary>
        public void Stop()
        {
            angularSpeed = linearSpeed = 0.0f;
        }

        void Update()
        {
            if (!m_rigidBody || m_rigidBody.isKinematic)
            {
                float maxDist = float.MaxValue;
                if (target)
                {
                    var forward = target.transform.position - transform.position;
                    forward.y = 0.0f;
                    if (forward.sqrMagnitude >= 1e-6f)
                    {
                        transform.forward = forward;
                        maxDist = forward.magnitude;
                    }
                    else
                    {
                        return;
                    }
                }
                else if (angularSpeed != 0)
                {
                    transform.eulerAngles += new Vector3(0, Time.deltaTime * -angularSpeed, 0.0f);
                }

                if (linearSpeed != 0)
                {
                    var dist = Mathf.Min(linearSpeed * Time.deltaTime, maxDist);
                    transform.position += transform.forward * dist;
                }
            }
        }

        void FixedUpdate()
        {
            if (m_rigidBody && !m_rigidBody.isKinematic)
            {
                float maxDist = float.MaxValue;
                if (target)
                {
                    Vector3 dir = Vector3.forward;
                    dir = target.transform.position - m_rigidBody.position;
                    dir.y = 0.0f;
                    if (dir.sqrMagnitude >= 1e-6f)
                    {
                        m_rigidBody.rotation = Quaternion.LookRotation(dir, Vector3.up);
                        maxDist = dir.magnitude;
                    }
                    else
                    {
                        return;
                    }
                }
                else if (angularSpeed != 0)
                {
                    var rot = Quaternion.Euler(0, Time.fixedDeltaTime * -angularSpeed, 0);
                    rot *= m_rigidBody.rotation;
                    m_rigidBody.MoveRotation(rot);
                }
                else
                {
                    m_rigidBody.angularVelocity = Vector3.zero;
                }

                if (linearSpeed != 0)
                {
                    var forward = m_rigidBody.rotation * Vector3.forward;
                    forward.y = 0;

                    var dist = Mathf.Min(linearSpeed * Time.fixedDeltaTime, maxDist);
                    m_rigidBody.MovePosition(m_rigidBody.position + forward * dist);
                }
                else
                {
                    m_rigidBody.velocity = Vector3.zero;
                }
            }
        }
    }
}
