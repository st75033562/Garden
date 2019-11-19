using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Robomation
{
    class RobotConnectionNull : IRobotConnection
    {
        public void initialize(Action<bool> onInitialized)
        {
            onInitialized(false);
        }

        public void uninitialize()
        {
        }

        public void reset(Action<bool> onReset)
        {
            onReset(false);
        }

        public RobotManager.Error startScan()
        {
            return RobotManager.Error.NotReady;
        }

        public void stopScan()
        {
        }

        public bool scanning
        {
            get { return false; }
        }

        public void disconnect(Robot robot)
        {
        }

        public void connect(Robot robot, bool reconnect)
        {
        }

        public bool enable()
        {
            return false;
        }

        public bool enabled
        {
            get { return false; }
        }

        public void onRobotRemoved(Robot robot)
        {
        }

        public void update()
        {
        }
    }
}
