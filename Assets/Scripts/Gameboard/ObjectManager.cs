using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Gameboard
{
    public class ObjectManager
    {
        public event Action<Entity> onEntityAdded;
        public event Action<Entity> onEntityActivated;
        public event Action<Entity> onEntityRemoved;

        private int m_prevEntityId;
        private readonly Dictionary<int, Entity> m_entities = new Dictionary<int, Entity>();
        private readonly HashSet<Entity> m_inactiveEntities = new HashSet<Entity>();

        public ObjectManager(GameboardSceneManager sceneManager)
        {
            this.sceneManager = sceneManager;
        }

        public void Reset()
        {
            RemoveAll();
            m_prevEntityId = 0;
        }

        public GameboardSceneManager sceneManager { get; private set; }

        /// <summary>
        /// for undo/redo
        /// </summary>
        public int prevEntityId
        {
            get { return m_prevEntityId; }
            set
            {
                if (m_prevEntityId < 0)
                {
                    throw new ArgumentOutOfRangeException("value");
                }

#if UNITY_EDITOR
                if (m_entities.Count > 0 && value < m_entities.Values.Max(x => x.id))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
#endif
                m_prevEntityId = value;
            }
        }

        public int Register(Entity entity, bool active)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }

            if (entity.id <= 0)
            {
                entity.id = ++m_prevEntityId;
            }
            m_entities.Add(entity.id, entity);
            entity.objectManager = this;
            if (!active)
            {
                Deactivate(entity);
            }

            if (onEntityAdded != null)
            {
                onEntityAdded(entity);
            }

            if (active)
            {
                InternalActivate(entity);
            }

            return entity.id;
        }

        private void Deactivate(Entity entity)
        {
            entity.gameObject.SetActive(false);
            m_inactiveEntities.Add(entity);
        }

        public void Unregister(Entity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }

            if (m_entities.Remove(entity.id))
            {
                m_inactiveEntities.Remove(entity);
                entity.objectManager = null;

                if (onEntityRemoved != null)
                {
                    onEntityRemoved(entity);
                }
            }
        }

        /// <summary>
        /// activate the object so that the object can be interactive
        /// </summary>
        public void Activate(Entity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }

            if (m_inactiveEntities.Remove(entity))
            {
                InternalActivate(entity);
            }
        }

        public void ActivateAll()
        {
            foreach (var entity in m_inactiveEntities.ToList())
            {
                Activate(entity);
            }
        }

        private void InternalActivate(Entity entity)
        {
            entity.gameObject.SetActive(true);

            foreach (var handler in entity.GetComponents<IObjectActivationHandler>())
            {
                handler.OnActivate();
            }

            if (onEntityActivated != null)
            {
                onEntityActivated(entity);
            }
        }

        public Entity Get(int id)
        {
            Entity entity;
            m_entities.TryGetValue(id, out entity);
            return entity;
        }

        /// <summary>
        /// return the first entity with the given name
        /// </summary>
        public Entity Get(string name)
        {
            return m_entities.Values.FirstOrDefault(x => x.entityName == name);
        }

        /// <summary>
        /// remove the entity and all its children
        /// </summary>
        /// <param name="id">object id</param>
        public void Remove(int id)
        {
            var entity = Get(id);
            if (entity)
            {
                entity.Destroy();
            }
        }

        public IEnumerable<Entity> objects
        {
            get { return m_entities.Values; }
        }

        public void RemoveAll()
        {
            foreach (var entity in m_entities.Values.ToArray())
            {
                if (entity)
                {
                    entity.Destroy();
                }
            }
            m_entities.Clear();
            m_inactiveEntities.Clear();
        }
    }
}
