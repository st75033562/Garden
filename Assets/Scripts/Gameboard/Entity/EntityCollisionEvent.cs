using RobotSimulation;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Gameboard
{
    public class EntityCollisionEvent : MonoBehaviour, IProjectileHitHandler
    {
        private const float EventLifeTime = 0.5f;

        private Entity m_entity;

        private class Event
        {
            public int collidedObjectId;
            public float lifeTime;
        }

        private readonly List<Event> m_events = new List<Event>();
        private ObjectManager m_objManager;

        void Start()
        {
            m_entity = GetComponent<Entity>();
            m_entity.onCollisionStateChanged += OnCollisionStateChanged;
            m_objManager = m_entity.objectManager;

            foreach (var source in GetComponentsInChildren<CollisionEventSource>())
            {
                source.collisionEnter.AddListener(OnCollisionEnter);
                source.collisionStay.AddListener(OnCollisionStay);
                source.triggerEnter.AddListener(OnTriggerEnter);
                source.triggerStay.AddListener(OnTriggerStay);
            }
        }

        public void ClearEvents()
        {
            m_events.Clear();
        }

        private void RemoveDeadEntityEvents()
        {
            m_events.RemoveAll(x => x.collidedObjectId != 0 && m_objManager.Get(x.collidedObjectId) == null);
        }

        public void RemoveEvents(int collidedObjectId)
        {
            m_events.RemoveAll(x => x.collidedObjectId == collidedObjectId);
        }

        public IEnumerable<int> GetCollidedObjects()
        {
            RemoveDeadEntityEvents();
            return m_events.Select(x => x.collidedObjectId);
        }

        public int TakeFirstCollidedObject()
        {
            RemoveDeadEntityEvents();

            int id = -1;
            if (m_events.Count > 0)
            {
                id = m_events[0].collidedObjectId;
                m_events.RemoveAt(0);
            }
            return id;
        }

        public bool HasCollision()
        {
            RemoveDeadEntityEvents();
            return m_events.Count > 0;
        }

        private void OnCollisionStateChanged(Entity entity)
        {
            if (!entity.isCollisionEnabledInHierarchy)
            {
                ClearEvents();
            }
        }

        void Update()
        {
            for (int i = 0; i < m_events.Count; )
            {
                m_events[i].lifeTime -= Time.deltaTime;
                if (m_events[i].lifeTime <= 0)
                {
                    m_events.RemoveAt(i);
                }
                else
                {
                    ++i;
                }
            }
        }

        void OnCollisionEnter(Collision collision)
        {
            AddCollisionEvent(collision.gameObject);
        }

        void OnCollisionStay(Collision collision)
        {
            AddCollisionEvent(collision.gameObject);
        }

        void OnTriggerEnter(Collider other)
        {
            AddCollisionEvent(other.gameObject);
        }

        void OnTriggerStay(Collider other)
        {
            AddCollisionEvent(other.gameObject);
        }

        void AddCollisionEvent(GameObject other, bool unique = true)
        {
            if (PhysicsUtils.IsFloor(other) || m_entity.id == 0)
            {
                return;
            }

            var otherEntity = other.GetComponentInParent<Entity>();
            var collidedObjectId = otherEntity ? otherEntity.id : 0;
            if (unique)
            {
                var collEvent = m_events.Find(x => x.collidedObjectId == collidedObjectId);
                if (collEvent != null)
                {
                    collEvent.lifeTime = EventLifeTime;
                    return;
                }
            }
            m_events.Add(new Event {
                collidedObjectId = collidedObjectId,
                lifeTime = EventLifeTime
            });
        }

        public void OnHit(GameObject other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }
            AddCollisionEvent(other, false);
        }
    }
}
