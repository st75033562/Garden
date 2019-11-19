using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RobotSimulation
{
    public class FloorGrid : MonoBehaviour, IFloor
    {
        [Serializable]
        public class PathInfo
        {
            public GameObject meshObject;
            /// <summary>
            /// normalized lightness value
            /// </summary>
            public float lightness;
#if UNITY_EDITOR
            // for debugging
            public Color intersectedTriangleColor = Color.red;
            public Color clippedRegionColor = Color.green;
#endif
        }

        // in case the sensor is not above any path
        public float defaultLightness;

        public PathInfo[] paths;
        public MeshGrid partition;

        private readonly MeshIntersectResult m_meshQueryResult = new MeshIntersectResult();
        private readonly List<Vector2> m_vertices = new List<Vector2>();
        private readonly Vector2[] m_triangle = new Vector2[3];

        public float ComputeLightness(Rectangle rc)
        {
            float value = 0.0f;
            float totalArea = rc.area;
            float nonIntersectedArea = totalArea;
            if (partition.GetIntersectedTriangles(rc, m_meshQueryResult))
            {
                for (int i = 0; i < m_meshQueryResult.subMeshCount; ++i)
                {
                    var mesh = m_meshQueryResult.GetSubMesh(i);
                    for (int j = 0; j < mesh.vertices.Count; j += 3)
                    {
                        m_triangle[0] = mesh.vertices[j];
                        m_triangle[1] = mesh.vertices[j + 1];
                        m_triangle[2] = mesh.vertices[j + 2];

                        m_vertices.Clear();
                        GeometryUtils.Clip(rc.corners, m_triangle, m_vertices);
                        float area = GeometryUtils.ComputeArea(m_vertices);
                        value += area * paths[mesh.meshId].lightness;
                        nonIntersectedArea -= area;

#if UNITY_EDITOR
                        DebugUtils.DrawLineLoop(m_triangle, 0.0f, paths[mesh.meshId].intersectedTriangleColor);
                        DebugUtils.DrawLineLoop(m_vertices, 
                            paths[mesh.meshId].meshObject.transform.position.y,
                            paths[mesh.meshId].clippedRegionColor);
#endif
                    }
                }
            }

            // calculate the default value
            value += defaultLightness * Mathf.Max(nonIntersectedArea, 0);

            // normalization
            return Mathf.Min(value / totalArea, 1);
        }

        public IEnumerable<GameObject> meshObjects
        {
            get { return paths.Select(x => x.meshObject); }
        }
    }
}
