using System;
using System.Collections;
using System.Collections.Generic;

public class DelegatedComparer<T> : IComparer<T>, IComparer
{
    private readonly Comparison<T> m_comp;

    public DelegatedComparer(Comparison<T> comp)
    {
        if (comp == null)
        {
            throw new ArgumentNullException();
        }
        m_comp = comp;
    }

    public int Compare(T x, T y)
    {
        return m_comp(x, y);
    }

    public int Compare(object x, object y)
    {
        return Compare((T)x, (T)y);
    }
}

public class DelegatedComparer
{
    public static DelegatedComparer<T> Of<T>(Comparison<T> comp)
    {
        return new DelegatedComparer<T>(comp);
    }
}
