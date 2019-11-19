using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RobotSimulation
{
    public class RobotManager : MonoBehaviour, IRobotManager, IEnumerable<Robot>
    {
        [SerializeField]
        private List<Robot> m_robots = new List<Robot>();

        public Robot Get(int index)
        {
            if (index < 0 || index >= m_robots.Count)
            {
                return null;
            }
            return m_robots[index];
        }

        public void Remove(int index)
        {
            var robot = Get(index);
            if (robot != null)
            {
                Destroy(robot.gameObject);
            }
            m_robots.RemoveAt(index);
        }

        public void Add(Robot robot)
        {
            m_robots.Add(robot);
        }

        public void RemoveAll()
        {
            foreach (var robot in m_robots)
            {
                if (robot)
                {
                    Destroy(robot.gameObject);
                }
            }
            m_robots.Clear();
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
            return Get(index);
        }

        IEnumerator<IRobot> IEnumerable<IRobot>.GetEnumerator()
        {
            return m_robots.Cast<IRobot>().GetEnumerator();
        }

        #endregion IRobotManager
    }
}
