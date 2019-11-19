using RobotSimulation;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Gameboard
{
    class WorldApi : IWorldApi
    {
        private readonly GameboardSceneManager m_sceneManager;

        private RobotManager robotManager { get { return m_sceneManager.robotManager; } }
        private ObjectManager objectManager { get { return m_sceneManager.objectManager; } }

        public WorldApi(GameboardSceneManager sceneManager)
        {
            m_sceneManager = sceneManager;
        }

        public void SetRobotPosition(int index, Vector3 pos)
        {
            var robot = robotManager.GetRobot(index);
            if (robot != null)
            {
                robot.GetComponent<Positional>().position = pos;
            }
        }

        public Vector2 GetRobotPosition(int index)
        {
            var robot = robotManager.GetRobot(index);
            if (robot != null)
            {
                return robot.GetComponent<Positional>().position;
            }
            return Vector2.zero;
        }

        public void SetRobotRotaton(int index, float rotation)
        {
            var robot = robotManager.GetRobot(index);
            if (robot != null)
            {
                robot.GetComponent<Positional>().rotation = new Vector3(0, 0, rotation);
            }
        }

        public float GetRobotRotation(int index)
        {
            var robot = robotManager.GetRobot(index);
            if (robot != null)
            {
                return GeometryUtils.NormalizeAngle(robot.GetComponent<Positional>().rotation.z);
            }
            return 0.0f;
        }

        public AsyncRequest<int> CreateObject(int assetId)
        {
            return m_sceneManager.objectFactory.Create(
                new ObjectCreateInfo {
                    active = false,
                    assetId = assetId
                });
        }

        public void SetObjectPosition(int objectId, Vector3 pos)
        {
            var entity = objectManager.Get(objectId);
            if (entity)
            {
                objectManager.Activate(entity);
                entity.positional.position = pos;
            }
        }

        public Vector3 GetObjectPosition(int objectId)
        {
            var entity = objectManager.Get(objectId);
            if (entity && entity.positional)
            {
                return entity.positional.position;
            }
            return Vector3.zero;
        }

        public void SetObjectRotation(int objectId, float rotation)
        {
            var entity = objectManager.Get(objectId);
            if (entity && entity.positional)
            {
                entity.positional.rotation = new Vector3(0, 0, rotation);
            }
        }

        public void SetObjectRotation(int objectId, float rotationAlpha, float rotationBate, float rotationTheta)
        {
            var entity = objectManager.Get(objectId);
            if (entity && entity.positional)
            {
                entity.positional.rotation = new Vector3(rotationAlpha, rotationBate, rotationTheta);
            }
        }

        public float GetObjectRotation(int objectId)
        {
            var entity = objectManager.Get(objectId);
            if (entity && entity.positional)
            {
                return GeometryUtils.NormalizeAngle(entity.positional.rotation.z);
            }
            return 0;
        }

        public void SetObjectScale(int objectId, Vector3 scale)
        {
            var entity = objectManager.Get(objectId);
            if (entity && entity.positional)
            {
                entity.positional.localScale = scale;
            }
        }

        public void MoveObject(int objectId, float angularSpeed, float linearSpeed)
        {
            var entity = objectManager.Get(objectId);
            if (entity && entity.gameObject.activeInHierarchy && entity.motor)
            {
                entity.motor.angularSpeed = angularSpeed;
                entity.motor.linearSpeed = linearSpeed;
            }
        }

        public void SetObjectCollision(int objectId, bool enabled)
        {
            var entity = objectManager.Get(objectId);
            if (entity)
            {
                entity.EnableCollision(enabled);
            }
        }

        public void SetObjectMass(int objectId, float mass)
        {
            var entity = objectManager.Get(objectId);
            if (entity)
            {
                var rigidbody = entity.GetComponent<Rigidbody>();
                if (rigidbody)
                {
                    rigidbody.mass = mass;
                }
            }
        }

        public void ShowObject(int objectId, bool visible)
        {
            var entity = objectManager.Get(objectId);
            if (entity)
            {
                entity.Show(visible);
            }
        }

        public void DeleteObject(int objectId)
        {
            objectManager.Remove(objectId);
        }

        public void PlayAction(int objectId, int actionId, params string[] args)
        {
            var entity = objectManager.Get(objectId);
            if (entity && entity.gameObject.activeInHierarchy)
            {
                var actionManager = entity.GetComponent<ObjectActionManager>();
                if (actionManager)
                {
                    actionManager.Execute(actionId, args);
                }
            }
        }

        public void AttachObjectToObject(int sourceObjectId, int targetObjectId)
        {
            var sourceObj = objectManager.Get(sourceObjectId);
            var targetObj = objectManager.Get(targetObjectId);

            if (sourceObj && targetObj && sourceObj != targetObj)
            {
                // must be activated before attaching
                objectManager.Activate(sourceObj);
                targetObj.Attach(sourceObj);
            }
        }

        public void AttachObjectToRobot(int sourceObjectId, int robotIndex)
        {
            var sourceObj = objectManager.Get(sourceObjectId);
            var robot = robotManager.GetRobot(robotIndex);

            if (sourceObj && robot)
            {
                // must be activated before attaching
                objectManager.Activate(sourceObj);
                robot.GetComponent<Entity>().Attach(sourceObj);
            }
        }

        public void DetachObject(int objectId)
        {
            var entity = objectManager.Get(objectId);
            if (entity && entity.parent)
            {
                entity.parent.Detach(entity);
            }
        }

        public void PrintTextOnObject(int objectId, string text, int size, Color color)
        {
            var entity = objectManager.Get(objectId);
            if (entity)
            {
                var textObject = entity.GetComponent<TextObject>();
                if (textObject)
                {
                    textObject.SetText(text, size, color);
                }
            }
        }

        public void SetLightFlux(int objectId, float flux)
        {
            var entity = objectManager.Get(objectId);
            if (entity)
            {
                var light = entity.GetComponent<ILightSource>();
                if (light != null)
                {
                    light.flux = flux;
                }
            }
        }

        public IEnumerable<int> GetRobotCollidedObjects(int robotIndex, bool clear)
        {
            var robot = robotManager.GetRobot(robotIndex);
            if (robot)
            {
                var coll = robot.GetComponent<EntityCollisionEvent>();
                var objectIds = coll.GetCollidedObjects().ToArray();
                if (clear)
                {
                    coll.ClearEvents();
                }
                return objectIds;
            }
            return Enumerable.Empty<int>();
        }

        public IEnumerable<int> GetObjectCollidedObjects(int objectId, bool clear)
        {
            var entity = objectManager.Get(objectId);
            if (entity)
            {
                var coll = entity.GetComponent<EntityCollisionEvent>();
                if (coll)
                {
                    var objectIds = coll.GetCollidedObjects().ToArray();
                    if (clear)
                    {
                        coll.ClearEvents();
                    }
                    return objectIds;
                }
            }
            return Enumerable.Empty<int>();
        }

        public int TakeObjectFirstCollidedObject(int objectId)
        {
            var entity = objectManager.Get(objectId);
            if (entity)
            {
                var coll = entity.GetComponent<EntityCollisionEvent>();
                if (coll)
                {
                    return coll.TakeFirstCollidedObject();
                }
            }
            return 0;
        }

        public void RotateTowards(int objectId, Vector2 pos)
        {
            var entity = objectManager.Get(objectId);
            if (entity && entity.gameObject.activeInHierarchy && entity.positional)
            {
                var dir = pos - entity.positional.position.xy();
                var angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                entity.positional.rotation = new Vector3(0, 0, angle - 90);
            }
        }

        public void MoveTowards(int objectId, int targetId)
        {
            if (objectId == targetId)
            {
                return;
            }

            var entity = objectManager.Get(objectId);
            var target = objectManager.Get(targetId);
            if (entity && entity.gameObject.activeInHierarchy && entity.motor)
            {
                entity.motor.target = target;
            }
        }

        public void SetTrigger(int objectId, bool isTrigger)
        {
            var entity = objectManager.Get(objectId);
            if (entity)
            {
                entity.SetTrigger(isTrigger);
            }
        }
    }
}
