using System;
using UnityEngine;
using UnityEngine.UI;

namespace Gameboard
{
    public enum UIRobotMenuAction
    {
        EditCode,
        AssignCode,
        UnassignCode,
        Settings,
        Delete
    }

    public class UIRobotMenu : MonoBehaviour
    {
        public RobotColorSettings m_colorSettings;
        public Image m_robotImage;
        public Text m_robotIndexText;

        public RectTransform m_menuContainer;
        public GameObject m_assignButton;
        public GameObject m_unassignButton;
        public GameObject[] m_editButtons;

        public void Configure(int colorId, int robotIndex, bool hasCode, bool showEditRobotButtons)
        {
            m_robotImage.sprite = m_colorSettings.sprites[colorId];
            m_robotIndexText.text = robotIndex.ToString();

            m_assignButton.SetActive(!hasCode);
            m_unassignButton.SetActive(hasCode);
            foreach (var btn in m_editButtons)
            {
                btn.SetActive(showEditRobotButtons);
            }
        }

        public void Open(float screenTopY)
        {
            gameObject.SetActive(true);
            var pos = m_menuContainer.position;
            pos.y = screenTopY;
            m_menuContainer.position = pos;
        }

        public Action<UIRobotMenuAction> onActionClicked
        {
            get;
            set;
        }

        public Action onClosed
        {
            get;
            set;
        }

        public void Close()
        {
            gameObject.SetActive(false);
            if (onClosed != null)
            {
                onClosed();
                onClosed = null;
            }
        }


        public void OnClick(int action)
        {
            if (onActionClicked != null)
            {
                onActionClicked((UIRobotMenuAction)action);
                onActionClicked = null;
            }

            Close();
        }
    }
}
