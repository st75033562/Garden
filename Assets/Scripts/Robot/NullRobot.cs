using System;

namespace Robomation
{
    public class NullRobot : IRobot
    {
        public static readonly NullRobot instance = new NullRobot();

        private NullRobot() { }

        public string getName()
        {
            return "Null";
        }

        public RobotType type
        {
            get { return RobotType.Invalid; }
        }

        public int read(int deviceId)
        {
            return 0;
        }

        public int read(int deviceId, int index)
        {
            return 0;
        }

        public float readFloat(int deviceId)
        {
            return 0;
        }

        public float readFloat(int deviceId, int index)
        {
            return 0;
        }

        public bool write(int deviceId, int data)
        {
            return false;
        }

        public bool write(int deviceId, int index, int data)
        {
            return false;
        }

        public bool writeFloat(int deviceId, float data)
        {
            return false;
        }

        public bool writeFloat(int deviceId, int index, float data)
        {
            return false;
        }

        public void resetDevices()
        {
        }
    }
}
