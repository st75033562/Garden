using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RobotSimulation
{
    [Serializable]
    public struct GridCell
    {
        [SerializeField]
        private IndexedMeshTriangle[] m_triangles;

        // unity does not like members of serializable struct have initializers
        public void Init()
        {
            m_triangles = new IndexedMeshTriangle[0];
        }

        public IndexedMeshTriangle[] triangles
        {
            get { return m_triangles; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException();
                }
                m_triangles = value;
            }
        }

        public void Add(IndexedMeshTriangle tri)
        {
            var newTri = new IndexedMeshTriangle[m_triangles.Length + 1];
            Array.Copy(m_triangles, newTri, m_triangles.Length);
            newTri[m_triangles.Length] = tri;
            m_triangles = newTri;
        }

        public int numTriangles
        {
            get { return m_triangles.Length; }
        }

        public IndexedMeshTriangle this[int index]
        {
            get { return m_triangles[index]; }
        }
    }

    [Serializable]
    public struct IndexedMeshTriangle : IEquatable<IndexedMeshTriangle>
    {
        private const int Mask = 0xFFFF;
        private const int TriShift = 0;
        private const int MeshShift = 16;

        [SerializeField]
        private int m_key;

        public IndexedMeshTriangle(short meshId, short tri)
        {
            m_key = ((ushort)meshId << MeshShift) | ((ushort)tri << TriShift);
        }

        public short MeshId
        {
            get { return (short)((m_key >> MeshShift) & Mask); }
            set
            {
                m_key = ((ushort)value << MeshShift) | ((ushort)TriangleId << TriShift);
            }
        }

        public short TriangleId
        {
            get { return (short)((m_key >> TriShift) & Mask); }
            set
            {
                m_key = ((ushort)value << TriShift) | ((ushort)MeshId << MeshShift);
            }
        }

        public int Key
        {
            get { return m_key; }
            set { m_key = value; }
        }

        public bool Equals(IndexedMeshTriangle rhs)
        {
            return m_key == rhs.m_key;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is IndexedMeshTriangle))
            {
                return false;
            }

            return Equals((IndexedMeshTriangle)obj);
        }

        public override int GetHashCode()
        {
            return m_key.GetHashCode();
        }
    }

    [Serializable]
    public class MeshData
    {
        [SerializeField]
        private Vector2[] m_vertices = new Vector2[0];

        [SerializeField]
        private short[] m_indices = new short[0];

        public MeshData()
        {
        }

        public MeshData(Vector2[] vertices, short[] indices)
        {
            this.vertices = vertices;
            this.indices = indices;
        }

        public Vector2[] vertices
        {
            get { return m_vertices; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("vertices");
                }
                m_vertices = value;
            }
        }

        public short[] indices
        {
            get { return m_indices; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("indices");
                }

                if (value.Length % 3 != 0)
                {
                    throw new ArgumentException("length of indices must be multiple of 3");
                }

                m_indices = value;
            }
        }
        
        public int numTriangles
        {
            get { return indices.Length / 3; }
        }

        public void GetTriangle(int tri, Vector2[] output)
        {
            output[0] = vertices[indices[tri * 3]];
            output[1] = vertices[indices[tri * 3 + 1]];
            output[2] = vertices[indices[tri * 3 + 2]];
        }
    }

    public class IntersectedMeshInfo
    {
        internal int m_meshId = -1;
        internal readonly List<Vector2> m_vertices = new List<Vector2>();

        public IList<Vector2> vertices
        {
            get { return m_vertices; }
        }

        public int meshId
        {
            get { return m_meshId; }
        }

        public void Reset()
        {
            m_meshId = -1;
            m_vertices.Clear();
        }
    }

    public class MeshIntersectResult
    {
        private IntersectedMeshInfo[] m_meshes = new IntersectedMeshInfo[0];
        private int m_usedCount;

        internal void SetMaxSubMeshCount(int count)
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (m_meshes.Length < count)
            {
                var newMeshes = new IntersectedMeshInfo[count];
                Array.Copy(m_meshes, newMeshes, m_meshes.Length);
                for (int i = m_meshes.Length; i < newMeshes.Length; ++i)
                {
                    newMeshes[i] = new IntersectedMeshInfo();
                }
                m_meshes = newMeshes;
            }
        }

        public void Add(int meshId, Vector2[] vertices)
        {
            foreach (var mesh in m_meshes)
            {
                if (mesh.meshId == -1)
                {
                    ++m_usedCount;
                    mesh.m_meshId = meshId;
                    mesh.m_vertices.AddRange(vertices);
                    return;
                }
                else if (mesh.meshId == meshId)
                {
                    mesh.m_vertices.AddRange(vertices);
                    return;
                }
            }

            throw new InvalidOperationException();
        }

        public int subMeshCount
        {
            get { return m_usedCount; }
        }

        public IntersectedMeshInfo GetSubMesh(int index)
        {
            if (index < 0 || index >= m_usedCount)
            {
                throw new ArgumentOutOfRangeException();
            }

            return m_meshes[index];
        }

        internal void Clear()
        {
            foreach (var mesh in m_meshes)
            {
                mesh.Reset();
            }
            m_usedCount = 0;
        }
    }

    [Serializable]
    public class MeshGrid : ScriptableObject
    {
        [SerializeField]
        private Rect m_bound;

        [SerializeField]
        private int m_numCellX = 1;

        [SerializeField]
        private int m_numCellY = 1;

        [SerializeField]
        private GridCell[] m_cells = new GridCell[0];

        [SerializeField]
        private MeshData[] m_meshes;

        // temporary buffer for de-duplicate triangles
        private readonly HashSet<IndexedMeshTriangle> m_candidates = new HashSet<IndexedMeshTriangle>();

        private readonly Vector2[] m_triangle = new Vector2[3];

        private Vector2 m_cellSize;

        void Awake()
        {
            UpdateCellSize();
        }

        private void UpdateCellSize()
        {
            m_cellSize.x = m_bound.size.x / numCellX;
            m_cellSize.y = m_bound.size.y / numCellY;
        }

        public Vector2 cellSize
        {
            get { return m_cellSize; }
        }

        public Rect bound
        {
            get { return m_bound; }
        }

        public int numCellX
        {
            get { return m_numCellX; }
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException();
                }
                m_numCellX = value;
            }
        }

        public int numCellY
        {
            get { return m_numCellY; }
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException();
                }
                m_numCellY = value;
            }
        }

        /// <summary>
        /// rebuild the grid from the mesh objects, meshes must be readable
        /// </summary>
        /// <param name="mesh"></param>
        public void Build(GameObject[] meshObjects)
        {
            Bounds bounds = new Bounds();
            if (meshObjects.Length > 0)
            {
                bounds = meshObjects[0].GetComponent<Renderer>().bounds; 
            }
            foreach (var meshObj in meshObjects.Skip(1))
            {
                bounds.Encapsulate(meshObj.GetComponent<Renderer>().bounds);
            }

            m_bound = new Rect(bounds.min.xz(), bounds.size.xz());
            UpdateCellSize();

            m_cells = new GridCell[m_numCellX * m_numCellY];
            for (int i = 0; i < m_cells.Length; ++i)
            {
                m_cells[i].Init();
            }

            m_meshes = new MeshData[meshObjects.Length];
            for (int i = 0; i < meshObjects.Length; ++i)
            {
                Build(i, meshObjects[i]);
            }
        }

        private void Build(int meshIndex, GameObject meshObject)
        {
            var mesh = meshObject.GetComponent<MeshFilter>().sharedMesh;

            var meshData = m_meshes[meshIndex] = new MeshData();

            meshData.vertices = mesh.vertices.Select(x => {
                return meshObject.transform.TransformPoint(x).xz();
            }).ToArray();

            meshData.indices = mesh.triangles.Select(x => (short)x).ToArray();

            for (int i = 0; i < meshData.numTriangles; ++i)
            {
                meshData.GetTriangle(i, m_triangle);
                var aabb = GeometryUtils.ComputeAABB(m_triangle);

                int minX, maxX, minY, maxY;
                GetOverlappedCells(aabb, out minX, out maxX, out minY, out maxY);

                for (int x = minX; x <= maxX; ++x)
                {
                    for (int y = minY; y <= maxY; ++y)
                    {
                        if (GeometryUtils.Intersect(GetCellRect(x, y), m_triangle))
                        {
                            m_cells[ToCellIndex(x, y)].Add(new IndexedMeshTriangle((short)meshIndex, (short)i));
                        }
                    }
                }
            }
        }

        private Rect GetCellRect(int cellX, int cellY)
        {
            float x = cellX * m_cellSize.x + m_bound.xMin;
            float y = cellY * m_cellSize.y + m_bound.yMin;

            return new Rect(x, y, m_cellSize.x, m_cellSize.y);
        }

        private void GetCellVertices(int cellX, int cellY, Vector2[] vertices)
        {
            float x = cellX * m_cellSize.x + m_bound.xMin;
            float y = cellY * m_cellSize.y + m_bound.yMin;

            vertices[0] = new Vector2(x, y);
            vertices[1] = new Vector2(x, y + m_cellSize.y);
            vertices[2] = new Vector2(x + m_cellSize.x, y + m_cellSize.y);
            vertices[3] = new Vector2(x + m_cellSize.x, y);
        }

        public GridCell? GetCell(Vector2 pos)
        {
            int x = ToCellX(pos.x);
            int y = ToCellX(pos.y);
            if (x < 0 || x >= numCellX || y < 0 || y >= numCellY)
            {
                return null;
            }

            return GetCell(x, y);
        }

        private GridCell GetCell(int x, int y)
        {
            return m_cells[ToCellIndex(x, y)];
        }

        private int ToCellIndex(int x, int y)
        {
            return y * numCellX + x;
        }

        public int ToCellX(float x)
        {
            return Mathf.FloorToInt((x - m_bound.xMin) / m_cellSize.x);
        }

        public int ToCellY(float y)
        {
            return Mathf.FloorToInt((y - m_bound.yMin) / m_cellSize.y);
        }

        public IList<GridCell> cells
        {
            get { return m_cells; }
        }

        /// <summary>
        /// get all intersected triangles
        /// </summary>
        /// <returns>true if there's any intersection</returns>
        public bool GetIntersectedTriangles(Rectangle rc, MeshIntersectResult result)
        {
            result.Clear();

            int minX, maxX, minY, maxY;
            if (!GetOverlappedCells(rc.AABB, out minX, out maxX, out minY, out maxY))
            {
                return false;
            }

            for (int i = minX; i <= maxX; ++i)
            {
                for (int j = minY; j <= maxY; ++j)
                {
                    var cell = GetCell(i, j);
                    for (int k = 0; k < cell.numTriangles; ++k)
                    {
                        m_candidates.Add(cell[k]);
                    }
                }
            }

            if (m_candidates.Count > 0)
            {
                result.SetMaxSubMeshCount(m_meshes.Length);
                foreach (var tri in m_candidates)
                {
                    m_meshes[tri.MeshId].GetTriangle(tri.TriangleId, m_triangle);
                    if (GeometryUtils.Intersect(rc.corners, m_triangle))
                    {
                        result.Add(tri.MeshId, m_triangle);
                    }
                }

                m_candidates.Clear();

                return true;
            }

            return false;
        }

        private bool GetOverlappedCells(Rect rc, out int minX, out int maxX, out int minY, out int maxY)
        {
            minX = ToCellX(rc.xMin);
            maxX = ToCellX(rc.xMax);

            if (maxX < 0 || minX >= numCellX)
            {
                minY = -1;
                maxY = -1;
                return false;
            }

            minY = ToCellY(rc.yMin);
            maxY = ToCellY(rc.yMax);

            if (maxY < 0 || minY >= numCellY)
            {
                return false;
            }

            minX = Mathf.Clamp(minX, 0, numCellX - 1);
            maxX = Mathf.Clamp(maxX, 0, numCellX - 1);

            minY = Mathf.Clamp(minY, 0, numCellY - 1);
            maxY = Mathf.Clamp(maxY, 0, numCellY - 1);

            return true;
        }
    }
}
