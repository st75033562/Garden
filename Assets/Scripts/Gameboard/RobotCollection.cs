using System.Collections;
using System.Collections.Generic;

namespace Gameboard
{
    public class RobotCollection : IRobotManager, IEnumerable<IRobot>, IEnumerable
    {
        private readonly List<int> m_robotIndices = new List<int>();

        public IRobotManager robotManager
        {
            get;
            set;
        }

        public void OnRobotRemoved(int robotIndex)
        {
            for (int i = 0; i < m_robotIndices.Count; ++i)
            {
                if (m_robotIndices[i] > robotIndex)
                {
                    m_robotIndices[i] = m_robotIndices[i] - 1;
                }
            }
        }
        
        public void Add(int robotIndex)
        {
            m_robotIndices.Add(robotIndex);
        }

        public void Add(IEnumerable<int> robotIndices)
        {
            m_robotIndices.AddRange(robotIndices);
        }

        public void Remove(int robotIndex)
        {
            m_robotIndices.Remove(robotIndex);
        }

        public void RemoveAll()
        {
            m_robotIndices.Clear();
        }

        public bool Contains(int robotIndex)
        {
            return m_robotIndices.Contains(robotIndex);
        }

        public int robotCount
        {
            get { return m_robotIndices.Count; }
        }

        public int getRobotIndex(int index)
        {
            return m_robotIndices[index];
        }

        public IEnumerable<int> robotIndices
        {
            get { return m_robotIndices; }
        }

        public IRobot get(int index)
        {
            if (index < 0 || index > m_robotIndices.Count || robotManager == null)
            {
                return null;
            }

            return robotManager.get(m_robotIndices[index]);
        }

        public IEnumerator<IRobot> GetEnumerator()
        {
            for (int i = 0; i < m_robotIndices.Count; i++)
            {
                yield return get(i);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
