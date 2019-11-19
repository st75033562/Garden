using System;
using System.Collections.Generic;

namespace Robomation
{
    public enum ConnectionState
    {
        Connecting,
        Connected,
        Disconnecting,
        Disconnected,
    }

    /**
     * @author akaii@kw.ac.kr (Kwang-Hyun Park)
     * 
     * ported by alex@g3games.cn
     */
    public abstract class Robot : IRobot
    {
        public event Action<Robot, ConnectionState> onConnectionStateChanged;

    	private readonly Dictionary<int, DeviceImpl> mDevices = new Dictionary<int, DeviceImpl>();

        private string mName = "";
        private readonly string mUUID;
        private readonly RobotManager mManager;
        private ConnectionState mConnState = ConnectionState.Disconnected;

        protected Robot(RobotManager manager, string uuid, string name, bool enabled)
        {
            if (manager == null)
            {
                throw new ArgumentNullException("manager");
            }

            mManager = manager;
            mUUID = uuid;
            mName = name ?? "";
            autoReconnect = true;
            isEnabled = enabled;
        }

        protected DeviceImpl addDevice(DeviceType deviceType, int id, string name, DataType dataType, int dataSize, object initialValue, float minValue, float maxValue)
        {
    		DeviceImpl device = null;
    		switch(deviceType)
    		{
    		case DeviceType.SENSOR:
    			switch(dataType)
    			{
    			case DataType.INTEGER:
    				device = new IntSensorImpl(id, name, dataSize, initialValue, (int)minValue, (int)maxValue);
    				break;
    			case DataType.FLOAT:
    				device = new FloatSensorImpl(id, name, dataSize, initialValue, minValue, maxValue);
    				break;
    			}
    			break;
    		case DeviceType.EFFECTOR:
    			switch(dataType)
    			{
    			case DataType.INTEGER:
    				device = new IntEffectorImpl(id, name, dataSize, initialValue, (int)minValue, (int)maxValue);
    				break;
    			case DataType.FLOAT:
    				device = new FloatEffectorImpl(id, name, dataSize, initialValue, minValue, maxValue);
    				break;
    			}
    			break;
    		}
            if (device != null)
                mDevices.Add(device.getId(), device);
    		return device;
        }

    	public Device findDeviceById(int deviceId)
    	{
            DeviceImpl device;
            mDevices.TryGetValue(deviceId, out device);
            return device;
    	}

        public virtual void resetDevices()
        {
            foreach (var device in mDevices.Values)
            {
                device.reset();
            }
        }

        public void updateDeviceStates()
        {
            foreach (var device in mDevices.Values)
            {
                device.updateState();
            }
        }

        public string uniqueId
        {
            get { return mUUID; }
        }

        public ConnectionState getConnectionState()
        {
            return mConnState;
        }

        internal void setConnectionState(ConnectionState state)
        {
            mConnState = state;
            if (onConnectionStateChanged != null)
            {
                onConnectionStateChanged(this, state);
            }
        }

        public void connect()
        {
            autoReconnect = true;
            mManager.connectRobot(this);
        }

        public void disconnect()
        {
            autoReconnect = false;
            mManager.disconnectRobot(this);
        }

        public string getName() { return mName; }

        public void setName(string name)
        {
            name = name ?? "";
            if (name != mName)
            {
                mName = name;
                mManager.changeRobotName(this, name);
            }
        }

        public bool isEnabled
        {
            get;
            private set;
        }

        public void enable()
        {
            isEnabled = true;
            connect();
        }

        public void disable()
        {
            isEnabled = false;
            disconnect();
        }

        internal bool autoReconnect
        {
            get;
            set;
        }

        public abstract byte[] encodeMotoringData();

        public abstract void decodeSensoryData(byte[] data);

        public abstract int sensoryPacketType { get; }

        #region IRobot

        public abstract RobotType type { get; }

        public int read(int deviceId)
        {
            var device = findDeviceById(deviceId);
            return device != null ? device.read() : 0;
        }

        public int read(int deviceId, int index)
        {
            var device = findDeviceById(deviceId);
            return device != null ? device.read(index) : 0;
        }

        public float readFloat(int deviceId)
        {
            var device = findDeviceById(deviceId);
            return device != null ? device.readFloat() : 0.0f;
        }

        public float readFloat(int deviceId, int index)
        {
            var device = findDeviceById(deviceId);
            return device != null ? device.readFloat(index) : 0.0f;
        }

        public bool write(int deviceId, int data)
        {
            var device = findDeviceById(deviceId);
            return device != null ? device.write(data) : false;
        }

        public bool write(int deviceId, int index, int data)
        {
            var device = findDeviceById(deviceId);
            return device != null ? device.write(index, data) : false;
        }

        public bool writeFloat(int deviceId, float data)
        {
            var device = findDeviceById(deviceId);
            return device != null ? device.writeFloat(data) : false;
        }

        public bool writeFloat(int deviceId, int index, float data)
        {
            var device = findDeviceById(deviceId);
            return device != null ? device.writeFloat(index, data) : false;
        }

        #endregion IRobot
    }
}
