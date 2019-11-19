using OpenCVForUnity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace AR
{
    public static class Utils
    {
        public static Matrix4x4 TR(Mat rotMat, double[] tvec)
        {
            Matrix4x4 transformationM = new Matrix4x4();
			transformationM.SetRow(0, new Vector4((float)rotMat.get(0, 0)[0], (float)rotMat.get(0, 1)[0], (float)rotMat.get(0, 2)[0], (float)tvec[0]));
			transformationM.SetRow(1, new Vector4((float)rotMat.get(1, 0)[0], (float)rotMat.get(1, 1)[0], (float)rotMat.get(1, 2)[0], (float)tvec[1]));
			transformationM.SetRow(2, new Vector4((float)rotMat.get(2, 0)[0], (float)rotMat.get(2, 1)[0], (float)rotMat.get(2, 2)[0], (float)tvec[2]));
			transformationM.SetRow(3, new Vector4(0, 0, 0, 1));
            return transformationM;
        }

        public static void Swap<T>(ref T a, ref T b)
        {
            T tmp = a;
            a = b;
            b = tmp;
        }

        public static void Dispose<T>(ref T t) where T : class, IDisposable
        {
            if (t != null)
            {
                t.Dispose();
                t = null;
            }
        }

        public static void Dispose<T>(T t) where T : class, IDisposable
        {
            if (t != null)
            {
                t.Dispose();
            }
        }

        public static void Dispose<T>(IEnumerable<T> e) where T : IDisposable
        {
            if (e != null)
            {
                foreach (var i in e)
                {
                    i.Dispose();
                }
            }
        }

        public static void Dispose<T>(List<T> l) where T : IDisposable
        {
            if (l != null)
            {
                for (int i = 0; i < l.Count; ++i)
                {
                    l[i].Dispose();
                }
                l.Clear();
            }
        }

        public static string Format(Vector3 v)
        {
            return string.Format("{0:0.000000}, {1:0.000000}, {2:0.000000}", v.x, v.y, v.z);
        }

        public static Quaternion Normalize(Quaternion q)
        {
            float s = 1.0f / Mathf.Sqrt(q.x * q.x + q.y * q.y + q.z * q.z + q.w * q.w);
            q.x *= s;
            q.y *= s;
            q.z *= s;
            q.w *= s;
            return q;
        }


        public static void InsertionSort<T>(this T[] array, Comparison<T> comp = null)
        {
            if (comp == null)
            {
                comp = Comparer<T>.Default.Compare;
            }
            
            for (int i = 1; i < array.Length; ++i)
            {
                int j;
                for (j = i - 1; j >= 0; --j)
                {
                    if (comp(array[i], array[j]) >= 0)
                    {
                        break;
                    }
                }
                if (j + 1 != i)
                {
                    T tmp = array[i];
                    Array.Copy(array, j + 1, array, j + 2, i - j - 1);
                    array[j + 1] = tmp;
                }
            }
        }
    }

}