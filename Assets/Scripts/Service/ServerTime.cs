using System;
using UnityEngine;

public interface IMonotonicTime
{
    double seconds { get; }
}

public class UnityMonotonicTime : IMonotonicTime
{
    public double seconds
    {
        get { return Time.realtimeSinceStartup; }
    }
}

public static class ServerTime
{
    private static DateTime s_initialServerTime;
    private static double s_initialLocalTime;
    private static IMonotonicTime s_monotonicTime = new UnityMonotonicTime();

    public static void Init(DateTime serverTime)
    {
        s_initialServerTime = serverTime;
        s_initialLocalTime = s_monotonicTime.seconds;
    }

    public static void Init(DateTime serverTime, IMonotonicTime time)
    {
        if (time == null)
        {
            throw new ArgumentNullException();
        }

        s_initialServerTime = serverTime;
        s_initialLocalTime = time.seconds;
        s_monotonicTime = time;
    }

    public static DateTime UtcNow
    {
        get
        {
            return s_initialServerTime.AddSeconds(s_monotonicTime.seconds - s_initialLocalTime);
        }
    }
}
