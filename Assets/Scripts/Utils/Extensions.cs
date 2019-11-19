using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using UnityEngine;
using System.Globalization;

public static class Extensions
{
    public static object Get(this Hashtable table, object key, object defaultValue)
    {
        if (table.ContainsKey(key))
        {
            return table[key];
        }
        return defaultValue;
    }

    public static T JsonGet<T>(this Hashtable table, string key, T defaultValue)
    {
        if (table.ContainsKey(key))
        {
            var v = table[key];
            if (v.GetType() == typeof(double))
            {
                var num = (double)table[key];
                switch (Type.GetTypeCode(typeof(T)))
                {
                    case TypeCode.Byte:
                        return (T)(object)(byte)num;
                    case TypeCode.SByte:
                        return (T)(object)(sbyte)num;
                    case TypeCode.UInt16:
                        return (T)(object)(ushort)num;
                    case TypeCode.UInt32:
                        return (T)(object)(uint)num;
                    case TypeCode.UInt64:
                        return (T)(object)(ulong)num;
                    case TypeCode.Int16:
                        return (T)(object)(short)num;
                    case TypeCode.Int32:
                        return (T)(object)(int)num;
                    case TypeCode.Int64:
                        return (T)(object)(long)num;
                    case TypeCode.Decimal:
                        return (T)(object)(decimal)num;
                    case TypeCode.Double:
                        return (T)(object)(double)num;
                    case TypeCode.Single:
                        return (T)(object)(float)num;
                    default:
                        throw new InvalidCastException();
                }
            }
            return (T)table[key];
        }
        return defaultValue;
    }

    public static string Localize(this string id, params object[] args)
    {
        var translation = LocalizationManager.instance.getString(id);
        if (args != null && args.Length != 0)
        {
            return string.Format(translation, args);
        }
        else
        {
            return translation;
        }
    }

    public static IEnumerable<Pair<T1, T2>> Zip<T1, T2>(this IEnumerable<T1> c1, IEnumerable<T2> c2)
    {
        using (var e1 = c1.GetEnumerator())
        using (var e2 = c2.GetEnumerator())
        {
            while (e1.MoveNext() && e2.MoveNext())
            {
                yield return Pair.Of(e1.Current, e2.Current);
            }
        }
    }

    public static string Format(this Vector3 v, int decimalDigits = 2)
    {
        var fmt = string.Format("F{0}", decimalDigits);
        return string.Format("x: {0}, y: {1}, z: {2}", v.x.ToString(fmt), v.y.ToString(fmt), v.z.ToString(fmt));
    }

    public static Vector3 RectCoreWorldPos(this RectTransform rectT) {
        Vector3[] corners = new Vector3[4];
        rectT.GetWorldCorners(corners);
        Vector3 result = new Vector3();
        result.x = (corners[0].x + corners[3].x) / 2;
        result.y = (corners[0].y + corners[1].y) / 2;
        result.z = corners[0].z;
        return result;
    }

    public static Vector3 RectRightWorldPos(this RectTransform rectT) {
        Vector3[] corners = new Vector3[4];
        rectT.GetWorldCorners(corners);
        Vector3 result = new Vector3();
        result.x = corners[2].x ;
        result.y = (corners[2].y + corners[3].y) / 2;
        result.z = corners[0].z;
        return result;
    }

    public static void DestroyChildren(this Transform trans)
    {
        for (int i = trans.childCount - 1; i >= 0; --i)
        {
            var child = trans.GetChild(i);
            Utils.Destroy(child.gameObject);
            child.SetParent(null);
        }
    }

    // depth starts from 0
    public static int Depth(this Transform trans)
    {
        var depth = 0;
        while (trans.parent)
        {
            ++depth;
            trans = trans.parent;
        }
        return depth;
    }

    public static Vector3 WorldToLocal(this Transform trans, Vector3 worldPos)
    {
        return trans.worldToLocalMatrix.MultiplyPoint3x4(worldPos);
    }
}

public static class ListExtensions
{
    public static void Swap<T>(this IList<T> list, int i, int j)
    {
        var tmp = list[i];
        list[i] = list[j];
        list[j] = tmp;
    }

    public static void Resize<T>(this IList<T> list, int newSize, T value = default(T))
    {
        while (list.Count < newSize)
        {
            list.Add(value);
        }
    }

    public static bool Remove<T>(this List<T> list, Predicate<T> match)
    {
        var index = list.FindIndex(match);
        if (index != -1)
        {
            list.RemoveAt(index);
        }
        return false;
    }

    public static void SortBy<T, U>(this List<T> list, Func<T, U> keySelector, IComparer<U> comparer)
    {
        if (keySelector == null)
        {
            throw new ArgumentNullException("keySelector");
        }
        if (comparer == null)
        {
            throw new ArgumentNullException("comparer");
        }

        list.Sort((x, y) => comparer.Compare(keySelector(x), keySelector(y)));
    }

    public static List<List<T>> Split<T>(this List<T> list, int count)
    {
        if (count <= 0)
        {
            throw new ArgumentOutOfRangeException("count");
        }

        var result = new List<List<T>>();
        List<T> curRun = null;
        for (int i = 0; i < list.Count; i++)
        {
            if (i % count == 0)
            {
                curRun = new List<T>();
                result.Add(curRun);
            }
            curRun.Add(list[i]);
        }
        return result;
    }
}

public static class LinkedListExtension
{
    public static void RemoveAll<T>(this LinkedList<T> list, Func<T, bool> pred)
    {
        RemoveAll(list, null, pred);
    }

