using UnityEngine;
using UnityEngine.UI;

namespace RobotSimulation
{
    public class UINoiseSliderController : MonoBehaviour
    {
        public Slider slider;
        public Text stdDeviationText;

        private NormalNoise m_noise;

        public NormalNoise noise
        {
            get { return m_noise; }
            set
            {
                m_noise = value;
                UpdateUI();
            }
        }

        void Start()
        {
            UpdateUI();
        }

        void OnEnable()
        {
            UpdateUI();
        }

        public void OnStdDeviationSliderValueChanged(float value)
        {
            m_noise.stdDeviation = value;
            stdDeviationText.text = ((int)value).ToString();
        }

        public void OnChangeStdDeviation(int value)
        {
            slider.value += value;
        }

        void UpdateUI()
        {
            if (noise != null)
            {
                slider.value = noise.stdDeviation;
            }
        }

    }
}
