using System;

// https://software.intel.com/en-us/articles/fast-random-number-generator-on-the-intel-pentiumr-4-processor
public class FastRandom : IRandom
{
    public FastRandom(uint seed)
    {
        this.seed = seed;
    }

    public uint seed
    {
        get;
        set;
    }

    public int nextInt
    {
        get
        {
            seed = 214013 * seed + 2531011;
            return (int)(seed & 0x7fffffff);
        }
    }

    public float nextFloat
    {
        get { return (float)((double)nextInt / int.MaxValue); }
    }

    public object state
    {
        get { return seed; }
        set
        {
            if (!(value is uint))
            {
                throw new ArgumentException("invalid state");
            }
            seed = (uint)value;
        }
    }
}
