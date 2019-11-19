using Robomation;

public static class RobotExtension
{
    public static int read(this Robot robot, int deviceId)
    {
        var device = robot.findDeviceById(deviceId);
        return device != null ? device.read() : 0;
    }

    public static int read(this Robot robot, int deviceId, int index)
    {
        var device = robot.findDeviceById(deviceId);
        return device != null ? device.read(index) : 0;
    }

    public static float readFloat(this Robot robot, int deviceId)
    {
        var device = robot.findDeviceById(deviceId);
        return device != null ? device.readFloat() : 0.0f;
    }

    public static float readFloat(this Robot robot, int deviceId, int index)
    {
        var device = robot.findDeviceById(deviceId);
        return device != null ? device.readFloat(index) : 0.0f;
    }

    public static bool write(this Robot robot, int deviceId, int data)
    {
        var device = robot.findDeviceById(deviceId);
        return device != null ? device.write(data) : false;
    }

    public static bool write(this Robot robot, int deviceId, int index, int data)
    {
        var device = robot.findDeviceById(deviceId);
        return device != null ? device.write(index, data) : false;
    }

    public static bool writeFloat(this Robot robot, int deviceId, float data)
    {
        var device = robot.findDeviceById(deviceId);
        return device != null ? device.writeFloat(data) : false;
    }

    public static bool writeFloat(this Robot robot, int deviceId, int index, float data)
    {
        var device = robot.findDeviceById(deviceId);
        return device != null ? device.writeFloat(index, data) : false;
    }
}
