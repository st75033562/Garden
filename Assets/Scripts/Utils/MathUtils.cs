using System;
using System.Linq;
using UnityEngine;

public static class MathUtils
{
    public static int NumberOfDigits(int c)
    {
        int count = 0;
        do
        {
            ++count;
            c /= 10;
        } while (c != 0);
        return count;
    }

    public static int LeftNumberOfDigits(int c, int n)
    {
        if (n < 0)
        {
            throw new ArgumentOutOfRangeException("n");
        }
        if (n == 0)
        {
            return 0;
        }

        var str = c.ToString();
        int offset = c < 0 ? 1 : 0;
        // use long to avoid overflow
        long num = long.Parse(str.Substring(offset, Mathf.Min(n, str.Length - offset)));
        return (int)(c >= 0 ? num : -num);
    }

    public static Vector3 GetX(ref Matrix4x4 m)
    {
        return new Vector3(m.m00, m.m10, m.m20);
    }

    public static Vector3 GetY(ref Matrix4x4 m)
    {
        return new Vector3(m.m01, m.m11, m.m21);
    }

    public static Vector3 GetZ(ref Matrix4x4 m)
    {
        return new Vector3(m.m02, m.m12, m.m22);
    }

    public static void SetX(ref Matrix4x4 m, Vector3 v)
    {
        m.m00 = v.x;
        m.m10 = v.y;
        m.m20 = v.z;
    }

    public static void SetY(ref Matrix4x4 m, Vector3 v)
    {
        m.m01 = v.x;
        m.m11 = v.y;
        m.m21 = v.z;
    }

    public static void SetZ(ref Matrix4x4 m, Vector3 v)
    {
        m.m02 = v.x;
        m.m12 = v.y;
        m.m22 = v.z;
    }

    public static Bounds Transform(Bounds bounds, ref Matrix4x4 m)
    {
        var center = m.MultiplyPoint(bounds.center);
        var extents = bounds.extents;

        var newExt = Vector3.zero;
        for (int i = 0; i < 3; ++i)
        {
            for (int j = 0; j < 3; ++j)
            {
                newExt[i] += Mathf.Abs(m[i, j] * extents[j]);
            }
        }

        return new Bounds(center, 2 * newExt);
    }
}
