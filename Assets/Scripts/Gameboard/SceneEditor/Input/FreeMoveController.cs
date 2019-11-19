using System;
using UnityEngine;

namespace Gameboard
{
    public class FreeMoveController
    {
        private readonly Entity m_entity;
        private readonly Camera m_camera;
        private readonly Vector3 m_startPos;
        private readonly Vector3 m_startPointerWorldPos;

        public FreeMoveController(Entity entity, Camera camera, Vector2 inputPos)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }
            if (camera == null)
            {
                throw new ArgumentNullException("camera");
            }

            m_entity = entity;
            m_camera = camera;
            m_startPos = entity.transform.position;
            m_startPointerWorldPos = GetPointerPosOnMovePlane(inputPos);
        }

        private Vector3 GetPointerPosOnMovePlane(Vector2 pos)
        {
            var ray = m_camera.ScreenPointToRay(pos);
            var plane = new Plane(m_camera.transform.forward, m_startPos);

            float t;
            plane.Raycast(ray, out t);
            return ray.GetPoint(t);
        }

        public void OnDrag(Vector2 inputPosition)
        {
            m_entity.transform.position = m_startPos + GetPointerPosOnMovePlane(inputPosition) - m_startPointerWorldPos;
        }
    }
}
