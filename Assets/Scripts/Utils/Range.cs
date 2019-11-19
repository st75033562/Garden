using System;

/// <summary>
/// exclusive range
/// </summary>
public struct Range
{
    public Range(int start, int count)
        : this()
    {
        if (count < 0)
        {
            throw new ArgumentOutOfRangeException("count");
        }
        this.start = start;
        this.end = start + count;
    }

    public int start { get; private set; }

    public int end { get; private set; }

    public int count { get { return end - start; } }

    public override string ToString()
    {
        return string.Format("[{0}, {1})", start, end);
    }
}