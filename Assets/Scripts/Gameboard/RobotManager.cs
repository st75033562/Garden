using RobotSimulation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace Gameboard
{
    public class RobotManager : IRobotManager, IEnumerable<Robot>
    {
        private readonly List<Robot> m_robots = new List<Robot>();
        private readonly RobotFactory m_robotFactory;
        private readonly ObjectManager m_objectManager;
        private Gameboard m_gameboard;
        private Transform m_defaultSpawnPoint;

        public RobotManager(RobotFactory factory, ObjectManager objManager)
        {
            m_robotFactory = factory;
            m_objectManager = objManager;
        }

        internal void SetDefaultSpawnPoint(Transform transform)
        {
            m_defaultSpawnPoint = transform;
        }

        internal void SetGameboard(Gameboard gameboard)
        {
            m_gameboard = gameboard;
        }

        public Robot GetRobot(int index)
        {
            if (index < 0 || index >= m_robots.Count)
            {
                return null;
            }
            return m_robots[index];
        }

        /// <summary>
        /// remove the robot from the Gameboard
        /// </summary>
        /// <param name="index"></param>
        public void RemoveRobot(int index)
        {
            if (index < 0 || index >= robotCount)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            var robot = GetRobot(index);
            if (robot != null)
            {
                robot.GetComponent<Entity>().Destroy();
            }
            m_robots.RemoveAt(index);

            // update following robot indices
            UpdateRobotIndices(index);
        }

        public void InsertRobot(int index, Robot robot)
        {
            if (robot.robotIndex != -1)
            {
                throw new ArgumentException("invalid robot index");
            }
            if (index < 0 || index > m_robots.Count)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            m_robots.Insert(index, robot);
            UpdateRobotIndices(index);

            m_objectManager.Register(robot.GetComponent<Entity>(), true);
        }

        private void UpdateRobotIndices(int start)
        {
            for (int i = start; i < m_robots.Count; ++i)
            {
                m_robots[i].robotIndex = i;
            }
        }

        public void AddRobot(Robot robot)
        {
            InsertRobot(m_robots.Count, robot);
        }

        public void RemoveRobots()
        {
            foreach (var robot in m_robots)
            {
                if (robot)
                {
                    robot.GetComponent<Entity>().Destroy();
                }
            }
            m_robots.Clear();
        }

        /// <summary>
        /// reset all robot objects to the initial state, new robots are created if needed
        /// </summary>
        public void ResetRobots()
        {
            if (m_gameboard == null) { return; }

            for (int i = 0; i < m_gameboard.robots.Count; ++i)
            {
                Robot robot;
                if (i < m_robots.Count)
                {
                    robot = m_robots[i];
                    robot.robotIndex = i;
                }
                else
                {
                    robot = m_robotFactory.Create();
                    AddRobot(robot);
                    
                }

                var robotInfo = m_gameboard.robots[i];
                robot.GetComponent<RobotColor>().colorId = robotInfo.colorId;

                robot.transform.position = robotInfo.position;
              //  robot.transform.localScale = robotInfo.scale;
                robot.transform.rotation = Quaternion.AngleAxis(robotInfo.rotation, Vector3.up);
                robot.gameObject.transform.localScale = new Vector3(0, 0, 0);
                var collider = robot.GetComponent<BoxCollider>();
                if (collider != null)
                {
                    collider.enabled = false;
                }
            }
        }

        /// <summary>
        /// create the robot at the default location, not added to the RobotManager
        /// </summary>
        public Robot CreateRobot(RobotInfo robotInfo = null)
        {
            var robot = m_robotFactory.Create();
            var entity = robot.GetComponent<Entity>();

            if (robotInfo == null)
            {
                Vector3 hitPoint;
                PhysicsUtils.GetPlacementPosition(m_defaultSpawnPoint.position.xz(), out hitPoint);
                robot.transform.position = hitPoint;
                robot.transform.rotation = m_defaultSpawnPoint.rotation;
            }
            else
            {
                entity.transform.position = robotInfo.position;
                entity.transform.eulerAngles = (robotInfo as IObjectInfo).rotation;
                entity.transform.localScale = robotInfo.scale;
                entity.GetComponent<RobotColor>().colorId = robotInfo.colorId;
            }

            entity.positional.Synchornize();

            return robot;
        }

        public void Reset()
        {
            m_defaultSpawnPoint = null;
            m_gameboard = null;
            RemoveRobots();
        }

        public IEnumerator<Robot> GetEnumerator()
        {
            return ((IEnumerable<Robot>)m_robots).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #region IRobotManager

        public int robotCount
        {
            get { return m_robots.Count; }
        }

        IRobot IRobotManager.get(int index)
        {
            return GetRobot(index);
        }

        IEnumerator<IRobot> IEnumerable<IRobot>.GetEnumerator()
        {
            return m_robots.Cast<IRobot>().GetEnumerator();
        }

        #endregion IRobotManager
    }
}
