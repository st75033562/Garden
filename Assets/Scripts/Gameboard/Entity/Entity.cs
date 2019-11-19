using AssetBundles;
using DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace Gameboard
{
    public class Entity : MonoBehaviour
    {
        public event Action<Entity> onVisibilityChanged;
        public event Action<Entity> onCollisionStateChanged;
        public event Action<Entity> onParentChanged;

        [Flags]
        private enum UpdateFlags
        {
            None       = 0,
            Visibility = 1 << 0,
            Collision  = 1 << 1,
            Trigger    = 1 << 2,
            All        = ~0,
        }

        private readonly List<Entity> m_children = new List<Entity>();

        private Rigidbody m_rigidBody;
        private bool m_useGravity;
        private bool m_isTrigger;
        private bool m_isKinematic;

        private Renderer[] m_renderers;
        private Graphic[] m_graphics;
        private IVisible[] m_visibles;
        private Collider[] m_colliders;
        private RigidBodyState m_savedRigidBodyState;

        public void Initialize()
        {
            m_rigidBody = GetComponent<Rigidbody>();
            if (m_rigidBody)
            {
                m_useGravity = m_rigidBody.useGravity;
                m_isKinematic = m_rigidBody.isKinematic;
            }

            motor = GetComponent<Motor>();
            positional = GetComponent<Positional>();

            isSelfCollisionEnabled = isCollisionEnabledInHierarchy = true;

            isSelfVisible = isVisibleInHierarchy = true;
            m_renderers = GetComponentsInChildren<Renderer>();
            m_graphics = GetComponentsInChildren<Graphic>();
            m_visibles = GetComponentsInChildren<IVisible>();
            m_colliders = GetComponentsInChildren<Collider>();

#if UNITY_EDITOR
            Assert.IsTrue(m_colliders.All(x => x.isTrigger == m_colliders[0].isTrigger));
#endif
            if (m_colliders.Length > 0)
            {
                // all colliders should be the same type
                SetTrigger(m_colliders[0].isTrigger);
            }
        }

        protected virtual void OnDestroy()
        {
            if (asset != null)
            {
                AssetBundleManager.UnloadAssetBundle(asset.bundleName, false);
            }
        }

        // id if any
        public int id { get; set; }

        // for tracking loaded asset bundles
        public BundleAssetData asset { get; set; }

        public string entityName { get; set; }

        public Positional positional { get; private set; }

        // motor for driving entity, may be null
        public Motor motor { get; private set; }

        public Entity parent { get; private set; }

        public Transform sceneRoot { get; set; }

        public ObjectManager objectManager { get; set; }

        public bool isVisibleInHierarchy
        {
            get;
            private set;
        }

        public bool isSelfVisible { get; private set; }

        private bool isParentVisibleInHierarchy
        {
            get { return parent ? parent.isVisibleInHierarchy : true; }
        }

        public bool isCollisionEnabledInHierarchy { get; private set; }

        public bool isSelfCollisionEnabled { get; private set; }

        public void EnableCollision(bool state)
        {
            if (isSelfCollisionEnabled != state)
            {
                isSelfCollisionEnabled = state;
                UpdateHierachicalStates(UpdateFlags.Collision);
            }
        }

        public bool isTriggerInHierarchy
        {
            get;
            private set;
        }

        public void SetTrigger(bool isTrigger)
        {
            m_isTrigger = isTrigger;
            UpdateHierachicalStates(UpdateFlags.Trigger);
        }

        public void Attach(Entity child)
        {
            if (child == null)
            {
                throw new ArgumentNullException("child");
            }

            if (child == this)
            {
                throw new InvalidOperationException("cannot attach to self");
            }

            if (child.parent)
            {
                child.parent.Detach(child);
            }

            child.parent = this;
            child.transform.SetParent(transform);
            m_children.Add(child);

            child.UpdateHierachicalStates(UpdateFlags.All);

            if (child.onParentChanged != null)
            {
                child.onParentChanged(this);
            }
        }

        private void UpdateHierachicalStates(UpdateFlags flag)
        {
            if ((flag & UpdateFlags.Visibility) != 0)
            {
                UpdateVisibility();
            }
            if ((flag & (UpdateFlags.Collision | UpdateFlags.Trigger)) != 0)
            {
                UpdateCollisionState(flag);
            }

            foreach (var child in m_children)
            {
                child.UpdateHierachicalStates(flag);
            }
        }

        // update visibility of the current entity
        private void UpdateVisibility()
        {
            var newVisibility = isParentVisibleInHierarchy & isSelfVisible;
            if (newVisibility == isVisibleInHierarchy)
            {
                return;
            }

            isVisibleInHierarchy = newVisibility;

            foreach (var render in m_renderers)
            {
                render.enabled = isVisibleInHierarchy;
            }

            foreach (var graphic in m_graphics)
            {
                graphic.enabled = isVisibleInHierarchy;
            }

            foreach (var visible in m_visibles)
            {
                visible.OnVisibilityChanged(isVisibleInHierarchy);
            }

            if (onVisibilityChanged != null)
            {
                onVisibilityChanged(this);
            }
        }

        private void UpdateCollisionState(UpdateFlags flag)
        {
            var oldCollState = isCollisionEnabledInHierarchy;
            isCollisionEnabledInHierarchy = (parent ? parent.isCollisionEnabledInHierarchy : true) & isSelfCollisionEnabled;
            isTriggerInHierarchy = (parent ? parent.isTriggerInHierarchy : true) & m_isTrigger;

            foreach (var collider in m_colliders)
            {
                collider.enabled = isCollisionEnabledInHierarchy;
                collider.isTrigger = isTriggerInHierarchy;
            }

            UpdateRigidBody(flag);

            if (oldCollState != isCollisionEnabledInHierarchy && onCollisionStateChanged != null)
            {
                onCollisionStateChanged(this);
            }
        }

        private void UpdateRigidBody(UpdateFlags flag)
        {
            if (parent && m_rigidBody)
            {
                // We want to disable the rigid body while keeping the collision.
                // Since there's no way to disable a rigid body directly, we have to destroy it.
                // save the state, we will need to re-create the rigid body after detaching the child
                m_savedRigidBodyState = new RigidBodyState(m_rigidBody);
                DestroyImmediate(m_rigidBody);
                m_rigidBody = null;
            }
            else if (!parent && m_savedRigidBodyState != null && isCollisionEnabledInHierarchy)
            {
                m_rigidBody = gameObject.AddComponent<Rigidbody>();
                m_savedRigidBodyState.ApplyTo(m_rigidBody);
                m_savedRigidBodyState = null;
            }

            if (m_rigidBody)
            {
                m_rigidBody.isKinematic = isTriggerInHierarchy || m_isKinematic;
                m_rigidBody.useGravity = isCollisionEnabledInHierarchy ? m_useGravity : false;
                if (!m_rigidBody.useGravity && (flag & UpdateFlags.Collision) != 0)
                {
                    m_rigidBody.velocity = Vector3.zero;
                }
            }
        }

        public void Detach(Entity child)
        {
            if (child == null)
            {
                throw new ArgumentNullException("child");
            }

            if (!child.parent)
            {
                throw new InvalidOperationException("no parent");
            }

            if (child.parent != this)
            {
                throw new InvalidOperationException("child is not attached the entity");
            }

            child.parent = null;
            child.transform.SetParent(sceneRoot);
            m_children.Remove(child);

            child.UpdateHierachicalStates(UpdateFlags.All);

            if (child.onParentChanged != null)
            {
                child.onParentChanged(this);
            }
        }

        public int childCount
        {
            get { return m_children.Count; }
        }

        public Entity GetChild(int index)
        {
            return m_children[index];
        }

        public void Show(bool visible)
        {
            if (isSelfVisible != visible)
            {
                isSelfVisible = visible;
                UpdateHierachicalStates(UpdateFlags.Visibility);
            }
        }

        /// <summary>
        /// destroy the entity and all its children.
        /// <remarks>To destroy an entity, do not call GameObject.Destroy.</remarks>
        /// </summary>
        public void Destroy()
        {
            if (parent)
            {
                parent.Detach(this);
            }

            for (int i = childCount - 1; i >= 0; --i)
            {
                var child = GetChild(i);
                child.Destroy();
            }
            Destroy(gameObject);

            if (objectManager != null)
            {
                objectManager.Unregister(this);
                objectManager = null;
            }
        }
    }
}
