using UnityEngine;
using UnityEngine.UI;

namespace RobotSimulation
{
    public class UIErrorSimulation : MonoBehaviour
    {
        public Simulator simulator;
        public UINoiseSliderController uiFloorSensor;
        public UINoiseSliderController uiProximitySensor;

        public Text balanceText;

        // TODO: read from a file
        private const int MinBalance = -10;
        private const int MaxBalance = 10;

        void Start()
        {
            uiFloorSensor.noise = simulator.floorSensorNoise;
            uiProximitySensor.noise = simulator.proximitySensorNoise;

            // TODO: show all balances settings
            OnChangeBalanceValue(0);
        }

        public void OnClose()
        {
            simulator.SaveSettings();
            gameObject.SetActive(false);
        }

        public void OnChangeBalanceValue(int delta)
        {
            simulator.wheelBalanceValues[0] = Mathf.Clamp(simulator.wheelBalanceValues[0] + delta, MinBalance, MaxBalance);
            balanceText.text = simulator.wheelBalanceValues[0].ToString();
        }
    }
}
