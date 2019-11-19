using System;

namespace Robomation
{
    public struct HamsterSensorPacket
    {
        public const int Size = 20;

        public const byte FlagLight = 0;
        public const byte FlagTemp = 1;

        private readonly byte[] m_data;

        public HamsterSensorPacket(byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException();
            }
            if (data.Length < Size)
            {
                throw new ArgumentException();
            }
            m_data = data;
        }

        public byte[] data { get { return m_data; } }

        public static HamsterSensorPacket Create()
        {
            return new HamsterSensorPacket(new byte[Size]);
        }

        public int signalStrength
        {
            get { return (sbyte)m_data[3]; }
            set { m_data[3] = (byte)value; }
        }

        public int leftProximity
        {
            get { return m_data[4]; }
            set { m_data[4] = (byte)value; }
        }

        public int rightProximity
        {
            get { return m_data[5]; }
            set { m_data[5] = (byte)value; }
        }

        public int leftFloor
        {
            get { return m_data[6]; }
            set { m_data[6] = (byte)value; }
        }

        public int rightFloor
        {
            get { return m_data[7]; }
            set { m_data[7] = (byte)value; }
        }

        public int accelerationX
        {
            get { return Utils.toInt16BE(m_data, 8); }
            set { Utils.int16ToBytesBE(value, m_data, 8); }
        }

        public int accelerationY
        {
            get { return Utils.toInt16BE(m_data, 10); }
            set { Utils.int16ToBytesBE(value, m_data, 10); }
        }

        public int accelerationZ
        {
            get { return Utils.toInt16BE(m_data, 12); }
            set { Utils.int16ToBytesBE(value, m_data, 12); }
        }

        public byte flagLightTemp
        {
            get { return m_data[14]; }
            private set { m_data[14] = value; }
        }

        public int light
        {
            get
            {
                if (flagLightTemp != FlagLight)
                {
                    throw new InvalidOperationException();
                }
                return Utils.toUInt16BE(m_data, 15);
            }
            set
            {
                Utils.int16ToBytesBE(value, m_data, 15);
                flagLightTemp = FlagLight;
            }
        }

        public int temperature
        {
            get
            {
                if (flagLightTemp != FlagTemp)
                {
                    throw new InvalidOperationException();
                }
                return (sbyte)m_data[15];
            }
            set
            {
                m_data[15] = (byte)value;
                flagLightTemp = FlagTemp;
            }
        }

        public int battery
        {
            get
            {
                if (flagLightTemp != FlagTemp)
                {
                    throw new InvalidOperationException();
                }
                return m_data[16];
            }
            set
            {
                m_data[16] = (byte)value;
                flagLightTemp = FlagTemp;
            }
        }

        public int inputA
        {
            get { return m_data[17]; }
            set { m_data[17] = (byte)value; }
        }

        public int inputB
        {
            get { return m_data[18]; }
            set { m_data[18] = (byte)value; }
        }

        public int lineTracerState
        {
            get { return m_data[19]; }
            set { m_data[19] = (byte)value; }
        }
    }

    public struct HamsterEffectorPacket
    {
        public const byte CommandMotoring = (byte)(0x1 << 4);
        public const byte CommandBalance = (byte)(0xe << 4);

        public const int Size = 20;

        private readonly byte[] m_data;

        public HamsterEffectorPacket(byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException();
            }
            if (data.Length < Size)
            {
                throw new ArgumentException();
            }
            m_data = data;
        }

        public static HamsterEffectorPacket Create()
        {
            return new HamsterEffectorPacket(new byte[Size]);
        }

        public byte[] data { get { return m_data; } }

        public int topology
        {
            get { return m_data[0]; }
            set { m_data[0] = (byte)value; }
        }

        public int version
        {
            get { return m_data[1]; }
            set { m_data[1] = (byte)value; }
        }

        public int command
        {
            get { return m_data[2]; }
            set { m_data[2] = (byte)value; }
        }

        public int leftWheel
        {
            get { return (sbyte)m_data[3]; }
            set { m_data[3] = (byte)value; }
        }

        public int rightWheel
        {
            get { return (sbyte)m_data[4]; }
            set { m_data[4] = (byte)value; }
        }

        public int leftLed
        {
            get { return m_data[5]; }
            set { m_data[5] = (byte)value; }
        }

        public int rightLed
        {
            get { return m_data[6]; }
            set { m_data[6] = (byte)value; }
        }

        public int buzzerFrequency
        {
            get { return m_data[7] << 16 | m_data[8] << 8 | m_data[9]; }
            set
            {
                m_data[7] = (byte)(value >> 16);
                m_data[8] = (byte)(value >> 8);
                m_data[9] = (byte)value;
            }
        }

        public int note
        {
            get { return m_data[10]; }
            set { m_data[10] = (byte)value; }
        }

        public int lineTracer
        {
            get { return m_data[11]; }
            set { m_data[11] = (byte)value; }
        }

        public int configProximity
        {
            get { return m_data[12]; }
            set { m_data[12] = (byte)value; }
        }

        public int configGravity
        {
            get { return (m_data[13] >> 4) & 0xF; }
            set
            {
                m_data[13] &= 0xF;
                m_data[13] |= (byte)((value & 0xF) << 4);
            }
        }

        public int configBandwidth
        {
            get { return m_data[13] & 0xF; }
            set
            {
                m_data[13] &= 0xF0;
                m_data[13] |= (byte)(value & 0xF);
            }
        }

        public int ioModeA
        {
            get { return (m_data[14] >> 4) & 0xF; }
            set
            {
                m_data[14] &= 0xF;
                m_data[14] |= (byte)((value & 0xF) << 4);
            }
        }

        public int ioModeB
        {
            get { return m_data[14] & 0xF; }
            set
            {
                m_data[14] &= 0xF0;
                m_data[14] |= (byte)(value & 0xF);
            }
        }

        public int outputA
        {
            get { return m_data[15]; }
            set { m_data[15] = (byte)value; }
        }

        public int outputB
        {
            get { return m_data[16]; }
            set { m_data[16] = (byte)value; }
        }

        public int wheelBalance
        {
            get { return m_data[17]; }
            set { m_data[17] = (byte)value; }
        }
    }
}
