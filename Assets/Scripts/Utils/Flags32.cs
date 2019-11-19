using System.Collections.Generic;
using System.Collections;

public class Flags32 : IEnumerable<int>
{
    private readonly int m_flags;

    public Flags32(int flags)
    {
        m_flags = flags;
    }

    public IEnumerator<int> GetEnumerator()
    {
        int flags = m_flags;
        while (flags != 0)
        {
            var newFlags = (flags - 1) & flags;
            yield return newFlags ^ flags;
            flags = newFlags;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}