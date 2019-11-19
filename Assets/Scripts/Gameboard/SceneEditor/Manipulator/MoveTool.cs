using System;
using UnityEngine;
using RobotSimulation;

namespace Gameboard
{
    /// <summary>
    /// Tool for moving objects
    /// </summary>
    public class MoveTool : MonoBehaviour, IInputHandler
    {
        // x, y, z handle
        public ArrowHandle[] m_handles;

        [SerializeField]
        private float m_scale = 1.0f;

        private static readonly float HandleHiddenThreshold = Mathf.Cos(Mathf.Deg2Rad * 5);

        private Camera m_renderCamera;
        private ArrowHandle m_currentHandle;

        [SerializeField]
        private float m_lineHitDistance;

        [SerializeField]
        private float m_coneHitRadius;

        void Awake()
        {
            renderCamera = Camera.main;
        }

        public float scale
        {
            get { return m_scale; }
            set { m_scale = value; }
        }

        public float lineHitDistance
        {
            get { return m_lineHitDistance; }
            set
            {
                m_lineHitDistance = value;
                foreach (var handle in m_handles)
                {
                    handle.lineHitDistance = value;
                }
            }
        }

        public float coneHitRadius
        {
            get { return m_coneHitRadius; }
            set
            {
                m_coneHitRadius = value;
                foreach (var handle in m_handles)
                {
                    handle.coneHitRadius = value;
                }
            }
        }

        public Camera renderCamera
        {
            get { return m_renderCamera; }
            set
            {
                if (!value)
                {
                    throw new ArgumentNullException("value");
                }
                m_renderCamera = value;

                foreach (var handle in m_handles)
                {
                    handle.renderCamera = value;
                }
            }
        }

        public Transform target
        {
            get { return m_handles[0].target; }
            set
            {
                foreach (var handle in m_handles)
                {
                    handle.target = value;
                }
            }
        }

        void UpdateSize()
        {
            if (!m_renderCamera) { return; }

            var dirToTarget = transform.position - m_renderCamera.transform.position;
            var dist = Mathf.Abs(Vector3.Dot(dirToTarget, m_renderCamera.transform.forward));
            foreach (var handle in m_handles)
            {
                handle.SetSize(dist * m_scale);
            }
        }

        void LateUpdate()
        {
            if (m_currentHandle == null && m_renderCamera)
            {
                // hide the handle if it's almost parallel to the viewing direction
                var viewDir = transform.position - m_renderCamera.transform.position;
                foreach (var handle in m_handles)
                {
                    var visible = !GeometryUtils.IsClose(viewDir, handle.transform.right, HandleHiddenThreshold);
                    handle.gameObject.SetActive(visible);
                }
            }

            UpdateSize();
        }

        public bool HitTest(Vector2 inputPosition)
        {
            m_currentHandle = null;
            foreach (var handle in m_handles)
            {
                if (handle.HitTest(inputPosition))
                {
                    m_currentHandle = handle;
                    break;
                }
            }
            return m_currentHandle != null;
        }

        public void OnPointerDown(Vector2 inputPosition)
        {
            if (m_currentHandle)
            {
                m_currentHandle.OnPointerDown(inputPosition);

                foreach (var handle in m_handles)
                {
                    handle.gameObject.SetActive(true);
                }
            }
        }

        public void OnPointerUp()
        {
            if (m_currentHandle)
            {
                m_currentHandle.OnPointerUp();
                m_currentHandle = null;
            }
        }

        public void OnDrag(Vector2 inputPosition)
        {
            if (m_currentHandle)
            {
                m_currentHandle.OnDrag(inputPosition);

                transform.position = m_currentHandle.transform.position;
                m_currentHandle.transform.localPosition = Vector3.zero;
            }
        }

        bool IInputHandler.enabled
        {
            get { return gameObject.activeInHierarchy;}
        }
    }
}
