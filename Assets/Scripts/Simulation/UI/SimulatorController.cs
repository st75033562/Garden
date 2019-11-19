using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RobotSimulation
{
    public class SimulatorController : MonoBehaviour
    {
        public UnityEvent onHide;

        public Canvas canvas;
        public Button backButton;
        public Button loadButton;
        public GameObject runButtonObj;
        public GameObject stopButtonObj;
        public GameObject cameraButtonObj;
        public Simulator simulator;
        public UIWorkspace workspace;
        public GameObject sceneSelectionUIObj;
        public GameObject settingsUIObj;

        private bool m_runClicked;
        
        void Start()
        {
            UpdateButtonStates(false);
            Show(false);
            workspace.m_OnStopRunning.AddListener(OnStoppedRunningCode);
        }

        public bool visible
        {
            get { return gameObject.activeInHierarchy; }
        }

        public void Show(bool visible)
        {
            canvas.enabled = visible;

            // we need objects active when simulator is running
            if (!visible && !simulator.isRunning)
            {
                simulator.ActivateSceneObjects(false);
            }
            else if (visible)
            {
                simulator.ActivateSceneObjects(true);
            }
            // save some cycles when simulator is not visible
            simulator.renderingOn = visible;

            if (!visible && onHide != null)
            {
                onHide.Invoke();
            }
        }

        public void OnStoppedRunningCode()
        {
            simulator.Stop();

            if (!visible)
            {
                simulator.ActivateSceneObjects(false);
            }
            UpdateButtonStates(false);
        }

        private void UpdateButtonStates(bool running)
        {
            runButtonObj.SetActive(!running && simulator.sceneLoaded);
            stopButtonObj.SetActive(running);
            //cameraButtonObj.SetActive(simulator.sceneLoaded);
            loadButton.interactable = !running;
        }

        #region ui handlers

        public void OnClickRun()
        {
            StartCoroutine(RunImpl());
        }

        private IEnumerator RunImpl()
        {
            m_runClicked = true;
            runButtonObj.SetActive(false);

            // reset the scene to the original state
            // XXX: need a simple reset mechanism for fast resetting
            if (simulator.currentSceneName != null)
            {
                yield return simulator.LoadScene(simulator.currentSceneName);
            }

            yield return new WaitForSeconds(0.1f);

            backButton.interactable = true;

            if (simulator.Run())
            {
                workspace.CodeContext.robotManager = simulator.robotManager;
                workspace.Run();
                UpdateButtonStates(true);
            }
            m_runClicked = false;
        }

        public void OnStartLoading()
        {
            backButton.interactable = false;
            loadButton.interactable = false;
        }

        public void OnStopLoading()
        {
            if (!m_runClicked)
            {
                backButton.interactable = true;
                UpdateButtonStates(false);
            }
        }

        public void OnClickLoad()
        {
            sceneSelectionUIObj.gameObject.SetActive(true);
        }

        public void OnClickSwitchCamera()
        {
            var nextCameraType = simulator.cameraManager.cameraType + 1;
            if (nextCameraType == CameraType.Max)
            {
                nextCameraType = CameraType.Normal;
            }
            simulator.cameraManager.ActivateCamera(nextCameraType);
        }

        public void OnClickSettings()
        {
            settingsUIObj.SetActive(true);
        }

        public void OnClickMonitor()
        {
            var dialog = UIDialogManager.g_Instance.GetDialog<UIMonitorDialog>();
            dialog.Configure(simulator.robotManager, workspace.CodeContext.variableManager, Application.isMobilePlatform);
            dialog.OpenDialog();
        }

        #endregion ui handlers
    }
}
