using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Gameboard
{
    public class MouseWorldPosition : MonoBehaviour
    {
        public event Action<Vector3> onPositionChanged;

        public UIGameboard m_uiGameboard;

        private Camera m_mainCamera;

        void Start()
        {
            m_uiGameboard.gameboardSceneManager.onEndLoading.AddListener(OnEndLoadingGameboard);
        }

        void OnEndLoadingGameboard()
        {
            m_mainCamera = m_uiGameboard.gameboardSceneManager.currentCamera;
        }

        public Vector3 position
        {
            get;
            private set;
        }

        void Update()
        {
            if (Application.isMobilePlatform && !Input.GetMouseButton(0))
            {
                return;
            }

            if (!m_mainCamera) { return; }
            var ray = m_mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, float.PositiveInfinity, 1 << PhysicsUtils.PlacementLayer))
            {
                if (position != hit.point)
                {
                    position = hit.point;
                    
                    if (onPositionChanged != null)
                    {
                        onPositionChanged(position);
                    }
                }
            }
        }
    }
}
