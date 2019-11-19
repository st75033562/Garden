using UnityEngine;

namespace Gameboard
{
    public class SelectionBox : MonoBehaviour
    {
        public Vector3 padding;

        private Collider m_targetCollider;

        public void Attach(Transform target)
        {
            m_targetCollider = target.GetComponent<Collider>();
            Update();
        }

        public void Detach()
        {
            m_targetCollider = null;
        }

        void Update()
        {
            if (m_targetCollider)
            {
                // box is 1x1x1
                var bounds = m_targetCollider.bounds;
                transform.position = bounds.center;
                // AABB
                transform.rotation = Quaternion.identity;
                transform.localScale = bounds.size + padding;
            }
        }
    }
}
