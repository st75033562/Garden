using System;

namespace Robomation
{
    // robot connection manager
    interface IRobotConnection
    {
        // initialize the connection, callback won't be null
        void initialize(Action<bool> onInitialized);

        void uninitialize();

        // reset the connection, callback won't be null
        void reset(Action<bool> onReset);

        // return true if scanning
        RobotManager.Error startScan();

        void stopScan();

        bool scanning { get; }

        void connect(Robot robot, bool reconnect);

        void disconnect(Robot robot);

        // the robot was removed from the manager
        void onRobotRemoved(Robot robot);

        // enable the connection in case it was disabled externally
        // enabling is asynchronous and depending on the system.
        // return false if immediate error occurred
        // 
        // e.g. on ios, BLE cannot be enabled programmatically,
        // do not expect `enabled' returns true immediately after `enable' is called
        bool enable();

        bool enabled { get; }

        void update();
    }
}