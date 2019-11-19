using RobotSimulation;
using UnityEngine;

namespace Gameboard
{
    public class WorldPosition : Positional
    {
        private Rigidbody m_rigidBody;

        private Vector3 m_currentPosition;
        private Vector3 m_currentScale;
        private Vector3 m_currentRotation;
        private bool m_needUpdate;

        protected override void Awake()
        {
            base.Awake();

            m_rigidBody = GetComponent<Rigidbody>();
            m_currentPosition = transform.position;
            m_currentRotation = transform.eulerAngles;
            m_currentScale = transform.localScale;

            entity.onParentChanged += OnParentChanged;
        }

        private void OnParentChanged(Entity entity)
        {
            m_rigidBody = GetComponent<Rigidbody>();
            UpdateKinematicState();
        }

        public override Vector3 position
        {
            get
            {
                return Coordinates.ConvertVector(m_currentPosition);
            }
            set
            {
                m_currentPosition = Coordinates.ConvertVector(value);
                m_needUpdate = true;
                UpdateKinematicState();
            }
        }

        public override Vector3 rotation
        {
            get
            {
                return Coordinates.ConvertRotation(m_currentRotation);
            }
            set
            {
                m_currentRotation = Coordinates.ConvertRotation(value);
                m_needUpdate = true;
                UpdateKinematicState();
            }
        }

        public override Vector3 localScale
        {
            get
            {
                return Coordinates.ConvertVector(m_currentScale);
            }
            set
            {
                m_currentScale = Coordinates.ConvertVector(value);
                m_needUpdate = true;
                UpdateKinematicState();
            }
        }

        public override void Synchornize()
        {
            m_currentPosition = transform.position;
            m_currentScale = transform.localScale;
            m_currentRotation = transform.eulerAngles;
        }

        void UpdateKinematicState()
        {
            if (!m_rigidBody || m_rigidBody.isKinematic)
            {
                if (m_needUpdate)
                {
                    m_needUpdate = false;

                    transform.localScale = m_currentScale;
                    transform.SetPositionAndRotation(m_currentPosition, Quaternion.Euler(m_currentRotation));
                }
                else
                {
                    Synchornize();
                }
            }
        }

        void LateUpdate()
        {
            UpdateKinematicState();
        }

        void FixedUpdate()
        {
            if (m_needUpdate && m_rigidBody && !m_rigidBody.isKinematic)
            {
                m_needUpdate = false;

                // make sure the scale is set before rotation and position, otherwise position and rotation won't effect
                transform.localScale = m_currentScale;
                m_rigidBody.position = m_currentPosition;
                m_rigidBody.rotation = Quaternion.Euler(m_currentRotation);
            }

            if (!m_needUpdate && m_rigidBody)
            {
                m_currentPosition = m_rigidBody.position;
                m_currentRotation = m_rigidBody.rotation.eulerAngles;
            }
        }
    }
}
