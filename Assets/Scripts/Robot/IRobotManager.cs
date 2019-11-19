using System.Collections.Generic;

public interface IRobotManager : IEnumerable<IRobot>
{
    int robotCount { get; }

    /// <summary>
    /// get the robot at given index
    /// </summary>
    /// <param name="index"></param>
    /// <returns>null if index is not valid</returns>
    IRobot get(int index);
}
