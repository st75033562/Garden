using System.Collections.Generic;

public class RobotRuntime
{
    private readonly List<RobotRuntimeState> m_robotRuntimeStates = new List<RobotRuntimeState>();

    public void SetStateCount(int count)
    {
        while (m_robotRuntimeStates.Count < count)
        {
            m_robotRuntimeStates.Add(new RobotRuntimeState());
        }

        m_robotRuntimeStates.RemoveRange(count, m_robotRuntimeStates.Count - count);
    }

    public void AddState()
    {
        m_robotRuntimeStates.Add(new RobotRuntimeState());
    }

    public void RemoveState(int index)
    {
        m_robotRuntimeStates.RemoveAt(index);
    }

    public void ResetStates()
    {
        foreach (var state in m_robotRuntimeStates)
        {
            state.reset();
        }
    }

    public RobotRuntimeState GetState(int index)
    {
        if (index >= 0 && index < m_robotRuntimeStates.Count)
        {
            return m_robotRuntimeStates[index];
        }
        return null;
    }
}