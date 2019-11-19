using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class SegmentedBuffer : IEnumerable<ArraySegment<byte>>
{
    private class Buffer
    {
        public byte[] buffer;
        public int size;
        public int remainingSize
        {
            get { return buffer.Length - size; }
        }
    }

    private readonly List<Buffer> m_buffers = new List<Buffer>();
    private int m_writeBufIndex;
    private readonly int m_growSize;

    public SegmentedBuffer(int initialCapacity, int growSize)
    {
        if (initialCapacity <= 0 || growSize <= 0)
        {
            throw new ArgumentOutOfRangeException();
        }

        m_buffers.Add(new Buffer {
            buffer = new byte[initialCapacity]
        });
        m_growSize = growSize;
    }

    public SegmentedBuffer(int growSize)
        : this(growSize, growSize)
    {
    }

    private void AddNewBuffer()
    {
        ++m_writeBufIndex;
        if (m_writeBufIndex == m_buffers.Count)
        {
            m_buffers.Add(new Buffer {
                buffer = new byte[m_growSize]
            });
        }
    }

    public void Add(ArraySegment<byte> data)
    {
        int cur = data.Offset;
        int end = data.Offset + data.Count;
        while (cur < end)
        {
            var buffer = m_buffers[m_writeBufIndex];
            if (buffer.remainingSize == 0)
            {
                AddNewBuffer();
                buffer = m_buffers[m_writeBufIndex];
            }
            int bytesToCopy = Mathf.Min(buffer.remainingSize, end - cur);
            Array.Copy(data.Array, cur, buffer.buffer, buffer.size, bytesToCopy);
            buffer.size += bytesToCopy;
            cur += bytesToCopy;
        }
    }

    public void Add(byte[] data)
    {
        Add(new ArraySegment<byte>(data));
    }

    public void Add(byte data)
    {
        var buffer = m_buffers[m_writeBufIndex];
        if (buffer.remainingSize == 0)
        {
            AddNewBuffer();
            buffer = m_buffers[m_writeBufIndex];
        }
        buffer.buffer[buffer.size++] = data;
    }

    public void Clear()
    {
        foreach (var buffer in m_buffers)
        {
            buffer.size = 0;
        }
        m_writeBufIndex = 0;
    }

    // TODO: Add Remove functions

    public void Shrink()
    {
        for (int i = m_buffers.Count - 1; i >= 0; --i)
        {
            if (m_buffers[i].size == 0)
            {
                m_buffers.RemoveAt(i);
            }
            else
            {
                m_writeBufIndex = i;
                break;
            }
        }
    }

    public int size
    {
        get
        {
            // first buffer size can be different
            int numBytes = m_buffers[0].size;
            if (m_writeBufIndex >= 1)
            {
                numBytes += (m_writeBufIndex - 1) * m_growSize + m_buffers[m_writeBufIndex].size;
            }
            return numBytes;
        }
    }

    public IEnumerator<ArraySegment<byte>> GetEnumerator()
    {
        foreach (var buffer in m_buffers)
        {
            if (buffer.size > 0)
            {
                yield return new ArraySegment<byte>(buffer.buffer, 0, buffer.size);
            }
            else
            {
                yield break;
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public byte[] ToArray()
    {
        byte[] data = new byte[size];
        int offset = 0;
        Buffer buffer;
        for (int i = 0; i <= m_writeBufIndex; ++i)
        {
            buffer = m_buffers[i];
            Array.Copy(buffer.buffer, 0, data, offset, buffer.size);
            offset += buffer.size;
        }
        return data;
    }
}
