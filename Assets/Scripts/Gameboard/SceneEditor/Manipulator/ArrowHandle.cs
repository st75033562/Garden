using System;
using UnityEngine;
using RobotSimulation;

namespace Gameboard
{
    public class ArrowHandle : MonoBehaviour, IInputHandler
    {
        public Color m_color = Color.white;
        public Color m_pressColor = Color.yellow;
        public LineRenderer m_lineRenderer;

        public Collider m_coneCollider;
        public MeshRenderer m_coneRenderer;

        // screen size
        [SerializeField]
        private float m_lineHitDistance = 5;

        [SerializeField]
        private float m_coneHitRadius = 10;

        // local size
        public float m_coneHeight = 0.03f;

        private float m_originalLineWidth;

        [SerializeField]
        private Camera m_renderCamera;

        private Vector2 m_startScreenPos;
        private Vector2 m_endScreenPos;
        private Vector3 m_startTargetPos;
        private float m_pressPosT;
        private Vector3 m_startPos;

        void Awake()
        {
            m_originalLineWidth = m_lineRenderer.widthMultiplier;

            m_lineRenderer.material.color = m_color;
            m_coneRenderer.material.color = m_color;
        }

        public float lineHitDistance
        {
            get { return m_lineHitDistance; }
            set { m_lineHitDistance = Mathf.Max(1, value); }
        }

        public float coneHitRadius
        {
            get { return m_coneHitRadius; }
            set { m_coneHitRadius = Mathf.Max(1, value); }
        }

        // the render camera for input handling
        public Camera renderCamera
        {
            get
            {
                if (!m_renderCamera)
                {
                    m_renderCamera = Camera.main;
                }
                return m_renderCamera;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                m_renderCamera = value;
            }
        }

        public void SetSize(float size)
        {
            m_lineRenderer.widthMultiplier = m_originalLineWidth * size;
            transform.localScale = Vector3.one * size;
        }

        public Transform target
        {
            get;
            set;
        }

        public bool HitTest(Vector2 inputPosition)
        {
            var coneCenter = m_coneRenderer.transform.position + m_coneRenderer.transform.right * m_coneHeight;
            var screenCenter = renderCamera.WorldToScreenPoint(coneCenter);
            if (GeometryUtils.IsInCircle(screenCenter, m_coneHitRadius, inputPosition))
            {
                return true;
            }

            var end = m_lineRenderer.GetPosition(1);
            var startScrenPos = renderCamera.WorldToScreenPoint(transform.position);
            var endScreenPos = renderCamera.WorldToScreenPoint(transform.localToWorldMatrix.MultiplyPoint3x4(end));

            return GeometryUtils.IsCloseToSegment(startScrenPos, endScreenPos, inputPosition, m_lineHitDistance);
        }

        public void OnPointerDown(Vector2 inputPosition)
        {
            m_lineRenderer.material.color = m_pressColor;
            m_coneRenderer.material.color = m_pressColor;

            m_startScreenPos = renderCamera.WorldToScreenPoint(transform.position);
            m_endScreenPos = renderCamera.WorldToScreenPoint(transform.position + transform.right);
            m_startTargetPos = target ? target.position : Vector3.zero;
            m_pressPosT = GeometryUtils.ComputeSegmentT(m_startScreenPos, m_endScreenPos, inputPosition);
            m_startPos = transform.position;

            isDragging = true;
        }

        public void OnPointerUp()
        {
            m_lineRenderer.material.color = m_color;
            m_coneRenderer.material.color = m_color;

            isDragging = false;
        }

        public void OnDrag(Vector2 inputPosition)
        {
            var t = GeometryUtils.ComputeSegmentT(m_startScreenPos, m_endScreenPos, inputPosition);
            var delta = (t - m_pressPosT) * transform.right;
            if (target)
            {
                target.position = m_startTargetPos + delta;
            }
            transform.position = m_startPos + delta;
        }

        public bool isDragging
        {
            get;
            private set;
        }

        bool IInputHandler.enabled
        {
            get { return gameObject.activeInHierarchy; }
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            lineHitDistance = m_lineHitDistance;
            coneHitRadius = m_coneHitRadius;
        }
#endif
    }
}
