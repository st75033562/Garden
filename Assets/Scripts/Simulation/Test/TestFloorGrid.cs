using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace RobotSimulation
{
    public class TestFloorGrid : MonoBehaviour
    {
        public FloorGrid grid;
        public NaiveMesh naiveMesh;
        public BoxCollider m_collider;
        public Text gridTimeText;
        public Text naiveTimeText;

        public int updateFrequency;
        private MovingAverageFilter m_gridTimeFilter;
        private MovingAverageFilter m_naiveTimeFilter;

        public Color[] meshColors;

        private MeshIntersectResult m_queryResult = new MeshIntersectResult();
        private Rectangle m_rc = new Rectangle();
        private Stopwatch m_sw = new Stopwatch();
        private int m_updateCount;

    	// Use this for initialization
        void Start()
        {
            m_collider = GetComponent<BoxCollider>();
            m_gridTimeFilter = new MovingAverageFilter(updateFrequency);
            m_naiveTimeFilter = new MovingAverageFilter(updateFrequency);
    	}
    	
    	// Update is called once per frame
    	void Update () {
            var center = transform.TransformPoint(m_collider.center);
            var extent = transform.TransformVector(m_collider.size * 0.5f);

            m_rc.center = center.xz();
            m_rc.dx = (transform.right * extent.x).xz();
            m_rc.dy = (transform.forward * extent.z).xz();
            m_rc.UpdateCorners();

            m_sw.Reset();
            m_sw.Start();
            grid.partition.GetIntersectedTriangles(m_rc, m_queryResult);
            m_sw.Stop();

            for (int i = 0; i < m_queryResult.subMeshCount; ++i)
            {
                var subMesh = m_queryResult.GetSubMesh(i);
                DebugUtils.DrawTriangles(subMesh.vertices.Select(x => x.xzAtY(0.0f)).ToArray(), meshColors[subMesh.meshId]);
            }

            m_gridTimeFilter.addSample((float)m_sw.Microseconds());

            m_sw.Reset();
            m_sw.Start();
            naiveMesh.GetIntersectedTriangles(m_rc, m_queryResult);
            m_sw.Stop();

            m_naiveTimeFilter.addSample((float)m_sw.Microseconds());
            if (++m_updateCount == updateFrequency)
            {
                m_updateCount = 0;
                gridTimeText.text = Mathf.FloorToInt(m_gridTimeFilter.value).ToString();
                naiveTimeText.text = Mathf.FloorToInt(m_naiveTimeFilter.value).ToString();
            }
    	}
    }

}