using System;
using System.Collections;
using System.Collections.Generic;

public class CircularBuffer<T> : IEnumerable<T>
{
    private T[] m_buf;
    private int m_head;
    private int m_tail;

    public CircularBuffer(int size)
    {
        if (size <= 0)
        {
            throw new ArgumentOutOfRangeException("must be positive");
        }

        // one extra space to differentiate empty with full
        m_buf = new T[size + 1];
    }

    public void Add(T v)
    {
        m_buf[m_tail] = v;
        m_tail = (m_tail + 1) % m_buf.Length;
        // buffer is full
        if (m_tail == m_head)
        {
            // remove the head
            m_head = (m_head + 1) % m_buf.Length;
        }
    }

    public T Pop()
    {
        if (IsEmpty)
        {
            throw new InvalidOperationException("empty");
        }
        T res = m_buf[m_head];
        m_head = (m_head + 1) % m_buf.Length;
        return res;
    }

    public void Clear()
    {
        for (int i = Count; i > 0; --i)
        {
            m_buf[m_head] = default(T);
            m_head = (m_head + 1) % m_buf.Length;
        }
    }

    public bool IsEmpty
    {
        get { return m_head == m_tail; }
    }

    public int Count
    {
        get { return (m_tail - m_head + m_buf.Length) % m_buf.Length; }
    }

    public int Capacity
    {
        get { return m_buf.Length - 1; }
        set
        {
            if (value > Capacity)
            {
                var newBuf = new T[value + 1];
                int i = 0;
                foreach (var v in this)
                {
                    newBuf[i++] = v;
                }
                m_buf = newBuf;
                m_head = 0;
                m_tail = i;
            }
        }
    }

    public T this[int index]
    {
        get
        {
            if (index < 0 || index >= Count)
            {
                throw new ArgumentOutOfRangeException();
            }

            return m_buf[(m_head + index) % m_buf.Length];
        }

        set
        {
            if (index < 0 || index >= Count)
            {
                throw new ArgumentOutOfRangeException();
            }

            m_buf[(m_head + index) % m_buf.Length] = value;
        }
    }

    public IEnumerator<T> GetEnumerator()
    {
        for (int i = Count, j = m_head; i > 0; --i)
        {
            yield return m_buf[j];
            j = (j + 1) % m_buf.Length;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
