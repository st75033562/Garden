using System;
using UnityEngine;

namespace Gameboard
{
    public class RigidBodyState
    {
        public float mass;
        public float drag;
        public float angularDrag;
        public bool useGravity;
        public bool isKinematic;
        public RigidbodyInterpolation interpolation;
        public CollisionDetectionMode collisionDetectionMode;
        public RigidbodyConstraints constraints;

        public RigidBodyState() { }

        public RigidBodyState(Rigidbody rb)
        {
            SaveFrom(rb);
        }

        public void SaveFrom(Rigidbody rb)
        {
            if (rb == null)
            {
                throw new ArgumentNullException("rb");
            }

            mass = rb.mass;
            drag = rb.drag;
            angularDrag = rb.angularDrag;
            useGravity = rb.useGravity;
            isKinematic = rb.isKinematic;
            interpolation = rb.interpolation;
            collisionDetectionMode = rb.collisionDetectionMode;
            constraints = rb.constraints;
        }

        public void ApplyTo(Rigidbody rb)
        {
            if (rb == null)
            {
                throw new ArgumentNullException("rb");
            }

            rb.mass = mass;
            rb.drag = drag;
            rb.angularDrag = angularDrag;
            rb.useGravity = useGravity;
            rb.isKinematic = isKinematic;
            rb.interpolation = interpolation;
            rb.collisionDetectionMode = collisionDetectionMode;
            rb.constraints = constraints;
        }
    }
}
