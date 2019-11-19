using UnityEngine;

public static class RandomExtensions
{
    /// <summary>
    /// return a float in the range [min, max]. min &gt max is possible.
    /// </summary>
    public static float Range(this IRandom random, float min, float max)
    {
        return Mathf.Lerp(min, max, random.nextFloat);
    }

    /// <summary>
    /// return a Vector2 in the rect formed by (0, 0) and range.
    /// </summary>
    public static Vector2 Range(this IRandom random, Vector2 range)
    {
        return new Vector2(random.Range(0, range.x), random.Range(0, range.y));
    }
}
