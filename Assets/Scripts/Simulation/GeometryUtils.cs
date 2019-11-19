using System.Collections.Generic;
using UnityEngine;

namespace RobotSimulation
{
    /// <summary>
    /// an oriented rectangle
    /// </summary>
    public class Rectangle
    {
        public Vector2 center;
        /// <summary>
        /// half size vector
        /// </summary>
        public Vector2 dx;
        public Vector2 dy;

        /// <summary>
        /// 0 - bottom left
        /// 1 - top left
        /// 2 - top right
        /// 3 - bottom right
        /// </summary>
        public readonly Vector2[] corners = new Vector2[4];

        // call this after making changes to center/dx/dy
        public void UpdateCorners()
        {
            var left = center - dx;
            corners[0] = left - dy;
            corners[1] = left + dy;

            var right = center + dx;
            corners[2] = right + dy;
            corners[3] = right - dy;
        }

        public Rect AABB
        {
            get
            {
                var minX = Mathf.Min(corners[0].x, corners[1].x, corners[2].x, corners[3].x);
                var maxX = Mathf.Max(corners[0].x, corners[1].x, corners[2].x, corners[3].x);
                var minY = Mathf.Min(corners[0].y, corners[1].y, corners[2].y, corners[3].y);
                var maxY = Mathf.Max(corners[0].y, corners[1].y, corners[2].y, corners[3].y);
                return new Rect(minX, minY, maxX - minX, maxY - minY);
            }
        }

        public float area
        {
            get { return 4 * Mathf.Sqrt(dx.sqrMagnitude * dy.sqrMagnitude); }
        }
    }

    public struct Interval
    {
        public float min;
        public float max;

        public Interval(float min, float max)
        {
            this.min = min;
            this.max = max;
        }

        public float length
        {
            get { return max - min; }
        }

        public bool Intersect(Interval rhs)
        {
            return !(max <= rhs.min || min >= rhs.max);
        }
    }

    public static class GeometryUtils
    {
        public const float Epsilon = 1e-6f;

        private static readonly List<Vector2> s_tmp = new List<Vector2>(3);
        private static readonly Vector2[] s_tmpRectCorners = new Vector2[4];

        /// <summary>
        /// clip a polygon against another polygon, output the resulting polygon's vertices
        /// <para/>polygon vertices are ordered in clock-wise
        /// <para/>see https://www.wikiwand.com/en/Sutherland%E2%80%93Hodgman_algorithm
        /// </summary>
        public static void Clip(Vector2[] poly, Vector2[] clippingPoly, List<Vector2> vertices)
        {
            vertices.Capacity = vertices.Count;

            var input = s_tmp;
            var output = vertices;
            input.AddRange(poly);

            // for each polygon edge
            for (int j = 0, i = clippingPoly.Length - 1; j < clippingPoly.Length && input.Count > 0; i = j, ++j)
            {
                float d0 = Classify(clippingPoly[i], clippingPoly[j], input[input.Count - 1]);
                for (int n = 0, m = input.Count - 1; n < input.Count; m = n, ++n)
                {
                    float d1 = Classify(clippingPoly[i], clippingPoly[j], input[n]);
                    if (d1 > 0)
                    {
                        if (d0 <= 0)
                        {
                            float t;
                            IntersectLines(input[m], input[n], clippingPoly[i], clippingPoly[j], out t);
                            output.Add(Vector2.Lerp(input[m], input[n], t));
                        }
                        output.Add(input[n]);
                    }
                    else
                    {
                        if (d0 > 0)
                        {
                            float t;
                            IntersectLines(input[m], input[n], clippingPoly[i], clippingPoly[j], out t);
                            output.Add(Vector2.Lerp(input[m], input[n], t));
                        }
                    }

                    d0 = d1;
                }

                Utils.Swap(ref input, ref output);
                output.Clear();
            }

            if (output == vertices)
            {
                vertices.Clear();
                vertices.AddRange(input);
            }
            s_tmp.Clear();
        }

