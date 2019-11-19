using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public static class VoiceUtil
{
    // little endian
    public static void GetBytes(byte[] buffer, int v)
    {
        if (buffer.Length < 4)
        {
            throw new ArgumentException("buffer length must be at least 4", "buffer");
        }
        buffer[0] = (byte)(v & 0xff);
        buffer[1] = (byte)((v >> 8) & 0xff);
        buffer[2] = (byte)((v >> 16) & 0xff);
        buffer[3] = (byte)((v >> 24) & 0xff);
    }
}
