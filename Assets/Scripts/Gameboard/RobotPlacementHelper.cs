using RobotSimulation;
using System;
using UnityEngine;

namespace Gameboard
{
    public class RobotPlacementHelper : MonoBehaviour
    {
        public EffectSwitcher invalidPlacementEffect;
        public GameboardSceneManager gameboardSceneManager;

        private Robot m_robot;
        private bool m_validPlacement = true;
        private BoxCollider m_collider;

        public void SetRobot(Robot robot)
        {
            if (m_robot)
            {
                invalidPlacementEffect.Restore();
                EnableCollision(true);
            }

            m_robot = robot;
            m_validPlacement = true;
            invalidPlacementEffect.target = robot != null ? robot.gameObject : null;

            if (m_robot)
            {
                m_collider = m_robot.GetComponent<BoxCollider>();
                EnableCollision(false);

                SetTransform(m_robot.transform.position.xz(), m_robot.transform.eulerAngles.y);
            }
        }

        public Robot GetRobot()
        {
            return m_robot;
        }

        private void EnableCollision(bool enabled)
        {
            var rigidbody = m_robot.GetComponent<Rigidbody>();
            rigidbody.detectCollisions = enabled;
            rigidbody.isKinematic = !enabled;
        }

        public void SetTransform(Vector2 position, float rotation)
        {
            if (m_robot == null)
            {
                throw new InvalidOperationException("robot not set");
            }

            position = GetValidPlacementPosition(position, rotation);

            Vector3 placementPosition;
            bool hit = PhysicsUtils.GetPlacementPosition(position, out placementPosition);
            if (!hit)
            {
                Debug.LogError("no valid position found on Floor");
            }
            m_robot.transform.position = placementPosition;

            var bounds = m_collider.bounds;
            bool overlapped = Physics.CheckBox(bounds.center, bounds.size * 0.5f,
                                               m_robot.transform.rotation,
                                               ~(1 << PhysicsUtils.PlacementLayer));
            if (overlapped && m_validPlacement)
            {
                m_validPlacement = false;
                invalidPlacementEffect.Apply();
            }
            else if (!overlapped && !m_validPlacement)
            {
                m_validPlacement = true;
                invalidPlacementEffect.Restore();
            }
        }

        private Vector2 GetValidPlacementPosition(Vector2 position, float rotation)
        {
            m_robot.transform.rotation = Quaternion.AngleAxis(rotation, Vector3.up);
            m_robot.transform.position = position.xzAtY(m_robot.transform.position.y);

            // restrict the robot within the placement area
            var robotBounds = m_collider.bounds;
            var placementBounds = gameboardSceneManager.robotPlacementBounds;

            float offsetX = 0;
            if (robotBounds.min.x < placementBounds.min.x)
            {
                offsetX = placementBounds.min.x - robotBounds.min.x;
            }
            else if (robotBounds.max.x > placementBounds.max.x)
            {
                offsetX = placementBounds.max.x - robotBounds.max.x;
            }

            float offsetZ = 0;
            if (robotBounds.max.z < placementBounds.min.z)
            {
                offsetZ = placementBounds.min.z - robotBounds.max.z;
            }
            else if (robotBounds.min.z > placementBounds.max.z)
            {
                offsetZ = placementBounds.max.z - robotBounds.min.z;
            }

            position.x += offsetX;
            position.y += offsetZ;

            return position;
        }

        public bool isValidPlacement
        {
            get { return m_validPlacement; }
        }
    }
}