        /// <summary>
        /// classify a point with respect to a line
        /// <para>the right side of the line is positive</para>
        /// </summary>
        /// <param name="v0">start point</param>
        /// <param name="v1">end point</param>
        /// <param name="p">testing point</param>
        /// <returns>0 if p is on the line, &lt; 0 if p is on negative side otherwise on positive side</returns>
        public static float Classify(Vector2 v0, Vector2 v1, Vector2 p)
        {
            var d0 = v1 - v0;
            var d1 = p - v0;

            var d = d0.y * d1.x - d0.x * d1.y;
            if (Mathf.Abs(d) <= Epsilon)
            {
                return 0;
            }
            return d;
        }

        /// <summary>
        /// calculate the intersection parameter on line v0v1 against line p0p1
        /// <para>use v0 + (pv1 - v0) * t to calculate the intersection point</para>
        /// </summary>
        /// <returns>if parallel, return false</returns>
        public static bool IntersectLines(Vector2 v0, Vector2 v1, Vector2 p0, Vector2 p1, out float t)
        {
            var d0 = v1 - v0;
            var d1 = p1 - p0;
            var d = p0 - v0;

            var denom = d0.x * -d1.y + d0.y * d1.x;
            if (Mathf.Abs(denom) <= Epsilon)
            {
                t = 0.0f;
                return false;
            }

            var numerator = d.x * -d1.y + d.y * d1.x;
            t = numerator / denom;
            return true;
        }

        /// <summary>
        /// compute the area of a polygon
        /// </summary>
        public static float ComputeArea(IList<Vector2> vertices)
        {
            if (vertices.Count < 3)
            {
                return 0;
            }

            float area = 0.0f;
            for (int j = 0, i = vertices.Count - 1; j < vertices.Count; i = j, j++)
            {
                area += (vertices[j].x - vertices[i].x) * (vertices[i].y + vertices[j].y);
            }
            return area * 0.5f;
        }

        /// <summary>
        /// compute interval along the given axis
        /// </summary>
        public static Interval ComputeInterval(Vector2[] poly, Vector2 dir)
        {
            var interval = new Interval(float.MaxValue, float.MinValue);
            for (int i = 0; i < poly.Length; ++i)
            {
                var d = Vector2.Dot(poly[i], dir);
                if (interval.min > d)
                {
                    interval.min = d;
                }
                if (interval.max < d)
                {
                    interval.max = d;
                }
            }
            return interval;
        }

        public static Interval ComputeIntervalX(Vector2[] poly)
        {
            var interval = new Interval(float.MaxValue, float.MinValue);
            for (int i = 0; i < poly.Length; ++i)
            {
                if (interval.min > poly[i].x)
                {
                    interval.min = poly[i].x;
                }
                if (interval.max < poly[i].x)
                {
                    interval.max = poly[i].x;
                }
            }
            return interval;
        }

        public static Interval ComputeIntervalY(Vector2[] poly)
        {
            var interval = new Interval(float.MaxValue, float.MinValue);
            for (int i = 0; i < poly.Length; ++i)
            {
                if (interval.min > poly[i].y)
                {
                    interval.min = poly[i].y;
                }
                if (interval.max < poly[i].y)
                {
                    interval.max = poly[i].y;
                }
            }
            return interval;
        }

        /// <summary>
        /// SAT test for intersection of two polygons
        /// </summary>
        public static bool Intersect(Vector2[] polyA, Vector2[] polyB)
        {
            return SATAgainst(polyA, polyB) && SATAgainst(polyB, polyA);
        }

