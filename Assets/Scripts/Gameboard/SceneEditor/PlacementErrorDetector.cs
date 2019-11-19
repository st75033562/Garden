using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Gameboard
{
    // used to detect if collision occurs between objects where at least one object is rigid body
    [RequireComponent(typeof(EffectSwitcher))]
    public class PlacementErrorDetector : MonoBehaviour
    {
        private int m_collisions;

        public bool hasError { get { return m_collisions > 0; } }

        private bool m_hasRigidBody;
        private EffectSwitcher m_errorEffect;
        private RobotColor m_robotColor;
        private readonly List<PlacementErrorDetector> m_collidedDetectors = new List<PlacementErrorDetector>();
        private bool m_isTrigger;

        void Awake()
        {
            m_hasRigidBody = GetComponent<Rigidbody>() != null;
            m_errorEffect = GetComponent<EffectSwitcher>();
            m_errorEffect.target = gameObject;
            m_robotColor = GetComponent<RobotColor>();

            foreach (var source in GetComponentsInChildren<CollisionEventSource>())
            {
                source.triggerEnter.AddListener(OnTriggerEnter);
                source.triggerExit.AddListener(OnTriggerExit);
            }

            foreach (var collider in GetComponentsInChildren<Collider>())
            {
                m_isTrigger |= collider.isTrigger;
                collider.isTrigger = true;
            }
        }

        void OnDestroy()
        {
            foreach (var source in GetComponentsInChildren<CollisionEventSource>())
            {
                source.triggerEnter.RemoveListener(OnTriggerEnter);
                source.triggerExit.RemoveListener(OnTriggerExit);
            }

            // notify other detectors that we have been destroyed so that reference won't be mis-counted
            foreach (var detector in m_collidedDetectors)
            {
                if (detector.m_collidedDetectors.Remove(this))
                {
                    detector.DecreaseCollisionCount();
                }
            }
        }

        public Material errorMaterial
        {
            get { return m_errorEffect.replacementMat; }
            set { m_errorEffect.replacementMat = value; }
        }

        void OnEnable()
        {
            if (hasError)
            {
                m_errorEffect.Apply();
            }
        }

        void OnDisable()
        {
            RestoreMaterials();
        }

        void OnTriggerEnter(Collider collider)
        {
            if (!IsValidCollider(collider))
            {
                return;
            }

            var detector = collider.GetComponentInParent<PlacementErrorDetector>();
            if (detector)
            {
                m_collidedDetectors.Add(detector);
            }

            if (++m_collisions == 1 && enabled)
            {
                m_errorEffect.Apply();
            }
        }

        void OnTriggerExit(Collider collider)
        {
            if (!IsValidCollider(collider))
            {
                return;
            }

            var detector = collider.GetComponentInParent<PlacementErrorDetector>();
            if (detector)
            {
                m_collidedDetectors.Remove(detector);
            }

            DecreaseCollisionCount();
        }

        void DecreaseCollisionCount()
        {
            Assert.IsTrue(m_collisions > 0);
            if (--m_collisions == 0 && enabled)
            {
                RestoreMaterials();
            }
        }

        bool IsValidCollider(Collider collider)
        {
            var entity = collider.GetComponentInParent<Entity>();
            var root = entity ? entity.gameObject : collider.gameObject;

            if (!m_hasRigidBody && !root.GetComponent<Rigidbody>())
            {
                return false;
            }

            var detector = entity ? entity.GetComponent<PlacementErrorDetector>() : null;
            if (m_isTrigger || detector && detector.m_isTrigger)
            {
                return false;
            }

            if (entity != null && entity.asset != null && !entity.asset.runtimeCollision)
            {
                return false;
            }

            return true;
        }

        void RestoreMaterials()
        {
            m_errorEffect.Restore();
            if (m_robotColor)
            {
                // reset the material in case the color is changed after applying the error effect
                m_robotColor.UpdateMaterial();
            }
        }
    }
}
