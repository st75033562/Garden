using RobotSimulation;
using UnityEngine;

namespace Gameboard
{
    public class ScreenPosition : Positional
    {
        [SerializeField]
        private Transform m_rotationAnchor;

        public override Vector3 position
        {
            get
            {
                var scenePos = entity.sceneRoot.InverseTransformPoint(transform.position);
                return new Vector3(scenePos.x, -scenePos.y, 0);
            }
            set
            {
                // ignore z
                transform.position = entity.sceneRoot.TransformPoint(new Vector3(value.x, -value.y, 0.0f));
            }
        }

        public override Vector3 rotation
        {
            get
            {
                return new Vector3(0, 0, -m_rotationAnchor.eulerAngles.z);
            }
            set
            {
                m_rotationAnchor.eulerAngles = new Vector3(0, 0, -value.z);
            }
        }

        public override Vector3 localScale
        {
            get
            {
                return transform.localScale;
            }
            set
            {
                transform.localScale = value;
            }
        }

        void Reset()
        {
            m_rotationAnchor = transform.GetChild(0);
        }
    }
}
