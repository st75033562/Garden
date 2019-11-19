using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace RobotSimulation
{
    public class NaiveMesh : MonoBehaviour
    {
        public GameObject[] meshObjects;

        private Vector2[][] m_vertices;
        private int[][] m_indices;
        private Vector2[] m_triangle = new Vector2[3];

        void Start()
        {
            Build();
        }

        private void Build()
        {
            m_vertices = new Vector2[meshObjects.Length][];
            m_indices = new int[meshObjects.Length][];

            for (int i = 0; i < meshObjects.Length; ++i)
            {
                var mesh = meshObjects[i].GetComponent<MeshFilter>().sharedMesh;
                m_vertices[i] = mesh.vertices.Select(x => meshObjects[i].transform.TransformPoint(x).xz()).ToArray();
                m_indices[i] = mesh.triangles;
            }
        }

        public void GetIntersectedTriangles(Rectangle rc, MeshIntersectResult result)
        {
            result.Clear();
            result.SetMaxSubMeshCount(2);

            for (int i = 0; i < m_indices.Length; ++i)
            {
                for (int j = 0; j < m_indices[i].Length; j += 3)
                {
                    m_triangle[0] = m_vertices[i][m_indices[i][j]];
                    m_triangle[1] = m_vertices[i][m_indices[i][j + 1]];
                    m_triangle[2] = m_vertices[i][m_indices[i][j + 2]];

                    if (GeometryUtils.Intersect(rc.corners, m_triangle))
                    {
                        result.Add(i, m_triangle);
                    }
                }
            }
        }
    }
}
