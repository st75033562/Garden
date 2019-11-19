using System.Collections.Generic;

public abstract class BaseComparer<T> : IComparer<T> where T : class
{
    public int Compare(T x, T y)
    {
        var xNull = ReferenceEquals(x, null);
        var yNull = ReferenceEquals(y, null);

        if (xNull != yNull)
        {
            return xNull ? -1 : 1;
        }

        if (xNull)
        {
            return 0;
        }

        return DoCompare(x, y);
    }

    protected abstract int DoCompare(T x, T y);
}
