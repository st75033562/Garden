using System.Collections.Generic;
using UnityEngine.Assertions;

public class ByteBufferPool
{
    private Dictionary<int, Stack<byte[]>> m_freeBufs = new Dictionary<int, Stack<byte[]>>();

    public static readonly ByteBufferPool Default = new ByteBufferPool();

    public byte[] Allocate(int length)
    {
        Assert.IsTrue(length > 0);

        Stack<byte[]> bufs;
        if (m_freeBufs.TryGetValue(length, out bufs))
        {
            if (bufs.Count > 0)
            {
                return bufs.Pop();
            }
        }

        return new byte[length];
    }

    public void Deallocate(byte[] buf)
    {
        Assert.IsTrue(buf != null && buf.Length > 0);

        Stack<byte[]> bufs;
        if (!m_freeBufs.TryGetValue(buf.Length, out bufs))
        {
            bufs = new Stack<byte[]>();
            m_freeBufs.Add(buf.Length, bufs);
        }
        bufs.Push(buf);
    }
}
