public static class RobotManagerExtensions
{
    public static void resetRobots(this IRobotManager manager)
    {
        for (int i = 0; i < manager.robotCount; ++i)
        {
            var robot = manager.get(i);
            if (robot != null)
            {
                robot.resetDevices();
            }
        }
    }
}
