namespace Robomation.BLE
{
    public interface IBLEConnection
    {
        /// <summary>
        /// minimum RSSI for scanning robot
        /// </summary>
        int minRSSI { get; set; }
    }
}
