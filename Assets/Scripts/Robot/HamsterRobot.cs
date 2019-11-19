using System;

namespace Robomation
{
    public class HamsterRobot : Robot
    {
        private DeviceImpl mLeftWheelDevice;
        private DeviceImpl mRightWheelDevice;
        private DeviceImpl mBuzzerDevice;
        private DeviceImpl mOutputADevice;
        private DeviceImpl mOutputBDevice;
        private DeviceImpl mWheelBalanceDevice;
        private DeviceImpl mTopologyDevice;
        private DeviceImpl mLeftLedDevice;
        private DeviceImpl mRightLedDevice;
        private DeviceImpl mNoteDevice;
        private DeviceImpl mLineTracerModeDevice;
        private DeviceImpl mLineTracerSpeedDevice;
        private DeviceImpl mIoModeADevice;
        private DeviceImpl mIoModeBDevice;
        private DeviceImpl mConfigProximityDevice;
        private DeviceImpl mConfigGravityDevice;
        private DeviceImpl mConfigBandWidthDevice;
        private DeviceImpl mSignalStrengthDevice;
        private DeviceImpl mLeftProximityDevice;
        private DeviceImpl mRightProximityDevice;
        private DeviceImpl mLeftFloorDevice;
        private DeviceImpl mRightFloorDevice;
        private DeviceImpl mAccelerationDevice;
        private DeviceImpl mLightDevice;
        private DeviceImpl mTemperatureDevice;
        private DeviceImpl mBatteryDevice;
        private DeviceImpl mInputADevice;
        private DeviceImpl mInputBDevice;
        private DeviceImpl mLineTracerStateDevice;

        private byte[] mWheelBalanceData;
        private int mLineTracerCommandFlag;

        public HamsterRobot(RobotManager manager, string uuid, bool enabled)
            : base(manager, uuid, Hamster.NAME, enabled)
        {
            initDevices();
        }

