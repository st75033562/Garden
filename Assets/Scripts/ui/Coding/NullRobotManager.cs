using System.Collections;
using System.Collections.Generic;

public class NullRobotManager : IRobotManager
{
    public static readonly NullRobotManager instance = new NullRobotManager();

    public int robotCount
    {
        get { return 0; }
    }

    public IRobot get(int index)
    {
        return null;
    }

    public IEnumerator<IRobot> GetEnumerator()
    {
        yield break;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
