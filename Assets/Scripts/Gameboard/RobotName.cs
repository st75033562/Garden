using UnityEngine;
using UnityEngine.UI;

namespace Gameboard
{
    public class RobotName : MonoBehaviour
    {
        public Text m_nameText;
        public GameObject m_canvasRoot;

        public string robotName
        {
            get { return m_nameText.text; }
            set { m_nameText.text = value; }
        }

        public void Show(bool visible)
        {
            m_canvasRoot.SetActive(visible);
        }
    }
}
