using Robomation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// only contains connected robots, all other robots will be removed from the underlying manager
/// </summary>
public class UIConnectedRobots : IEnumerable<Robot>
{
    public event Action<Robot> onRobotAdded;
    public event Action<Robot> onRobotRemoved;

    private readonly RobotManager m_manager;
    private readonly List<Robot> m_robots = new List<Robot>();
    private bool m_removingRobot;

    public UIConnectedRobots(RobotManager manager)
    {
        m_manager = manager;
        manager.onRobotStateChanged += OnRobotStateChanged;

        m_robots.AddRange(manager.robots.Where(x => x.getConnectionState() == ConnectionState.Connected));
    }

    private void OnRobotStateChanged(Robot robot)
    {
        if (robot.getConnectionState() == ConnectionState.Connected)
        {
            if (!m_robots.Contains(robot))
            {
                if (m_manager.scanning)
                {
                    m_robots.Add(robot);
                    if (onRobotAdded != null)
                    {
                        onRobotAdded(robot);
                    }
                }
                else
                {
                    SafeRemove(robot);
                }
            }
        }
        else if (robot.getConnectionState() == ConnectionState.Disconnecting ||
                 robot.getConnectionState() == ConnectionState.Disconnected)
        {
            if (m_robots.Contains(robot))
            {
                Debug.Log("removing robot: " + robot.uniqueId);
                Remove(robot);
            }
            else if (m_manager.find(robot.uniqueId) != null)
            {
                Debug.Log("disconnecting other robot: " + robot.uniqueId);
                SafeRemove(robot);
            }
        }
    }

    public IList<Robot> Robots
    {
        get { return m_robots; }
    }

    public void Remove(Robot robot)
    {
        if (!m_robots.Contains(robot))
        {
            throw new InvalidOperationException();
        }

        m_robots.Remove(robot);
        // make sure the disconnected robot won't block other robots
        SafeRemove(robot);
        if (onRobotRemoved != null)
        {
            onRobotRemoved(robot);
        }
    }

    private void SafeRemove(Robot robot)
    {
        // avoid double remove since the robot will be Disconnecting when being removed
        if (!m_removingRobot)
        {
            m_removingRobot = true;
            m_manager.remove(robot);
            m_removingRobot = false;
        }
    }

    public void Clear()
    {
        m_robots.Clear();
    }

    public void Dispose()
    {
        m_manager.onRobotStateChanged -= OnRobotStateChanged;
        m_robots.Clear();
    }

    public IEnumerator<Robot> GetEnumerator()
    {
        return m_robots.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
