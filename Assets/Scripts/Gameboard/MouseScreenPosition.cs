using System;
using UnityEngine;

namespace Gameboard
{
    public class MouseScreenPosition : MonoBehaviour
    {
        public event Action<Vector2> onPositionChanged;
        public RectTransform m_uiRoot;

        private Camera m_worldCamera;

        void Awake()
        {
            m_worldCamera = m_uiRoot.GetComponentInParent<Canvas>().worldCamera;
        }

        public Vector2 position
        {
            get;
            private set;
        }

        void Update()
        {
            Vector2 localPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                m_uiRoot, Input.mousePosition, m_worldCamera, out localPos);
            localPos.y = Mathf.RoundToInt(-localPos.y);
            localPos.x = Mathf.RoundToInt(localPos.x);
            if (localPos != position)
            {
                position = localPos;
                if (onPositionChanged != null)
                {
                    onPositionChanged(position);
                }
            }
        }
    }
}
