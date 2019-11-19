using UnityEngine;
using UnityEngine.UI;

namespace Gameboard
{
    [RequireComponent(typeof(Toggle))]
    public class RobotColorButton : MonoBehaviour
    {
        public IntUnityEvent onColorSelected;
        public RobotColorSettings colorSettings;
        public Image colorImage;
        public Image checkImage;
        
        private int m_colorId;

        void Start()
        {
            var toggle = GetComponent<Toggle>();
            toggle.onValueChanged.AddListener(OnStateChanged);
            OnStateChanged(toggle.isOn);
        }

        private void OnStateChanged(bool isOn)
        {
            checkImage.enabled = isOn;
            if (isOn)
            {
                if (onColorSelected != null)
                {
                    onColorSelected.Invoke(colorId);
                }
            }
        }

        public int colorId
        {
            get { return m_colorId; }
            set
            {
                m_colorId = value;
                colorImage.color = colorSettings.GetColor(colorId);
            }
        }
    }
}