        private void initDevices()
        {
            mLeftWheelDevice       = addDevice(DeviceType.EFFECTOR, Hamster.LEFT_WHEEL,        "LeftWheel",       DataType.INTEGER, 1, 0, -100.0F, 100.0F);
            mRightWheelDevice      = addDevice(DeviceType.EFFECTOR, Hamster.RIGHT_WHEEL,       "RightWheel",      DataType.INTEGER, 1, 0, -100.0F, 100.0F);
            mBuzzerDevice          = addDevice(DeviceType.EFFECTOR, Hamster.BUZZER,            "Buzzer",          DataType.FLOAT, 1, 0, 0.0F, 167772.16F);
            mOutputADevice         = addDevice(DeviceType.EFFECTOR, Hamster.OUTPUT_A,          "OutputA",         DataType.INTEGER, 1, 0, 0.0F, 255.0F);
            mOutputBDevice         = addDevice(DeviceType.EFFECTOR, Hamster.OUTPUT_B,          "OutputB",         DataType.INTEGER, 1, 0, 0.0F, 255.0F);
            mWheelBalanceDevice    = addDevice(DeviceType.EFFECTOR, Hamster.WHEEL_BALANCE,     "WheelBalance",    DataType.INTEGER, 1, 0, -128.0F, 127.0F);
            mTopologyDevice        = addDevice(DeviceType.EFFECTOR, Hamster.TOPOLOGY,          "Topology",        DataType.INTEGER, 1, 0, 0.0F, 15.0F);
            mLeftLedDevice         = addDevice(DeviceType.EFFECTOR, Hamster.LEFT_LED,          "LeftLed",         DataType.INTEGER, 1, 0, 0.0F, 7.0F);
            mRightLedDevice        = addDevice(DeviceType.EFFECTOR, Hamster.RIGHT_LED,         "RightLed",        DataType.INTEGER, 1, 0, 0.0F, 7.0F);
            mNoteDevice            = addDevice(DeviceType.EFFECTOR, Hamster.NOTE,              "Note",            DataType.INTEGER, 1, 0, 0.0F, 88.0F);
            mLineTracerModeDevice  = addDevice(DeviceType.EFFECTOR, Hamster.LINE_TRACER_MODE,  "LineTracerMode",  DataType.INTEGER, 1, 0, 0.0F, 14.0F);
            mLineTracerSpeedDevice = addDevice(DeviceType.EFFECTOR, Hamster.LINE_TRACER_SPEED, "LineTracerSpeed", DataType.INTEGER, 1, 4, 0.0F, 7.0F);
            mIoModeADevice         = addDevice(DeviceType.EFFECTOR, Hamster.IO_MODE_A,         "IoModeA",         DataType.INTEGER, 1, 0, 0.0F, 15.0F);
            mIoModeBDevice         = addDevice(DeviceType.EFFECTOR, Hamster.IO_MODE_B,         "IoModeB",         DataType.INTEGER, 1, 0, 0.0F, 15.0F);
            mConfigProximityDevice = addDevice(DeviceType.EFFECTOR, Hamster.CONFIG_PROXIMITY,  "ConfigProximity", DataType.INTEGER, 1, 2, 1.0F, 7.0F);
            mConfigGravityDevice   = addDevice(DeviceType.EFFECTOR, Hamster.CONFIG_GRAVITY,    "ConfigGravity",   DataType.INTEGER, 1, 0, 0.0F, 3.0F);
            mConfigBandWidthDevice = addDevice(DeviceType.EFFECTOR, Hamster.CONFIG_BAND_WIDTH, "ConfigBandWidth", DataType.INTEGER, 1, 3, 1.0F, 8.0F);
            mSignalStrengthDevice  = addDevice(DeviceType.SENSOR,   Hamster.SIGNAL_STRENGTH,   "SignalStrength",  DataType.INTEGER, 1, 0, -128.0F, 0.0F);
            mLeftProximityDevice   = addDevice(DeviceType.SENSOR,   Hamster.LEFT_PROXIMITY,    "LeftProximity",   DataType.INTEGER, 1, 0, 0.0F, 255.0F);
            mRightProximityDevice  = addDevice(DeviceType.SENSOR,   Hamster.RIGHT_PROXIMITY,   "RightProximity",  DataType.INTEGER, 1, 0, 0.0F, 255.0F);
            mLeftFloorDevice       = addDevice(DeviceType.SENSOR,   Hamster.LEFT_FLOOR,        "LeftFloor",       DataType.INTEGER, 1, 0, 0.0F, 255.0F);
            mRightFloorDevice      = addDevice(DeviceType.SENSOR,   Hamster.RIGHT_FLOOR,       "RightFloor",      DataType.INTEGER, 1, 0, 0.0F, 255.0F);
            mAccelerationDevice    = addDevice(DeviceType.SENSOR,   Hamster.ACCELERATION,      "Acceleration",    DataType.INTEGER, 3, 0, -32768.0F, 32767.0F);
            mLightDevice           = addDevice(DeviceType.SENSOR,   Hamster.LIGHT,             "Light",           DataType.INTEGER, 1, 0, 0.0F, 65535.0F);
            mTemperatureDevice     = addDevice(DeviceType.SENSOR,   Hamster.TEMPERATURE,       "Temperature",     DataType.INTEGER, 1, 0, -40.0F, 88.0F);
            mBatteryDevice         = addDevice(DeviceType.SENSOR,   Hamster.BATTERY,           "Battery",         DataType.FLOAT, 1, 0, 0.0F, 5.0F);
            mInputADevice          = addDevice(DeviceType.SENSOR,   Hamster.INPUT_A,           "inputA",          DataType.INTEGER, 1, 0, 0.0F, 255.0F);
            mInputBDevice          = addDevice(DeviceType.SENSOR,   Hamster.INPUT_B,           "inputB",          DataType.INTEGER, 1, 0, 0.0F, 255.0F);
            mLineTracerStateDevice = addDevice(DeviceType.SENSOR,   Hamster.LINE_TRACER_STATE, "LineTracerState", DataType.INTEGER, 1, 0, 0.0F, 255.0F);
        }

        public override RobotType type
        {
            get { return RobotType.Hamster; }
        }

