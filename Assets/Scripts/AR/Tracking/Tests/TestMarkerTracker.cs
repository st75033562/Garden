using OpenCVForUnitySample;
using UnityEngine;
using UnityEngine.UI;

namespace AR.Tests
{
    public class TestMarkerTracker : MonoBehaviour
    {
        public GameObject[] m_arGameObjects;

        public InputField m_processNoiseInput;
        public InputField m_measurementNoiseInput;
        public InputField m_errorCovPostInput;

        public Slider m_smoothFactorSlider;
        public Text m_smoothFactorText;

        public Toggle m_filterTypeKalmanToggle;
        public Toggle m_filterTypeExpToggle;

        public Toggle m_trackingModeCameraToggle;
        public Toggle m_trackingModeWorldCenterToggle;

        public MarkerTracker m_markerTracker;

        void Start()
        {
            m_markerTracker.OnTrackingUpdated += OnTrackingUpdated;

            m_processNoiseInput.text = m_markerTracker.PoseKalmanFilterParams.processNoise.ToString();
            m_measurementNoiseInput.text = m_markerTracker.PoseKalmanFilterParams.measurementNoise.ToString();
            m_errorCovPostInput.text = m_markerTracker.PoseKalmanFilterParams.errorCovPost.ToString();

            m_smoothFactorSlider.value = m_markerTracker.PoseExpSmoothFactor * 10;

            m_filterTypeKalmanToggle.isOn = m_markerTracker.PoseFilterType == TrackingFilterType.Kalman;
            m_filterTypeExpToggle.isOn = m_markerTracker.PoseFilterType == TrackingFilterType.Exponentail;

            m_trackingModeCameraToggle.isOn = m_markerTracker.TrackingMode == TrackingMode.FixedCamera;
            m_trackingModeWorldCenterToggle.isOn = m_markerTracker.TrackingMode == TrackingMode.WorldCenter;
        }

        void OnTrackingUpdated()
        {
            foreach (var marker in m_markerTracker.Markers)
            {
                if (marker.Id < m_arGameObjects.Length)
                {
                    var m = marker.WorldMatrix;
                    ARUtils.SetTransformFromMatrix(m_arGameObjects[marker.Id].transform, ref m);
                }
            }
        }

        public void OnProcessNoiseChanged(string value)
        {
            m_markerTracker.PoseKalmanFilterParams.processNoise = float.Parse(value);
        }

        public void OnMeasurementNoiseChanged(string value)
        {
            m_markerTracker.PoseKalmanFilterParams.measurementNoise = float.Parse(value);
        }

        public void OnErrorCovPostChanged(string value)
        {
            m_markerTracker.PoseKalmanFilterParams.errorCovPost = float.Parse(value);
        }

        public void OnSmoothFactorChanged(float value)
        {
            m_markerTracker.PoseExpSmoothFactor = value / 10;
            m_smoothFactorText.text = (value / 10).ToString();
        }

        public void OnFilterTypeKalmanToggled(bool isOn)
        {
            if (isOn)
            {
                m_markerTracker.PoseFilterType = TrackingFilterType.Kalman;
            }
        }

        public void OnFilterTypeExponentialToggled(bool isOn)
        {
            if (isOn)
            {
                m_markerTracker.PoseFilterType = TrackingFilterType.Exponentail;
            }
        }

        public void OnTrackingModeWorldCenterToggled(bool isOn)
        {
            if (isOn)
            {
                m_markerTracker.TrackingMode = TrackingMode.WorldCenter;
            }
        }

        public void OnTrackingModeCameraToggled(bool isOn)
        {
            if (isOn)
            {
                m_markerTracker.TrackingMode = TrackingMode.FixedCamera;
            }
        }

        public void ResetTracking()
        {
            m_markerTracker.ResetTrackingStates();
        }
    }
}
