using UnityEngine;
using UnityEngine.UI;

namespace Gameboard
{
    public class UIGameboardInfoPanel : MonoBehaviour
    {
        public MouseWorldPosition m_worldPosition;
        public Text m_mousePosText;

        public MouseScreenPosition m_screenPosition;
        public Text m_mouseScreenText;

        void Start()
        {
            m_worldPosition.onPositionChanged += OnWorldPositionChanged;
            m_screenPosition.onPositionChanged += OnScreenPositionChanged;

            OnWorldPositionChanged(Vector3.zero);
            OnScreenPositionChanged(Vector2.zero);
        }

        public void ShowMouseWorldPosition(bool show)
        {
            m_mousePosText.enabled = show;
        }

        private void OnWorldPositionChanged(Vector3 pos)
        {
            pos = Coordinates.ConvertVector(pos);
            m_mousePosText.text = "ui_gameboard_pointer_world_pos".Localize(pos.x, pos.y, pos.z);
        }

        private void OnScreenPositionChanged(Vector2 pos)
        {
            m_mouseScreenText.text = "ui_gameboard_pointer_screen_pos".Localize(pos.x, pos.y);
        }
    }
}
