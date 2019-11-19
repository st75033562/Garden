namespace Robomation.BLE
{
    // BLE service and characteristic UUIDs
    internal static class BLERobotConfig
    {
        public const string AdvertisingServiceUUID         = "f138";
        public const string SensorServiceUUID              = "00009001-9c80-11e3-a5e2-0800200c9a66";

        public const string DeviceNameCharacteristicUUID   = "2a00";
        public const string ManufacturerCharacteristicUUID = "2a29";
        public const string FirmwareCharacteristicUUID     = "2a26";
        public const string TxRxCharacteristicUUID         = "0000900a-9c80-11e3-a5e2-0800200c9a66";
    }
}
