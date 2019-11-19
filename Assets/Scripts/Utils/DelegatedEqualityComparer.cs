using System;
using System.Collections.Generic;

public class DelegatedEqualityComparer<T> : IEqualityComparer<T>
{
    private readonly Func<T, T, bool> m_comp;

    public DelegatedEqualityComparer(Func<T, T, bool> comp)
    {
        if (comp == null)
        {
            throw new ArgumentNullException();
        }
        m_comp = comp;
    }

    public bool Equals(T x, T y)
    {
        return m_comp(x, y);
    }

    public int GetHashCode(T obj)
    {
        return obj.GetHashCode();
    }
}

public class DelegatedEqualityComparer
{
    public static DelegatedEqualityComparer<T> Of<T>(Func<T, T, bool> comp)
    {
        return new DelegatedEqualityComparer<T>(comp);
    }
}