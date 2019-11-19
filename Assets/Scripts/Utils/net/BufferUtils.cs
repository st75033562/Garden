using System.Net;
public static class BufferUtils
{
    public static void WriteLE(byte[] buffer, int offset, int value)
    {
        buffer[offset++] = (byte)(value & 0xFF);
        buffer[offset++] = (byte)((value >> 8) & 0xFF);
        buffer[offset++] = (byte)((value >> 16) & 0xFF);
        buffer[offset++] = (byte)((value >> 24) & 0xFF);
    }

    public static void WriteNetworkOrder(byte[] buffer, int offset, int value)
    {
        WriteLE(buffer, offset, IPAddress.HostToNetworkOrder(value));
    }
}
