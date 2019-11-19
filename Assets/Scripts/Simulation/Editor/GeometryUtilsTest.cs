using UnityEngine;
using NUnit.Framework;
using System.Collections.Generic;

namespace RobotSimulation
{
    public class GeometryUtilsTest
    {
        private const float Epsilon = 1e-6f;

        [Test]
        public void TestIntersectParallelLines()
        {
            var v0 = Vector2.zero;
            var v1 = Vector2.right;

            var p0 = v0 + Vector2.up;
            var p1 = v1 + Vector2.up;

            float t;
            Assert.IsFalse(GeometryUtils.IntersectLines(v0, v1, p0, p1, out t));
        }

        [Test]
        public void TestIntersectCoincidentLines()
        {
            var v0 = Vector2.zero;
            var v1 = Vector2.right;

            var p0 = v0;
            var p1 = v1;
            
            float t;
            Assert.IsFalse(GeometryUtils.IntersectLines(v0, v1, p0, p1, out t));
        }

        [Test]
        public void TestIntersectNonParallelLines()
        {
            var v0 = Vector2.zero;
            var v1 = Vector2.right;

            var p0 = v0;
            var p1 = Vector2.up;

            float t;
            Assert.IsTrue(GeometryUtils.IntersectLines(v0, v1, p0, p1, out t));
            Assert.AreEqual(0.0f, t, Epsilon);

            p0 = new Vector2(0.5f, -0.5f);
            p1 = new Vector2(0.5f, 0.5f);

            Assert.IsTrue(GeometryUtils.IntersectLines(v0, v1, p0, p1, out t));
            Assert.AreEqual(0.5f, t, Epsilon);
        }

        [Test]
        public void TestClassification()
        {
            var v0 = Vector2.left;
            var v1 = Vector2.right;

            Assert.AreEqual(0.0f, GeometryUtils.Classify(v0, v1, Vector2.zero));
            Assert.IsTrue(GeometryUtils.Classify(v0, v1, Vector2.up) < 0);
            Assert.IsTrue(GeometryUtils.Classify(v0, v1, Vector2.down) > 0);
        }

        [Test]
        public void TestClipTriangleInRect()
        {
            var tri = new[] {
                Vector2.left * 0.5f,
                Vector2.up * 0.5f,
                Vector2.right * 0.5f
            };
            var rect = GetRectanglePoly();
            var vertices = new List<Vector2>();

            GeometryUtils.Clip(tri, rect, vertices);
            UnitTestUtils.AreEqual(tri, vertices, Epsilon);
        }

        [Test]
        public void TestClipTriangleContainsRect()
        {
            var tri = new[] {
                new Vector2(-3, -1),
                new Vector2(0, 2),
                new Vector2(3, -1)
            };
            var rect = GetRectanglePoly();
            var vertices = new List<Vector2>();

            GeometryUtils.Clip(tri, rect, vertices);
            Utils.Rotate(rect, 3);
            UnitTestUtils.AreEqual(rect, vertices, Epsilon);
        }

        [Test]
        public void TestClipTriangleEdgeOnRect()
        {
            var tri = new[] {
                new Vector2(-1, 1),
                new Vector2(0, 2),
                new Vector2(1, 1)
            };
            var rect = GetRectanglePoly();
            var vertices = new List<Vector2>();

            GeometryUtils.Clip(tri, rect, vertices);
            Assert.IsEmpty(vertices);
        }

        [Test]
        public void TestClipTriangleOneVertexOnRect()
        {
            var tri = new[] {
                new Vector2(0, 1),
                new Vector2(-1, 2),
                new Vector2(1, 2),
            };
            var rect = GetRectanglePoly();
            var vertices = new List<Vector2>();

            GeometryUtils.Clip(tri, rect, vertices);
            Assert.IsEmpty(vertices);
        }

        [Test]
        public void TestClipTriangleOutsideOfRect()
        {
            var tri = new[] {
                new Vector2(0, 2),
                new Vector2(-1, 3),
                new Vector2(1, 3),
            };
            var rect = GetRectanglePoly();
            var vertices = new List<Vector2>();

            GeometryUtils.Clip(tri, rect, vertices);
            Assert.IsEmpty(vertices);
        }

        private Vector2[] GetRectanglePoly()
        {
            var rect = new Vector2[4];
            rect[0] = Vector2.left + Vector2.down;
            rect[1] = Vector2.left + Vector2.up;
            rect[2] = Vector2.right + Vector2.up;
            rect[3] = Vector2.right + Vector2.down;
            return rect;
        }

        [Test]
        public void TestNonPolygonArea()
        {
            Assert.AreEqual(GeometryUtils.ComputeArea(new Vector2[] { }), 0);
            Assert.AreEqual(GeometryUtils.ComputeArea(new Vector2[] { Vector2.zero }), 0);
            Assert.AreEqual(GeometryUtils.ComputeArea(new Vector2[] { Vector2.zero, Vector2.right }), 0);
        }

        [Test]
        public void TestDegenerateTriangleArea()
        {
            // point
            Assert.AreEqual(GeometryUtils.ComputeArea(new [] { Vector2.zero, Vector2.zero, Vector2.zero }), 0);
            // segment
            Assert.AreEqual(GeometryUtils.ComputeArea(new [] { Vector2.zero, Vector2.right, Vector2.zero }), 0);
        }

        [Test]
        public void TestRectangleArea()
        {
            var rect = GetRectanglePoly();
            var area = (rect[1].y - rect[0].y) * (rect[3].x - rect[0].x);
            Assert.AreEqual(GeometryUtils.ComputeArea(rect), area, Epsilon);
        }

        [Test]
        public void TestPolygonNoIntersection()
        {
            var rect = GetRectanglePoly();
            var tri = new[] {
                new Vector2(0, 3),
                new Vector2(3, 3),
                new Vector2(3, 0),
            };
            Assert.IsFalse(GeometryUtils.Intersect(rect, tri));
        }

        [Test]
        public void TestPolygonIntersection()
        {
            var rect = GetRectanglePoly();
            var tri = new[] {
                new Vector2(0, 1.5f),
                new Vector2(1.5f, 1.5f),
                new Vector2(1.5f, 0),
            };
            Assert.IsTrue(GeometryUtils.Intersect(rect, tri));
        }

        [Test]
        public void TestRectIntersectsPoly()
        {
            var rect = new Rect(-1, -1, 2, 2);
            var tri = new[] {
                new Vector2(0, 1.5f),
                new Vector2(1.5f, 1.5f),
                new Vector2(1.5f, 0),
            };
            Assert.IsTrue(GeometryUtils.Intersect(rect, tri));
        }

        [Test]
        public void TestRectNotIntersectPoly()
        {
            var rect = new Rect(-1, -1, 2, 2);
            var tri = new[] {
                new Vector2(0, 3),
                new Vector2(3, 3),
                new Vector2(3, 0),
            };
            Assert.IsFalse(GeometryUtils.Intersect(rect, tri));
        }

        [Test]
        public void TestComputeAABB()
        {
            var rect = GetRectanglePoly();
            var aabb = GeometryUtils.ComputeAABB(rect);

            Assert.AreEqual(aabb.xMin, rect[0].x, Epsilon);
            Assert.AreEqual(aabb.yMin, rect[0].y, Epsilon);
            Assert.AreEqual(aabb.xMax, rect[2].x, Epsilon);
            Assert.AreEqual(aabb.yMax, rect[2].y, Epsilon);
        }
    }
}