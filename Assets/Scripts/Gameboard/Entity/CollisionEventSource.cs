using System;
using UnityEngine;
using UnityEngine.Events;

namespace Gameboard
{
    public class CollisionEventSource : MonoBehaviour
    {
        [Serializable]
        public class CollisionEvent : UnityEvent<Collision> { }

        [Serializable]
        public class TriggerEvent : UnityEvent<Collider> { }

        [SerializeField]
        private CollisionEvent m_collisionEnter = new CollisionEvent();

        [SerializeField]
        private CollisionEvent m_collisionExit = new CollisionEvent();

        [SerializeField]
        private CollisionEvent m_collisionStay = new CollisionEvent();

        [SerializeField]
        private TriggerEvent m_triggerEnter = new TriggerEvent();

        [SerializeField]
        private TriggerEvent m_triggerExit = new TriggerEvent();

        [SerializeField]
        private TriggerEvent m_triggerStay = new TriggerEvent();

        public CollisionEvent collisionEnter { get { return m_collisionEnter; } }
        public CollisionEvent collisionStay { get { return m_collisionStay; } }
        public CollisionEvent collisionExit { get { return m_collisionExit; } }

        public TriggerEvent triggerEnter { get { return m_triggerEnter; } }
        public TriggerEvent triggerStay { get { return m_triggerStay; } }
        public TriggerEvent triggerExit { get { return m_triggerExit; } }

        void OnCollisionEnter(Collision collision)
        {
            m_collisionEnter.Invoke(collision);
        }

        void OnCollisionStay(Collision collision)
        {
            m_collisionStay.Invoke(collision);
        }

        void OnCollisionExit(Collision collision)
        {
            m_collisionExit.Invoke(collision);
        }

        void OnTriggerEnter(Collider collider)
        {
            m_triggerEnter.Invoke(collider);
        }

        void OnTriggerStay(Collider collider)
        {
            m_triggerStay.Invoke(collider);
        }

        void OnTriggerExit(Collider collider)
        {
            m_triggerExit.Invoke(collider);
        }
    }
}