        public override byte[] encodeMotoringData()
        {
            if (mWheelBalanceData != null)
            {
                var data = mWheelBalanceData;
                mWheelBalanceData = null;
                return data;
            }

            return encodePacket(HamsterEffectorPacket.CommandMotoring);
        }

        protected byte[] encodePacket(byte command)
        {
            var motoringPacket = HamsterEffectorPacket.Create();
            // version is 0
            motoringPacket.topology = mTopologyDevice.read();
            motoringPacket.command = command;

#if !UNITY_IOS
            // robots connected to ios run slower than to other OSes, temporarily limit the speed
            const float SpeedMultiplier = 0.7f;
            // #TODO need to know why converting from float to byte directly produces incorrect result
            motoringPacket.leftWheel = (int)(mLeftWheelDevice.read() * SpeedMultiplier);
            motoringPacket.rightWheel = (int)(mRightWheelDevice.read() * SpeedMultiplier);
#else
            motoringPacket.leftWheel = mLeftWheelDevice.read(); 
            motoringPacket.rightWheel = mRightWheelDevice.read();
#endif
            motoringPacket.leftLed = mLeftLedDevice.read();
            motoringPacket.rightLed = mRightLedDevice.read();

            motoringPacket.buzzerFrequency = (int)(mBuzzerDevice.readFloat() * 100);
            motoringPacket.note = mNoteDevice.read();

            // if mode is written, then toggle the command bit to send mode
            if (mLineTracerModeDevice.isWritten())
            {
                mLineTracerCommandFlag ^= 1;
            }
            int lineTracerData = mLineTracerCommandFlag << 7;
            lineTracerData |= mLineTracerModeDevice.read() << 3;
            lineTracerData |= mLineTracerSpeedDevice.read();
            motoringPacket.lineTracer = (byte)lineTracerData;

            motoringPacket.configProximity = mConfigProximityDevice.read();
            motoringPacket.configGravity = mConfigGravityDevice.read();
            motoringPacket.configBandwidth = mConfigBandWidthDevice.read();
            motoringPacket.ioModeA = mIoModeADevice.read();
            motoringPacket.ioModeB = mIoModeBDevice.read();
            motoringPacket.outputA = mOutputADevice.read();
            motoringPacket.outputB = mOutputBDevice.read();
            motoringPacket.wheelBalance = mWheelBalanceDevice.read();

            // pull-up and reserved are not set

            return motoringPacket.data;
        }

        public override void decodeSensoryData(byte[] data)
        {
            var packet = new HamsterSensorPacket(data);

            mSignalStrengthDevice.put(packet.signalStrength);
            mLeftProximityDevice.put(packet.leftProximity);
            mRightProximityDevice.put(packet.rightProximity);
            mLeftFloorDevice.put(packet.leftFloor);
            mRightFloorDevice.put(packet.rightFloor);

            mAccelerationDevice.put(Hamster.ACCEL_X, packet.accelerationX);
            mAccelerationDevice.put(Hamster.ACCEL_Y, packet.accelerationY);
            mAccelerationDevice.put(Hamster.ACCEL_Z, packet.accelerationZ);

            if (packet.flagLightTemp == HamsterSensorPacket.FlagLight)
            {
                mLightDevice.put(packet.light);
            }
            else if (packet.flagLightTemp == HamsterSensorPacket.FlagTemp)
            {
                mTemperatureDevice.put(24 + packet.temperature / 2);
                mBatteryDevice.putFloat(2.0f + packet.battery / 100.0f);
            }

            mInputADevice.put(packet.inputA);
            mInputBDevice.put(packet.inputB);
            mLineTracerStateDevice.put(packet.lineTracerState);
        }

        /// <summary>
        /// update wheel balance permanently
        /// </summary>
        public void saveWheelBalance()
        {
            mWheelBalanceData = encodePacket(HamsterEffectorPacket.CommandBalance);
        }

        public override int sensoryPacketType
        {
            get { return Hamster.SENSORY_PACKET_TYPE; }
        }
    }
}
