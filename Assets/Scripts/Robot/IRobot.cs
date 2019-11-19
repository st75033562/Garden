using Robomation;

public enum RobotType
{
    Invalid = -1,
    Hamster,
    CheeseStick
}

public interface IRobot
{
    string getName();

    RobotType type { get; }

    /// <summary>
    /// read integer value from the device
    /// </summary>
    /// <param name="deviceId"></param>
    /// <returns>0 if device id is not valid</returns>
    int read(int deviceId);

    /// <summary>
    /// read integer value from the indexed device
    /// </summary>
    /// <param name="deviceId"></param>
    /// <param name="index"></param>
    /// <returns>0 if device id is not valid</returns>
    int read(int deviceId, int index);

    /// <summary>
    /// read float value from the device
    /// </summary>
    /// <param name="deviceId"></param>
    /// <returns></returns>
    float readFloat(int deviceId);

    /// <summary>
    /// read float value from the indexed device
    /// </summary>
    /// <param name="deviceId"></param>
    /// <param name="index"></param>
    /// <returns>0 if device id is not valid</returns>
    float readFloat(int deviceId, int index);

    /// <summary>
    /// write integer data to the device
    /// </summary>
    /// <param name="deviceId"></param>
    /// <returns></returns>
    bool write(int deviceId, int data);

    /// <summary>
    /// write integer data to the index device
    /// </summary>
    /// <param name="deviceId"></param>
    /// <param name="index"></param>
    /// <param name="data"></param>
    bool write(int deviceId, int index, int data);

    /// <summary>
    /// write float data to the device
    /// </summary>
    /// <param name="deviceId"></param>
    /// <param name="data"></param>
    bool writeFloat(int deviceId, float data);

    /// <summary>
    /// write float data to the indexed device
    /// </summary>
    /// <param name="deviceId"></param>
    /// <param name="index"></param>
    /// <param name="data"></param>
    bool writeFloat(int deviceId, int index, float data);

    void resetDevices();
}
