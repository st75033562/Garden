using UnityEngine;

namespace RobotSimulation
{
    public class FloorSensor : MonoBehaviour
    {
        [SerializeField]
        private Vector2 m_halfSize;

        private readonly Rectangle m_bound = new Rectangle();
        private const int MaxValue = 100;

        public IFloor floor
        {
            get;
            set;
        }

        public int value
        {
            get;
            private set;
        }

        public NormalNoise noise
        {
            get;
            set;
        }

        void Update()
        {
            if (floor != null)
            {
                m_bound.center = transform.position.xz();
                m_bound.dx = (transform.right * m_halfSize.x).xz();
                m_bound.dy = (transform.forward * m_halfSize.y).xz();
                m_bound.UpdateCorners();

                float newValue = Mathf.RoundToInt(floor.ComputeLightness(m_bound) * MaxValue);
                value = Mathf.Clamp((int)(noise != null ? noise.Apply(newValue) : newValue), 0, MaxValue);
            }
        }

        void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(m_halfSize.x * 2, 0.1f, m_halfSize.y * 2));
            DebugUtils.DrawText(transform.position, value.ToString(), Color.white);
        }
    }
}