    public static void RemoveAll<T>(this LinkedList<T> list, LinkedListNode<T> start, Func<T, bool> pred)
    {
        if (start != null && start.List != list)
        {
            throw new ArgumentException("start");
        }

        for (var p = start ?? list.First; p != null; )
        {
            var next = p.Next;
            if (pred(p.Value))
            {
                list.Remove(p);
            }
            p = next;
        }
    }

    public static bool Contains<T>(this LinkedList<T> list, Func<T, bool> pred)
    {
        for (var p = list.First; p != null; p = p.Next)
        {
            if (pred(p.Value))
            {
                return true;
            }
        }
        return false;
    }
}


public static class StringExtensions
{
    public static bool EqualsIgnoreCase(this string a, string b)
    {
        return a.Equals(b, StringComparison.InvariantCultureIgnoreCase);
    }

    /// <summary>
    /// compare strings with current ui culture
    /// </summary>
    public static int CompareWithUICulture(this string a, string b, CompareOptions op = CompareOptions.None)
    {
        return string.Compare(a, b, CultureInfo.CurrentUICulture, op);
    }

    public static string Capitalize(this string a)
    {
        if (a == "")
        {
            return "";
        }

        return Char.ToUpperInvariant(a[0]) + a.Substring(1);
    }
}

public static class VectorExtensions
{
    public static Vector2 xz(this Vector3 v)
    {
        return new Vector2(v.x, v.z);
    }

    public static Vector2 xy(this Vector3 v)
    {
        return new Vector2(v.x, v.y);
    }

    public static Vector3 xzAtY(this Vector2 v, float y = 0)
    {
        return new Vector3(v.x, y, v.y);
    }

    public static bool Approximate(this Vector2 v0, Vector2 v1, float delta)
    {
        for (int i = 0; i < 2; ++i)
        {
            if (Mathf.Abs(v0[i] - v1[i]) > delta)
            {
                return false;
            }
        }
        return true;
    }

    public static uint AsUInt(this bool value)
    {
        return value ? 1u : 0u;
    }
}

public static class StopwatchExtensions
{
    public static double Microseconds(this Stopwatch sw)
    {
        return (double)sw.ElapsedTicks * 1000000 / Stopwatch.Frequency;
    }
}

public static class JsonDataExtensions
{
    public static int[] GetIntArray(this JsonData data)
    {
        var arr = new int[data.Count];
        for (int i = 0; i < arr.Length; ++i)
        {
            arr[i] = (int)data[i];
        }
        return arr;
    }

    public static string[] GetStringArray(this JsonData data)
    {
        var arr = new string[data.Count];
        for (int i = 0; i < arr.Length; ++i)
        {
            arr[i] = (string)data[i];
        }
        return arr;
    }

    public static float GetFloat(this JsonData data)
    {
        return (float)(double)data;
    }

    public static int GetInt(this JsonData data)
    {
        return (int)(double)data;
    }

    public static bool HasKey(this JsonData data, string key)
    {
        var dict = data as IDictionary;
        return dict.Contains(key);
    }
}

public static class IEnumerableExtensions
{
    public static int FindIndex<T>(this IEnumerable<T> seq, Predicate<T> pred)
    {
        if (pred == null)
        {
            throw  new ArgumentNullException("pred");
        }

        int index = 0;
        foreach (var elem in seq)
        {
            if (pred(elem))
            {
                return index;
            }
            ++index;
        }
        return -1;
    }

    public static IEnumerable<T> Except<T>(this IEnumerable<T> seq, T value)
    {
        return Enumerable.Except(seq, new T[] { value });
    }
}

public static class ArrayExtensions
{
    public static void Swap<T>(this T[] array, int a, int b)
    {
        T tmp = array[a];
        array[a] = array[b];
        array[b] = tmp;
    }

    public static T[] Repeat<T>(this T[] array, int times)
    {
        if (times < 0)
        {
            throw new ArgumentOutOfRangeException("times");
        }

        var res = new T[array.Length * (times + 1)];
        for (int i = 0; i < res.Length; i += array.Length)
        {
            Array.Copy(array, 0, res, i, array.Length);
        }
        return res;
    }
}

public static class DelegateExtensions
{
    public static bool IsSafeToInvoke(this Delegate del)
    {
        if (del.Target != null)
        {
            // NOTE: This cannot detect the case when MonoBehaviour is captured by anonymous delegate.
            MonoBehaviour tCurObj = del.Target as MonoBehaviour;
            if (!ReferenceEquals(tCurObj, null) && !tCurObj)
            {
                return false;
            }
        }

        return true;
    }
}

public static class TypeExtensions
{
    public static T FirstCustomAttribute<T>(this Type type, bool inherit) where T : Attribute
    {
        return (T)type.GetCustomAttributes(inherit).FirstOrDefault(x => x is T);
    }

    public static T FirstCustomAttribute<T>(this FieldInfo fi, bool inherit) where T : Attribute
    {
        return (T)fi.GetCustomAttributes(inherit).FirstOrDefault(x => x is T);
    }
}

public static class ComparisionExtensions
{
    /// <summary>
    /// invert the comparison result based on the flag
    /// </summary>
    public static Comparison<T> Invert<T>(this Comparison<T> comp, bool invert)
    {
        if (invert)
        {
            return (lhs, rhs) => -comp(lhs, rhs);
        }
        return comp;
    }
}

public static class DateTimeExtensions
{
    public static long SecondsSinceEpoch(this DateTime dateTimeUtc)
    {
        return (long)(dateTimeUtc - TimeUtils.EpochTimeUTC).TotalSeconds;
    }
}
