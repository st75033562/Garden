namespace Robomation.BLE
{
    class DumpQueueCommand : IVarCommand
    {
        private readonly RobotConnectionBLE mManager;

        public DumpQueueCommand(RobotConnectionBLE manager)
        {
            mManager = manager;
        }

        public string Execute(string[] args)
        {
            return mManager.dumpRequestQueue();
        }
    }
}
