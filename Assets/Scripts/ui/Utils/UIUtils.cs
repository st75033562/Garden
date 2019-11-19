using System;
using UnityEngine;

public static class UIUtils
{
    // RRGGBB[AA]
    public static Color ParseColor(string s)
    {
        if (s.Length == 6 || s.Length == 8)
        {
            var r = HexToInt(s, 0, 2) / 255.0f;
            var g = HexToInt(s, 2, 2) / 255.0f;
            var b = HexToInt(s, 4, 2) / 255.0f;
            var a = (s.Length == 8 ? HexToInt(s, 6, 2) : 255) / 255.0f;

            return new Color(r, g, b, a);
        }
        else
        {
            throw new ArgumentException("RRGGBB[AA]");
        }
    }

    private static int HexToInt(char c)
    {
        c = char.ToLower(c);
        if (c >= '0' && c <= '9')
        {
            return c - '0';
        }
        else if (c >= 'a' && c <= 'f')
        {
            return c - 'a' + 10;
        }
        else
        {
            throw new ArgumentException();
        }
    }

    private static int HexToInt(string s, int start, int length)
    {
        int v = 0;
        for (; length > 0; ++start, --length)
        {
            v = v * 16 + HexToInt(s[start]);
        }
        return v;
    }

    /// <summary>
    /// quantify a number to the given max width
    /// </summary>
    public static string Quantify(int count, int maxWidth = 4)
    {
        if (count < 0)
        {
            throw new ArgumentOutOfRangeException("count must be positive");
        }

        if (count < 1000)
        {
            return count.ToString();
        }

        char unit;
        int order;
        if (count >= 10000)
        {
            order = 10000;
            unit = 'w';
        }
        else
        {
            order = 1000;
            unit = 'k';
        }

        // number of integer digits
        int ni = MathUtils.NumberOfDigits(count / order);
        // number of fractional digits
        int f = count % order;
        int nf = f != 0 ? MathUtils.NumberOfDigits(f) : 0;

        // calculate the max available number of fractional digits
        int fw = maxWidth - (ni + 2);
        if (fw <= 0 || nf == 0)
        {
            return ni.ToString() + unit;
        }
        else
        {
            return ni + "." + MathUtils.LeftNumberOfDigits(f, fw) + unit;
        }
    }
}
