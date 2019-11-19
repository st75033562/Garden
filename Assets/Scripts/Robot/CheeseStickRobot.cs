namespace Robomation
{
    public class CheeseStickRobot : Robot
    {
        private DeviceImpl mPianoNote;
        private DeviceImpl mSoundClip;
        private DeviceImpl mBuzz;
        private DeviceImpl mSoundOut;
        private DeviceImpl mConfigSa;
        private DeviceImpl mConfigSb;
        private DeviceImpl mConfigSc;
        private DeviceImpl mConfigLMode;
        private DeviceImpl mConfigLa;
        private DeviceImpl mConfigLb;
        private DeviceImpl mConfigLc;
        private DeviceImpl mOutLa;
        private DeviceImpl mOutLb;
        private DeviceImpl mOutLc;
        private DeviceImpl mConfigMMode;
        private DeviceImpl mStep;
        private DeviceImpl mCycle;
        private DeviceImpl mBandwidth;
        private DeviceImpl mGRange;
        private DeviceImpl mPullSa;
        private DeviceImpl mPullSb;
        private DeviceImpl mPullSc;
        private DeviceImpl mAdcSa;
        private DeviceImpl mAdcSb;
        private DeviceImpl mAdcSc;
        private DeviceImpl mOutSa;
        private DeviceImpl mOutSb;
        private DeviceImpl mOutSc;
        private DeviceImpl mOutMa;
        private DeviceImpl mOutMb;
        private DeviceImpl mPps;
        private DeviceImpl mPulses;
        private DeviceImpl mClearEncoder;
        private DeviceImpl mClearStep;
        private DeviceImpl mEcho;

        private DeviceImpl mInputA;
        private DeviceImpl mInputB;
        private DeviceImpl mInputC;
        private DeviceImpl mInputLA;
        private DeviceImpl mInputLB;
        private DeviceImpl mInputLC;
        private DeviceImpl mInputAcc;
        private DeviceImpl mStepCounter;
        private DeviceImpl mPowerState;
        private DeviceImpl mPlayState;
        private DeviceImpl mStepMotorState;
        private DeviceImpl mTemperature;
        private DeviceImpl mSignalStrength;
        private DeviceImpl mBattery;
        private DeviceImpl mFreeFallId;
        private DeviceImpl mTapId;

        private byte mLastSoundClip;
        private int mClearEncoderCounter;
        private int mClearStepCounter;

        public CheeseStickRobot(RobotManager manager, string uuid, bool enabled)
            : base(manager, uuid, CheeseStick.NAME, enabled)
        {
            initDevices();
        }
        
        private void initDevices()
        {
            mConfigSa = addDevice(DeviceType.EFFECTOR, CheeseStick.CONFIG_SA, "Sa",       DataType.INTEGER, 1, 0, 0, 3);
            mConfigSb = addDevice(DeviceType.EFFECTOR, CheeseStick.CONFIG_SB, "Sb",       DataType.INTEGER, 1, 0, 0, 3);
            mConfigSc = addDevice(DeviceType.EFFECTOR, CheeseStick.CONFIG_SC, "Sc",       DataType.INTEGER, 1, 0, 0, 3);
            mSoundOut = addDevice(DeviceType.EFFECTOR, CheeseStick.SOUND_OUT, "SoundOut", DataType.INTEGER, 1, 0, 0, 3);

            mConfigLMode = addDevice(DeviceType.EFFECTOR, CheeseStick.CONFIG_L_MODE, "LMode", DataType.INTEGER, 1, 0, 0, 3);
            mConfigLa    = addDevice(DeviceType.EFFECTOR, CheeseStick.CONFIG_LA,     "La",    DataType.INTEGER, 1, 0, 0, 3);
            mConfigLb    = addDevice(DeviceType.EFFECTOR, CheeseStick.CONFIG_LB,     "Lb",    DataType.INTEGER, 1, 0, 0, 3);
            mConfigLc    = addDevice(DeviceType.EFFECTOR, CheeseStick.CONFIG_LC,     "Lc",    DataType.INTEGER, 1, 0, 0, 3);

            mOutLa    = addDevice(DeviceType.EFFECTOR, CheeseStick.OUT_LA,    "OutLa",    DataType.INTEGER, 1, 0, 0,    255);
            mOutLb    = addDevice(DeviceType.EFFECTOR, CheeseStick.OUT_LB,    "OutLb",    DataType.INTEGER, 1, 0, 0,    255);
            mOutLc    = addDevice(DeviceType.EFFECTOR, CheeseStick.OUT_LC,    "OutLc",    DataType.INTEGER, 1, 0, 0,    255);

            mConfigMMode = addDevice(DeviceType.EFFECTOR, CheeseStick.CONFIG_M_MODE,  "MMode", DataType.INTEGER, 1, 0, 0, 3);
            mStep        = addDevice(DeviceType.EFFECTOR, CheeseStick.CONFIG_M_STEP,  "Step",  DataType.INTEGER, 1, 0, 0, 3);
            mCycle       = addDevice(DeviceType.EFFECTOR, CheeseStick.CONFIG_M_CYCLE, "Cycle", DataType.INTEGER, 1, 0, 0, 3);

            mBandwidth = addDevice(DeviceType.EFFECTOR, CheeseStick.BANDWIDTH, "Bandwidth", DataType.INTEGER, 1, 0, 0, 7);
            mGRange = addDevice(DeviceType.EFFECTOR, CheeseStick.G_RANGE, "GRange", DataType.INTEGER, 1, 0, 0, 3);

            mPullSa = addDevice(DeviceType.EFFECTOR, CheeseStick.PULL_SA, "PullSa", DataType.INTEGER, 1, 0, 0, 1);
            mPullSb = addDevice(DeviceType.EFFECTOR, CheeseStick.PULL_SB, "PullSb", DataType.INTEGER, 1, 0, 0, 1);
            mPullSc = addDevice(DeviceType.EFFECTOR, CheeseStick.PULL_SC, "PullSc", DataType.INTEGER, 1, 0, 0, 1);

            mAdcSa = addDevice(DeviceType.EFFECTOR, CheeseStick.ADC_SA, "ADCSa", DataType.INTEGER, 1, 0, 0, 1);
            mAdcSb = addDevice(DeviceType.EFFECTOR, CheeseStick.ADC_SB, "ADCSb", DataType.INTEGER, 1, 0, 0, 1);
            mAdcSc = addDevice(DeviceType.EFFECTOR, CheeseStick.ADC_SC, "ADCSc", DataType.INTEGER, 1, 0, 0, 1);

            mOutSa = addDevice(DeviceType.EFFECTOR, CheeseStick.OUT_SA, "OutSa", DataType.INTEGER, 1, 0, 0, 0xff);
            mOutSb = addDevice(DeviceType.EFFECTOR, CheeseStick.OUT_SB, "OutSb", DataType.INTEGER, 1, 0, 0, 0xff);
            mOutSc = addDevice(DeviceType.EFFECTOR, CheeseStick.OUT_SC, "OutSc", DataType.INTEGER, 1, 0, 0, 0xff);

            mOutMa = addDevice(DeviceType.EFFECTOR, CheeseStick.OUT_MA,  "OutMa",  DataType.INTEGER, 1, 0, -100, 100);
            mOutMb = addDevice(DeviceType.EFFECTOR, CheeseStick.OUT_MB,  "OutMb",  DataType.INTEGER, 1, 0, -100, 100);
            mPps   = addDevice(DeviceType.EFFECTOR, CheeseStick.PPS,     "PPS",    DataType.INTEGER, 1, 0, -1000, 1000);
            mPulses = addDevice(DeviceType.EFFECTOR, CheeseStick.PULSES, "Pulses", DataType.INTEGER, 1, 0, 0, 65535);

            mClearEncoder = addDevice(DeviceType.EFFECTOR, CheeseStick.CLEAR_ENCODER, "Pulses", DataType.INTEGER, 1, 0, 1, 1);
            mClearStep    = addDevice(DeviceType.EFFECTOR, CheeseStick.CLEAR_STEP,    "Pulses", DataType.INTEGER, 1, 0, 1, 1);

            mBuzz      = addDevice(DeviceType.EFFECTOR, CheeseStick.BUZZ, "Buzz", DataType.FLOAT, 1, 0, 0, 4186.0f);
            mPianoNote = addDevice(DeviceType.EFFECTOR, CheeseStick.PIANO_NOTE, "PianoNote", DataType.INTEGER, 1, 0, 0, 88);
            mSoundClip = addDevice(DeviceType.EFFECTOR, CheeseStick.SOUND_CLIP, "SoundClip", DataType.INTEGER, 1, 0, 0, 0x35);
            
            mInputA = addDevice(DeviceType.SENSOR, CheeseStick.INPUT_A,   "InputA", DataType.INTEGER, 1, 0, 0, 0xFF);
            mInputB = addDevice(DeviceType.SENSOR, CheeseStick.INPUT_B,   "InputB", DataType.INTEGER, 1, 0, 0, 0xFF);
            mInputC = addDevice(DeviceType.SENSOR, CheeseStick.INPUT_C,   "InputC", DataType.INTEGER, 1, 0, 0, 0xFF);

            mInputLA = addDevice(DeviceType.SENSOR, CheeseStick.INPUT_LA, "InputLA", DataType.INTEGER, 1, 0, 0, 0xFF);
            mInputLB = addDevice(DeviceType.SENSOR, CheeseStick.INPUT_LB, "InputLB", DataType.INTEGER, 1, 0, 0, 0xFF);
            mInputLC = addDevice(DeviceType.SENSOR, CheeseStick.INPUT_LC, "InputLC", DataType.INTEGER, 1, 0, 0, 0xFF);

            mEcho           = addDevice(DeviceType.SENSOR, CheeseStick.ECHO,         "Echo",         DataType.INTEGER, 1, 0, 0,      0xFF);
            mInputAcc       = addDevice(DeviceType.SENSOR, CheeseStick.ACCELERATION, "Acceleration", DataType.INTEGER, 3, 0, -2048,  2047);
            mStepCounter    = addDevice(DeviceType.SENSOR, CheeseStick.STEP_COUNTER, "StepCounter",  DataType.INTEGER, 1, 0, -32768, 32767);

            mFreeFallId     = addDevice(DeviceType.SENSOR, CheeseStick.FREE_FALL_ID, "FreeFallId", DataType.INTEGER, 1, 0, 0, 3);
            mTapId          = addDevice(DeviceType.SENSOR, CheeseStick.TAP_ID,       "TapId",      DataType.INTEGER, 1, 0, 0, 3);

            mPowerState     = addDevice(DeviceType.SENSOR, CheeseStick.POWER_STATE,     "PowerState",     DataType.INTEGER, 1, 0, -32768, 32767);
            mPlayState      = addDevice(DeviceType.SENSOR, CheeseStick.PLAY_STATE,      "PlayState",      DataType.INTEGER, 1, 0, 0,      1);
            mStepMotorState = addDevice(DeviceType.SENSOR, CheeseStick.STEP_STATE,      "StepState",      DataType.INTEGER, 1, 0, 0,      1);
            mTemperature    = addDevice(DeviceType.SENSOR, CheeseStick.TEMPERATURE,     "Temperature",    DataType.FLOAT,   1, 0, -40,    88);
            mSignalStrength = addDevice(DeviceType.SENSOR, CheeseStick.SIGNAL_STRENGTH, "SignalStrength", DataType.INTEGER, 1, 0, -128,   0);
            mBattery        = addDevice(DeviceType.SENSOR, CheeseStick.BATTERY_LEVEL,   "Battery",        DataType.FLOAT,   1, 0, 0,      3.6f);
        }

        public override RobotType type
        {
            get { return RobotType.CheeseStick; }
        }

        public override void resetDevices()
        {
            base.resetDevices();
            mLastSoundClip = 0;
        }

        public override byte[] encodeMotoringData()
        {
            var packet = new byte[20];
            packet[0] = 0x10;

            packet[1] = (byte)((mSoundOut.read() << 6) | (mConfigSc.read() << 4) |
                               (mConfigSb.read() << 2) | mConfigSa.read());
            
            switch (mConfigLMode.read())
            {
            case CheeseStick.L_MODE_NORMAL:
                packet[2] = (byte)((mConfigLc.read() << 4) | (mConfigLb.read() << 2) | mConfigLa.read());
                break;

            case CheeseStick.L_MODE_ULTRA_SONIC:
                packet[2] = CheeseStick.DEVICE_ULTRASONIC;
                break;

            case CheeseStick.L_MODE_ENCODER:
                packet[2] = CheeseStick.DEVICE_ENCODER;
                break;

            case CheeseStick.L_MODE_UART_9600:
                packet[2] = CheeseStick.DEVICE_UART_9600;
                break;
            }

            packet[3] = (byte)((mConfigMMode.read() << 4) | (mStep.read() << 2) | mCycle.read());
            packet[4] = (byte)((mBandwidth.read() << 4) | mGRange.read());

            packet[5] = getOutSValue(mConfigSa, mOutSa, mPullSa, mAdcSa);
            packet[6] = getOutSValue(mConfigSb, mOutSb, mPullSb, mAdcSb);
            packet[7] = getOutSValue(mConfigSc, mOutSc, mPullSc, mAdcSc);

            packet[8] =  (byte)mOutLa.read();
            packet[9] =  (byte)mOutLb.read();
            packet[10] = (byte)mOutLc.read();

            if (mConfigMMode.read() <= CheeseStick.M_MODE_DUAL_SERVO)
            {
                packet[11] = (byte)mOutMa.read();
                packet[12] = (byte)mOutMb.read();
            }
            else
            {
                Utils.int16ToBytesBE(mPps.read(), packet, 11);
            }

            Utils.int16ToBytesBE(mPulses.read(), packet, 13);

            if (mClearEncoder.isWritten())
            {
                ++mClearEncoderCounter;
                packet[15] |= (byte)((mClearEncoderCounter & 0xf) << 4);
            }

            if (mClearStep.isWritten())
            {
                ++mClearStepCounter;
                packet[15] |= (byte)(mClearStepCounter & 0xf);
            }

            Utils.int16ToBytesBE((int)(mBuzz.readFloat() * 10), packet, 16);
            packet[18] = (byte)mPianoNote.read();

            if (mSoundClip.isWritten())
            {
                var clip = mSoundClip.read();
                // toggle the highest bit if clip does not change
                if (clip != 0 && clip == (mLastSoundClip & 0x7F))
                {
                    clip |= (mLastSoundClip & 0x80) ^ 0x80;
                }
                mLastSoundClip = packet[19] = (byte)clip;
            }
            else
            {
                packet[19] = mLastSoundClip;
            }

            return packet;
        }

        private byte getOutSValue(DeviceImpl sx, DeviceImpl outS, DeviceImpl pull, DeviceImpl adc)
        {
            if (sx.read() <= CheeseStick.S_MODE_ANALOG)
            {
                return (byte)(pull.read() << 1 | adc.read());
            }
            else
            {
                return (byte)outS.read();
            }
        }

        public override void decodeSensoryData(byte[] data)
        {
            mInputA.put(data[1]);
            mInputB.put(data[2]);
            mInputC.put(data[3]);

            mInputLA.put(data[4]);
            mInputLB.put(data[5]);
            mInputLC.put(data[6]);

            mEcho.put(Utils.toInt16BE(data, 5));

            mInputAcc.put(CheeseStick.ACCEL_X, Utils.toInt16BE(data, 7));
            mInputAcc.put(CheeseStick.ACCEL_Y, Utils.toInt16BE(data, 9));
            mInputAcc.put(CheeseStick.ACCEL_Z, Utils.toInt16BE(data, 11));

            mStepCounter.put(Utils.toInt16BE(data, 13));

            mFreeFallId.put(data[15] >> 6);
            mTapId.put((data[15] >> 4) & 0x3);

            mStepMotorState.put(data[16] >> 7);
            mPowerState.put(data[16] & 0x3);
            mPlayState.put((data[16] >> 4) & 1);

            mTemperature.putFloat(24.0f + (sbyte)data[17] / 2.0f);
            mSignalStrength.put((sbyte)data[18]);
            mBattery.putFloat(2.0f + data[19] / 100.0f);
        }

        public override int sensoryPacketType
        {
            get { return CheeseStick.SENSORY_PACKET_TYPE; }
        }
    }
}
