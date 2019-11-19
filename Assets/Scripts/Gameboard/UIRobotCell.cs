using UnityEngine;
using UnityEngine.UI;

namespace Gameboard
{
    public class UIRobotCell : ScrollableCell
    {
        public UIRobotListView robotListView;
        public Image robotImage;
        public Image backgroundImage;
        public Image menuIndicator;
        public Text robotIndexText;
        public Image checkImage;
        public Color selectedColor = Color.white;

        public override void ConfigureCellData()
        {
            if (DataObject == null) { return; }

            bool selected = robotListView.selectedRobotIndex == robotIndex;

            backgroundImage.color = selected ? selectedColor : Color.white;
            backgroundImage.enabled = !robotListView.isMenuVisible;
            menuIndicator.enabled = !robotListView.isMenuVisible && !robotListView.uiGameboard.isRunning;
            checkImage.enabled = selected;
            robotIndexText.text = robotIndex.ToString();
            robotImage.sprite = robotListView.GetRobotImage(robotIndex);
        }

        public int robotIndex
        {
            get { return (int)DataObject; }
        }
    }
}
