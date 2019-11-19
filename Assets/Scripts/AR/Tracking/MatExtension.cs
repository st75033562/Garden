using OpenCVForUnity;
using System;
using UnityEngine;

namespace AR
{
    public static class MatExtension
    {
        // extract a column vector2 from range [row, col] - [row + 1, col]
        public static Vector2 ToVector_21(this Mat m, int row = 0, int col = 0)
        {
            if (m.channels() != 1)
            {
                throw new ArgumentException("invalid channels");
            }
            return new Vector2((float)m.get(row, col)[0], (float)m.get(row + 1, col)[0]);
        }

        // extract a row vector2 from range [row, col] - [row, col + 1]
        public static Vector2 ToVector2_12(this Mat m, int row = 0, int col = 0)
        {
            if (m.channels() != 1)
            {
                throw new ArgumentException("invalid channels");
            }
            return new Vector2((float)m.get(row, col)[0], (float)m.get(row, col + 1)[0]);
        }

        // extract a vector2 at [row][col]
        public static Vector2 ToVector2_11(this Mat m, int row = 0, int col = 0)
        {
            if (m.channels() != 2)
            {
                throw new ArgumentException("invalid channels");
            }
            var e = m.get(row, col);
            return new Vector2((float)e[0], (float)e[1]);
        }

        // extract a column vector3 from range [row, col] - [row + 2, col]
        public static Vector3 ToVector_31(this Mat m, int row = 0, int col = 0)
        {
            if (m.channels() != 1)
            {
                throw new ArgumentException("invalid channels");
            }
            return new Vector3((float)m.get(row, col)[0], (float)m.get(row + 1, col)[0], (float)m.get(row + 2, col)[0]);
        }

        // extract a row vector3 from range [row, col] - [row, col + 2]
        public static Vector3 ToVector3_13(this Mat m, int row = 0, int col = 0)
        {
            if (m.channels() != 1)
            {
                throw new ArgumentException("invalid channels");
            }
            return new Vector3((float)m.get(row, col)[0], (float)m.get(row, col + 1)[0], (float)m.get(row, col + 2)[0]);
        }

        // extract a vector3 at [row][col]
        public static Vector3 ToVector3_11(this Mat m, int row = 0, int col = 0)
        {
            if (m.channels() != 3)
            {
                throw new ArgumentException("invalid channels");
            }
            var e = m.get(row, col);
            return new Vector3((float)e[0], (float)e[1], (float)e[2]);
        }

        // set a column vector2 to range [row, col] - [row + 1, col]
        public static void Set_21(this Mat m, Vector2 v, int row = 0, int col = 0)
        {
            if (m.channels() != 1)
            {
                throw new ArgumentException("invalid channels");
            }

            m.put(row, col, v.x);
            m.put(row + 1, col, v.y);
        }

        // set a row vector2 to range [row, col] - [row, col + 1]
        public static void Set_12(this Mat m, Vector2 v, int row = 0, int col = 0)
        {
            if (m.channels() != 1)
            {
                throw new ArgumentException("invalid channels");
            }

            m.put(row, col, v.x);
            m.put(row, col + 1, v.y);
        }

        // set a vector2 to [row][col]
        public static void Set_11(this Mat m, Vector2 v, int row = 0, int col = 0)
        {
            if (m.channels() != 2)
            {
                throw new ArgumentException("invalid channels");
            }

            m.put(row, col, v.x, v.y);
        }

        // set a column vector3 to range [row, col] - [row + 2, col]
        public static void Set_31(this Mat m, Vector3 v, int row = 0, int col = 0)
        {
            if (m.channels() != 1)
            {
                throw new ArgumentException("invalid channels");
            }

            m.put(row, col, v.x);
            m.put(row + 1, col, v.y);
            m.put(row + 2, col, v.z);
        }

        // set a column vector3 to range [row, col] - [row, col + 2]
        public static void Set_13(this Mat m, Vector3 v, int row = 0, int col = 0)
        {
            if (m.channels() != 1)
            {
                throw new ArgumentException("invalid channels");
            }

            m.put(row, col, v.x);
            m.put(row, col + 1, v.y);
            m.put(row, col + 2, v.z);
        }

        // set a column vector3 to [row][col]
        public static void Set_11(this Mat m, Vector3 v, int row = 0, int col = 0)
        {
            if (m.channels() != 3)
            {
                throw new ArgumentException("invalid channels");
            }

            m.put(row, col, v.x, v.y, v.z);
        }
    }

}