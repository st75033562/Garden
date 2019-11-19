using RobotSimulation;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Gameboard
{
    public class UIRobotListView : MonoBehaviour
    {
        public ScrollableAreaController scrollController;
        public int maxVisibleRobots;

        public Button addRobotButton;

        public Editor editor;
        public UIObjectSettingsDialog objectSettingsDialog;

        public UIGameboard uiGameboard;
        public GameboardSceneManager gameboardSceneManager;

        public SelectionBox selectionHint;
        public RobotColorSettings robotColorSettings;
        public UIRobotMenu robotMenu;

        private Robot m_currentRobot;
        private RobotInfo m_currentRobotInfo;

        private bool m_isRobotEditable = true;
        private Gameboard m_gameboard;

        void Awake()
        {
            gameboardSceneManager.onBeginLoading.AddListener(OnBeginLoadingGameboard);
            gameboardSceneManager.onEndLoading.AddListener(OnEndLoadingGameboard);
            uiGameboard.onStartRunning.AddListener(Refresh);
            uiGameboard.onStopRunning.AddListener(Refresh);
        }

        void Start()
        {
            Refresh();
        }

        void OnBeginLoadingGameboard()
        {
            CancelSettings();
            addRobotButton.interactable = false;
        }

        void OnEndLoadingGameboard()
        {
            addRobotButton.interactable = true;
            scrollController.scrollPosition = 1.0f;
            Refresh();
        }

        public void SetGameboard(Gameboard gameboard)
        {
            if (gameboard == null)
            {
                throw new ArgumentNullException("gameboard");
            }

            if (m_gameboard != null)
            {
                m_gameboard.onRobotAdded -= OnRobotAdded;
                m_gameboard.onRobotRemoved -= OnRobotRemoved;
                m_gameboard.onRobotUpdated -= OnRobotUpdated;
            }

            m_gameboard = gameboard;
            m_gameboard.onRobotAdded += OnRobotAdded;
            m_gameboard.onRobotRemoved += OnRobotRemoved;
            m_gameboard.onRobotUpdated += OnRobotUpdated;

            Refresh();
        }

        private void OnRobotRemoved(int robotIndex)
        {
            Refresh();
        }

        private void OnRobotUpdated(int robotIndex)
        {
            Refresh();
        }

        private void OnRobotAdded(int robotIndex)
        {
            Refresh();
        }

        void Refresh()
        {
            var numRobots = m_gameboard != null ? m_gameboard.robots.Count : 0;
            UpdateScrollViewHeight(numRobots);
            scrollController.InitializeWithData(Enumerable.Range(0, numRobots).ToArray());
            UpdateAddButton();
        }

        private void UpdateScrollViewHeight(int robotCount)
        {
            var numVisibleRobots = Mathf.Min(robotCount, maxVisibleRobots);
            var scrollTrans = scrollController.GetComponent<RectTransform>();
            scrollTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,
                                                  numVisibleRobots * scrollController.cellHeight);
        }

        private void UpdateAddButton()
        {
            addRobotButton.gameObject.SetActive(isRobotEditable);
            addRobotButton.interactable = !uiGameboard.isRunning && m_gameboard != null;
        }

        public bool isRobotEditable
        {
            get { return m_isRobotEditable; }
            set
            {
                if (m_isRobotEditable != value)
                {
                    m_isRobotEditable = value;
                    UpdateAddButton();
                }
            }
        }

        public bool isEditing
        {
            get { return m_currentRobot != null; }
        }

        public bool isMenuVisible
        {
            get { return robotMenu.gameObject.activeSelf; }
        }

        public int selectedRobotIndex
        {
            get { return m_currentRobot ? m_currentRobot.robotIndex : -1; }
        }

        internal IScriptController scriptController
        {
            get { return uiGameboard.scriptController; }
        }

        public void OnClickRobot(UIRobotCell cell)
        {
            if (uiGameboard.isLoading ||
                uiGameboard.isRunning && !scriptController.IsCodeAssigned(cell.robotIndex) ||
                m_currentRobot != null)
            {
                return;
            }

            EditRobotCode(cell.robotIndex);
        }

        public Sprite GetRobotImage(int index)
        {
            var robots = m_gameboard.robots;
            if (index >= 0 && index < robots.Count)
            {
                return robotColorSettings.sprites[robots[index].colorId];
            }
            return null;
        }

        public void EditRobotSettings(UIRobotCell cell)
        {
            var curRobotIndex = selectedRobotIndex;
            CancelSettings();

            if (curRobotIndex != cell.robotIndex)
            {
                BeginEdit();

                SetSelectedRobot(gameboardSceneManager.robotManager.GetRobot(cell.robotIndex),
                                 gameboardSceneManager.gameboard.robots[cell.robotIndex]);

                OpenSettingsDialog();
            }
            else
            {
                objectSettingsDialog.Close();
            }

            Refresh();
        }

        private void OnRobotColorChanged()
        {
            Refresh();
        }

        private void OpenSettingsDialog()
        {
            objectSettingsDialog.Open();

            RobotPropertyViewModel viewModel;
            if (m_currentRobot.robotIndex != -1)
            {
                var undoChanges = new UndoRobotSettings(
                    uiGameboard.undoManager, gameboardSceneManager.objectManager, m_gameboard);
                viewModel = new RobotPropertyViewModel(m_currentRobot, m_currentRobotInfo, undoChanges);
            }
            else
            {
                var undoAdd = new UndoAddRobot(uiGameboard.undoManager, editor, gameboardSceneManager);
                viewModel = new RobotPropertyViewModel(m_currentRobot, m_currentRobotInfo, undoAdd);
            }

            objectSettingsDialog.SetViewModel(viewModel);
        }

        private void SetSelectedRobot(Robot robot, RobotInfo info)
        {
            if (robot != null)
            {
                selectionHint.gameObject.SetActive(true);
                selectionHint.Attach(robot.transform);
            }
            else
            {
                selectionHint.Detach();
                selectionHint.gameObject.SetActive(false);
            }

            m_currentRobot = robot;
            m_currentRobotInfo = info;
        }

        public void OnClickAdd()
        {
            BeginEdit();

            var newRobot = gameboardSceneManager.robotManager.CreateRobot();
            editor.SetupEntity(newRobot.GetComponent<Entity>(), true);

            var robotInfo = new RobotInfo();
            robotInfo.position = newRobot.transform.position;
            robotInfo.rotation = newRobot.transform.eulerAngles.y;
            robotInfo.scale = newRobot.transform.localScale;
            robotInfo.colorId = newRobot.GetComponent<RobotColor>().colorId;

            SetSelectedRobot(newRobot, robotInfo);
            OpenSettingsDialog();
        }

        private void BeginEdit()
        {
            scrollController.GetComponent<ScrollRect>().enabled = false;

            objectSettingsDialog.onOk.AddListener(OnConfirmSettings);
            objectSettingsDialog.onCancel.AddListener(OnCancelSettings);
            objectSettingsDialog.onColorChanged.AddListener(OnRobotColorChanged);
        }

        private void EndEdit()
        {
            scrollController.GetComponent<ScrollRect>().enabled = true;

            objectSettingsDialog.onOk.RemoveListener(OnConfirmSettings);
            objectSettingsDialog.onCancel.RemoveListener(OnCancelSettings);
            objectSettingsDialog.onColorChanged.RemoveListener(OnRobotColorChanged);
        }

        private void OnConfirmSettings()
        {
            EndEdit();

            if (selectedRobotIndex == -1)
            {
                scrollController.scrollPosition = 1.0f;
            }

            SetSelectedRobot(null, null);
        }

        private void OnCancelSettings()
        {
            CancelSettings();
            Refresh();
        }

        private void CancelSettings()
        {
            if (m_currentRobot)
            {
                EndEdit();

                objectSettingsDialog.Close();
                if (selectedRobotIndex == -1)
                {
                    Destroy(m_currentRobot.gameObject);
                }
                SetSelectedRobot(null, null);
            }
        }

        public void ShowRobotMenu(UIRobotCell cell)
        {
            // disable scrolling to ensure alignment
            scrollController.StopScrolling();

            var cellTrans = cell.GetComponent<RectTransform>();
            float screenY = cellTrans.TransformPoint(new Vector2(0, cellTrans.rect.yMax)).y;

            var robot = m_gameboard.robots[cell.robotIndex];
            robotMenu.Configure(robot.colorId,
                                cell.robotIndex,
                                scriptController.IsCodeAssigned(cell.robotIndex),
                                m_isRobotEditable);

            robotMenu.onActionClicked = action => OnClickMenu(cell, action);
            robotMenu.onClosed = Refresh;
            robotMenu.Open(screenY);

            Refresh();
        }

        private void OnClickMenu(UIRobotCell cell, UIRobotMenuAction action)
        {
            switch (action)
            {
            case UIRobotMenuAction.EditCode:
                EditRobotCode(cell.robotIndex);
                break;

            case UIRobotMenuAction.AssignCode:
                scriptController.AssignCode(cell.robotIndex, true);
                break;

            case UIRobotMenuAction.UnassignCode:
                scriptController.UnassignCode(cell.robotIndex);
                break;

            case UIRobotMenuAction.Settings:
                EditRobotSettings(cell);
                break;

            case UIRobotMenuAction.Delete:
                DeleteRobot(cell.robotIndex);
                break;

            default:
                throw new ArgumentException();
            }
        }
        
        private void DeleteRobot(int index)
        {
            uiGameboard.undoManager.AddUndo(new DeleteRobotCommand(editor, gameboardSceneManager, index));
        }

        void EditRobotCode(int robotIndex)
        {
            if (scriptController.IsCodeAssigned(robotIndex))
            {
                scriptController.EditCode(robotIndex);
            }
            else
            {
                scriptController.AssignCode(
                    robotIndex,
                    true,
                    () => scriptController.EditCode(robotIndex));
            }
        }
    }
}
