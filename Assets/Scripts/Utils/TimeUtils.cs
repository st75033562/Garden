using System;

public static class TimeUtils
{
    public const int MinutesPerDay = 24 * 60;
    public static readonly DateTime EpochTimeUTC = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

    public static long secondsSinceEpoch
    {
        get
        {
            return SecondsSince(EpochTimeUTC);
        }
    }

    public static long SecondsSince(DateTime time)
    {
        return (long)(DateTime.UtcNow - time).TotalSeconds;
    }

    public static DateTime FromEpochMilliseconds(long milliseconds)
    {
        return EpochTimeUTC.AddMilliseconds(milliseconds);
    }

    public static DateTime FromEpochSeconds(long seconds)
    {
        return EpochTimeUTC.AddSeconds((double)seconds);
    }

    public static long ToEpochSeconds(DateTime time)
    {
        return (long)(time - EpochTimeUTC).TotalSeconds;
    }

    // TODO: should use CurrentCulture for formatting
    /// <summary>
    /// return the localized local time string
    /// </summary>
    public static string GetLocalizedTime(long secondsSinceEpoch)
    {
        return FromEpochSeconds(secondsSinceEpoch).ToLocalTime().ToString("ui_date_time".Localize());;
    }

    public static string GetLocalizedTime(DateTime time)
    {
        return time.ToLocalTime().ToString("ui_date_time".Localize());
    }


    public static string GetHHmmssString(int seconds)
    {
		int hours = seconds / 3600;
		seconds -= (hours * 3600);
		int mins = seconds / 60;
		seconds -= (mins * 60);
		return string.Format("{0:00}:{1:00}:{2:00}", hours, mins, seconds);
    }
}
