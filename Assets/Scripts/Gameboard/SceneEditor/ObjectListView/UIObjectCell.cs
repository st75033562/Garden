using UnityEngine;
using UnityEngine.UI;

namespace Gameboard
{
    public class UIObjectCell : ScrollableCell
    {
        public Text m_nameText;
        public GameObject m_optionButton;
        public Color m_highlightedColor;

        public override void ConfigureCellData()
        {
            m_nameText.text = data.name;
            m_nameText.color = IsSelected ? m_highlightedColor : Color.white;
            m_optionButton.SetActive(IsSelected);
        }

        public ObjectInfo data
        {
            get { return (ObjectInfo)dataObject; }
        }
    }
}
