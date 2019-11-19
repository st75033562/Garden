using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class VoiceClipHeader {
    public const int Size = 4 + 1 + 1;

    public int samplingRate;
    public int channels;
    public int frameDurationMs;

    public void Deserialize(byte[] buffer)
    {
        if (buffer.Length < Size)
        {
            throw new ArgumentException("not enough length");
        }
        // assume LE
        samplingRate = BitConverter.ToInt32(buffer, 0);
        channels = buffer[4];
        frameDurationMs = buffer[5];
    }

    public byte[] Serialize()
    {
        var buffer = new byte[Size];
        VoiceUtil.GetBytes(buffer, samplingRate);
        if (channels > byte.MaxValue)
        {
            throw new ArgumentOutOfRangeException("channels must be < 256");
        }
        buffer[4] = (byte)channels;
        if (frameDurationMs > byte.MaxValue)
        {
            throw new ArgumentOutOfRangeException("frameDurationMs must be < 256");
        }
        buffer[5] = (byte)frameDurationMs;
        return buffer;
    }
}
