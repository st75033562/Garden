using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace RobotSimulation
{
    public class Simulator : MonoBehaviour
    {
        public UnityEvent onStartLoading;
        public UnityEvent onFinishLoading;
        public UnityEvent onStartRunning;
        public UnityEvent onStopRunning;

        private World m_world;
        private bool m_renderingOn = true;

        private const string SettingsKey = "Simulator_Settings";

        private class Settings
        {
            public float floorSensorStdDeviation;
            public float proximitySensorStdDeviation;
            public float[] wheelBalanceValues;
        }

        void Awake()
        {
            floorSensorNoise = new NormalNoise();
            proximitySensorNoise = new NormalNoise();
            wheelBalanceValues = new float[1];

            LoadSettings();
        }

        public NormalNoise floorSensorNoise
        {
            get;
            private set;
        }

        public NormalNoise proximitySensorNoise
        {
            get;
            private set;
        }

        // TODO: temporary
        public float[] wheelBalanceValues
        {
            get;
            set;
        }

        /// <summary>
        /// the scene for simulation, no effect if running
        /// </summary>
        public string currentSceneName
        {
            get;
            private set;
        }

        public bool isRunning
        {
            get;
            private set;
        }

        public bool isLoading
        {
            get;
            private set;
        }

        public bool sceneLoaded
        {
            get;
            private set;
        }

        public IRobotManager robotManager
        {
            get { return m_world ? m_world.robotManager : null; }
        }

        public CameraManager cameraManager
        {
            get { return m_world ? m_world.cameraManager : null; }
        }

        public Coroutine LoadScene(string sceneName)
        {
            if (isRunning)
            {
                Debug.LogWarning("cannot load scene when running");
                return null;
            }

            if (isLoading)
            {
                Debug.LogWarning("already loading");
                return null;
            }

            Assert.IsFalse(string.IsNullOrEmpty(sceneName));

            return StartCoroutine(LoadSceneImpl(sceneName));
        }

        private IEnumerator LoadSceneImpl(string sceneName)
        {
            isLoading = true;
            if (onStartLoading != null)
            {
                onStartLoading.Invoke();
            }

            var oldScene = SceneManager.GetSceneByName(currentSceneName);
            currentSceneName = sceneName;
            yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

            CameraType oldCameraType = CameraType.Normal;
            if (m_world)
            {
                oldCameraType = cameraManager.cameraType;
            }
            if (oldScene.IsValid())
            {
#pragma warning disable 618
                // UnloadLevelAsync does not work when allowSceneActivation is false which is
                // necessary to avoid activating physics before unloading the old scene
                SceneManager.UnloadScene(oldScene);
#pragma warning restore
            }

            var currentScene = SceneManager.GetSceneByName(currentSceneName);

            // all objects are under the root object
#if UNITY_EDITOR
            Assert.IsTrue(currentScene.GetRootGameObjects().Length == 1, "a scene must have only one root");
#endif
            var worldRoot = currentScene.GetRootGameObjects()[0];
            m_world = worldRoot.GetComponent<World>();
            Assert.IsNotNull(m_world, "World not found");

            for (int i = 0; i < m_world.robotManager.robotCount; ++i)
            {
                var robot = m_world.robotManager.Get(i);
                robot.floorSensorNoise = floorSensorNoise;
                robot.proximitySensorNoise = proximitySensorNoise;
                robot.wheelBalanceValue = i < wheelBalanceValues.Length ? wheelBalanceValues[i] : 0;
            }

            cameraManager.renderingOn = renderingOn;

            if (m_world && sceneName == currentSceneName)
            {
                cameraManager.ActivateCamera(oldCameraType, true);
            }

            isLoading = false;
            sceneLoaded = true;
            if (onFinishLoading != null)
            {
                onFinishLoading.Invoke();
            }
        }

        public bool Run()
        {
            if (isRunning) { return false; }

            if (string.IsNullOrEmpty(currentSceneName))
            {
                Debug.LogWarning("scene not loaded");
                return false;
            }

            isRunning = true;
            if (onStartRunning != null)
            {
                onStartRunning.Invoke();
            }

            return true;
        }

        public void Stop()
        {
            if (!isRunning) { return; }

            isRunning = false;
            if (onStopRunning != null)
            {
                onStopRunning.Invoke();
            }
        }

        public bool renderingOn
        {
            get { return m_renderingOn; }
            set
            {
                m_renderingOn = value;
                if (cameraManager)
                {
                    cameraManager.renderingOn = value;
                }
            }
        }

        public void ActivateSceneObjects(bool active)
        {
            var scene = SceneManager.GetSceneByName(currentSceneName);
            if (scene.isLoaded)
            {
                foreach (var root in scene.GetRootGameObjects())
                {
                    root.SetActive(active);
                }
            }
        }

        // TODO: this is temporary
        public void LoadSettings()
        {
            var settingValue = PlayerPrefs.GetString(SettingsKey);
            if (!string.IsNullOrEmpty(settingValue))
            {
                try
                {
                    var setting = JsonMapper.ToObject<Settings>(settingValue);
                    floorSensorNoise.stdDeviation = setting.floorSensorStdDeviation;
                    proximitySensorNoise.stdDeviation = setting.proximitySensorStdDeviation;
                    wheelBalanceValues = setting.wheelBalanceValues;

                    // TODO:
                    if (wheelBalanceValues == null || wheelBalanceValues.Length == 0)
                    {
                        wheelBalanceValues = new float[1];
                    }
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }

        public void SaveSettings()
        {
            var setting = new Settings();
            setting.floorSensorStdDeviation = floorSensorNoise.stdDeviation;
            setting.proximitySensorStdDeviation = proximitySensorNoise.stdDeviation;
            setting.wheelBalanceValues = wheelBalanceValues;

            PlayerPrefs.SetString(SettingsKey, JsonMapper.ToJson(setting));
        }
    }
}
