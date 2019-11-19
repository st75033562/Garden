using System;

namespace Robomation
{
    internal static class Utils
    {
        public static int clamp(int v, int min, int max)
        {
            if (min > max)
            {
                throw new ArgumentException("min > max");
            }
            return v < min ? min : (v > max ? max : v);
        }

        public static float clamp(float v, float min, float max)
        {
            if (min > max)
            {
                throw new ArgumentException("min > max");
            }
            return v < min ? min : (v > max ? max : v);
        }

        public static void copyClamped(int[] src, int srcIndex, int[] dest, int destIndex, int length, int min, int max)
        {
            for (int i = 0; i < length; ++i)
            {
                dest[destIndex++] = clamp(src[srcIndex++], min, max);
            }
        }

        public static void copyClamped(float[] src, int srcIndex, float[] dest, int destIndex, int length, float min, float max)
        {
            for (int i = 0; i < length; ++i)
            {
                dest[destIndex++] = clamp(src[srcIndex++], min, max);
            }
        }

        public static short toInt16BE(byte[] data, int offset)
        {
            return (short)((data[offset] << 8) | data[offset + 1]);
        }

        public static void int16ToBytesBE(int value, byte[] data, int offset)
        {
            data[offset] = (byte)(value >> 8);
            data[offset + 1] = (byte)value;
        }

        public static ushort toUInt16BE(byte[] data, int offset)
        {
            return (ushort)((data[offset] << 8) | data[offset + 1]);
        }
    }
}
