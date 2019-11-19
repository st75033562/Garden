namespace Robomation.BLE
{
    class RobotConnectionState
    {
        public bool canSendMotoringPacket
        {
            get;
            set;
        }

        public float nextConnectionDelay
        {
            get;
            set;
        }

        public int disconnectionTryCount
        {
            get;
            set;
        }

        public int reconnectionCount
        {
            get;
            set;
        }

        public void reset()
        {
            canSendMotoringPacket = false;
            nextConnectionDelay = 0;
            disconnectionTryCount = 0;
            reconnectionCount = 0;
        }
    }
}