        private static bool SATAgainst(Vector2[] polyA, Vector2[] polyB)
        {
            for (int j = 0, i = polyB.Length - 1; j < polyB.Length; i = j, ++j)
            {
                var dir = PerpCCW(polyB[j] - polyB[i]);
                var ia = ComputeInterval(polyA, dir);
                var ib = ComputeInterval(polyB, dir);

                if (!ia.Intersect(ib))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// return the vector rotated counter clock-wise 90 degrees
        /// </summary>
        public static Vector2 PerpCCW(Vector2 v)
        {
            return new Vector2(-v.y, v.x);
        }

        /// <summary>
        /// return the vector rotated clock-wise 90 degrees
        /// </summary>
        public static Vector2 PerpCW(Vector2 v)
        {
            return new Vector2(v.y, -v.x);
        }

        /// <summary>
        /// test intersection of a rect with a polygon
        /// </summary>
        public static bool Intersect(Rect rc, Vector2[] poly)
        {
            // test poly against the rect
            var interval = ComputeIntervalX(poly);
            if (interval.max <= rc.xMin || interval.min >= rc.xMax)
            {
                return false;
            }

            interval = ComputeIntervalY(poly);
            if (interval.max <= rc.yMin || interval.min >= rc.yMax)
            {
                return false;
            }

            GetCorners(rc, s_tmpRectCorners);
            return SATAgainst(s_tmpRectCorners, poly);
        }

        /// <summary>
        /// compute AABB of a polygon
        /// </summary>
        public static Rect ComputeAABB(Vector2[] poly)
        {
            var intervalX = ComputeIntervalX(poly);
            var intervalY = ComputeIntervalY(poly);

            return new Rect(intervalX.min, intervalY.min, intervalX.length, intervalY.length);
        }

        public static void GetCorners(Rect rc, Vector2[] corners)
        {
            corners[0] = new Vector2(rc.xMin, rc.yMin);
            corners[1] = new Vector2(rc.xMin, rc.yMax);
            corners[2] = new Vector2(rc.xMax, rc.yMax);
            corners[3] = new Vector2(rc.xMax, rc.yMin);
        }

        /// <summary>
        /// normalize the angle into [0, 360)
        /// </summary>
        /// <param name="angle"></param>
        /// <returns></returns>
        public static float NormalizeAngle(float angle)
        {
            angle = angle % 360;
            if (angle < 0)
            {
                angle += 360;
            }
            if (Mathf.Approximately(angle, 360))
            {
                angle = 0;
            }
            return angle;
        }

        /// <summary>
        /// compute the line parameter of the projection of p on the segment
        /// </summary>
        public static float ComputeSegmentT(Vector2 start, Vector2 end, Vector2 p)
        {
            var d = end - start;
            var d2 = d.sqrMagnitude;
            if (d2 <= Epsilon)
            {
                return 0;
            }

            return Vector2.Dot(p - start, d) / d2;
        }

        /// <summary>
        /// compute the line parameter of the projection of p on the segment
        /// </summary>
        public static float ComputeSegmentT(Vector3 start, Vector3 end, Vector3 p)
        {
            var d = end - start;
            var d2 = d.sqrMagnitude;
            if (d2 <= Epsilon)
            {
                return 0;
            }

            return Vector3.Dot(p - start, d) / d2;
        }

        /// <summary>
        /// compute the squared distance from point p to the segment
        /// </summary>
        /// <returns></returns>
        public static float SqrDistanceToSegment(Vector2 p0, Vector2 p1, Vector2 p)
        {
            var p0p1 = p1 - p0;
            var d2 = p0p1.sqrMagnitude;
            var p0p = p - p0;

            if (d2 <= Epsilon)
            {
                return p0p.sqrMagnitude;
            }

            var t = Vector2.Dot(p0p, p0p1) / d2;
            if (t <= 0)
            {
                return p0p.sqrMagnitude;
            }
            if (t >= 1)
            {
                return (p - p1).sqrMagnitude;
            }

            return p0p.sqrMagnitude - t * t * d2;
        }

        /// <summary>
        /// true if hat(d0) dot hat(d1) >= threshold, if either vector is close to zero, return false
        /// </summary>
        public static bool IsClose(Vector3 d0, Vector3 d1, float threshold)
        {
            var d02 = d0.sqrMagnitude;
            var d01 = d1.sqrMagnitude;

            if (d02 <= Epsilon || d01 <= Epsilon)
            {
                return false;
            }

            var dot = Vector3.Dot(d0, d1);
            return dot * dot / (d02 * d01) >= threshold;
        }

        public static bool IsInCircle(Vector2 center, float r, Vector2 p)
        {
            return (p - center).sqrMagnitude <= r * r;
        }

        public static bool IsCloseToSegment(Vector2 start, Vector2 end, Vector2 p, float threshold)
        {
            return SqrDistanceToSegment(start, end, p) <= threshold * threshold;
        }
    }
}
