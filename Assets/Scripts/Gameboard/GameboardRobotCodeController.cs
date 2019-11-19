using RobotSimulation;
using System;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace Gameboard
{
    class GameboardRobotCodeController : RobotCodeControllerBase
    {
        [SerializeField] Button m_submitButton;

        private RobotCodeManager m_codeManager;
        private bool m_needPromptGroupRemoval;
        private bool m_quitHandlerRegistered;
        private VisualScriptController m_scriptController;
        private IGameboardPlayer m_player;

        public void Init(RobotCodeManager codeManager, IGameboardPlayer player, VisualScriptController scriptController)
        {
            if (codeManager == null)
            {
                throw new ArgumentNullException("codeManager");
            }

            if (player == null)
            {
                throw new ArgumentNullException("player");
            }

            if (scriptController == null)
            {
                throw new ArgumentNullException("scriptController");
            }

            m_codeManager = codeManager;
            m_player = player;
            m_player.onStartRunning.AddListener(OnStartRunningGameboard);
            m_player.onStopRunning.AddListener(OnStopRunningGameboard);
            m_scriptController = scriptController;
            GetComponent<ControlButtons>().Init(player);
        }

        protected override void Start()
        {
            base.Start();

            CodeProjectRepository.instance.onProjectDeleted += OnProjectDeleted;

            m_Workspace.m_OnVisibleChanged.AddListener(OnWorkspaceVisibleChanged);
            OnWorkspaceVisibleChanged(m_Workspace.IsVisible);

            m_Workspace.OnSceneViewClicked += delegate { Exit(); };
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            m_player.onStartRunning.RemoveListener(OnStartRunningGameboard);
            m_player.onStopRunning.RemoveListener(OnStopRunningGameboard);
            m_Workspace.m_OnVisibleChanged.RemoveListener(OnWorkspaceVisibleChanged);
            ApplicationEvent.onQuit -= OnQuit;

            CodeProjectRepository.instance.onProjectDeleted -= OnProjectDeleted;
        }

        private void OnStartRunningGameboard()
        {
            m_submitButton.interactable = false;
        }

        private void OnStopRunningGameboard()
        {
            m_submitButton.interactable = true;
        }

        private void OnWorkspaceVisibleChanged(bool visible)
        {
            if (visible && !m_quitHandlerRegistered)
            {
                ApplicationEvent.onQuit += OnQuit;
                m_quitHandlerRegistered = true;
            }
            else if (!visible)
            {
                ApplicationEvent.onQuit -= OnQuit;
                m_quitHandlerRegistered = false;
            }
        }

        private void OnQuit(ApplicationQuitEvent evt)
        {
            m_saveController.SaveWithConfirm(status => {
                if (status == SaveCodeStatus.Saved)
                {
                    OnProjectSaved();
                }

                m_codeManager.CloseRobotCodingSpace(status != SaveCodeStatus.Discard);
                evt.Accept(); 
            }, evt.Ignore);
        }

        private int currentRobotIndex
        {
            get { return m_codeManager.activeRobotIndex; }
        }

        protected override void OnProjectNew()
        {
            var group = m_codeManager.GetOrCreateLocalGroup(m_Workspace.WorkingDirectory);
            m_codeManager.AddRobotToGroup(currentRobotIndex, group);
            group.isProjectDirty = false;
        }

        protected override void OnProjectSaved()
        {
            Assert.IsTrue(m_Workspace.ProjectName != string.Empty);

            var group = m_codeManager.AddRobotToGroup(currentRobotIndex, m_Workspace.ProjectPath);
            group.project = m_Workspace.GetProject();
            group.workingDirectory = m_Workspace.WorkingDirectory;
            group.isProjectDirty = false;
            StartCoroutine(group.Refresh());
        }

        protected override void LoadProject(IRepositoryPath path)
        {
            m_needPromptGroupRemoval = false;

            var group = m_codeManager.GetOrCreateLocalGroup(path.ToString());
            if (group != null)
            {
                m_codeManager.AddRobotToGroup(currentRobotIndex, group);
                StartCoroutine(group.Refresh());
            }
            else
            {
                PopupManager.Notice("ui_failed_to_load_project".Localize());
            }
        }

        protected override void OnAbortOpen()
        {
            if (m_needPromptGroupRemoval)
            {
                var dialog = UINoticeDialog.Ok("ui_dialog_notice".Localize(), 
                                               "ui_project_already_deleted".Localize(m_Workspace.ProjectName));
                dialog.onClosed = () => {
                    m_Workspace.New();
                    dialog.onClosed = null;
                };
                m_needPromptGroupRemoval = false;
            }
        }

        private void OnProjectDeleted(string path)
        {
            if (path == m_Workspace.ProjectPath)
            {
                m_needPromptGroupRemoval = true;
            }
        }

        public override void Exit()
        {
            m_saveController.SaveWithConfirm(status => {
                if (status == SaveCodeStatus.Saved)
                {
                    OnProjectSaved();
                }
                m_codeManager.CloseRobotCodingSpace(status != SaveCodeStatus.Discard);
                if (status != SaveCodeStatus.Unchanged && m_player.isRunning)
                {
                    m_player.Restart();
                }
            });
        }

        public void ShowSubmitButton(bool visible)
        {
            m_submitButton.gameObject.SetActive(visible);
        }

        public void OnClickSubmit()
        {
            if (!m_scriptController.IsUserCodeAssigned(currentRobotIndex) || m_Workspace.ProjectName == "")
            {
                m_saveController.SaveAs(() => OnSubmit(true));
            }
            else
            {
                m_saveController.Save(OnSubmit);
            }
        }

        void OnSubmit(bool changesSaved)
        {
            if (changesSaved)
            {
                OnProjectSaved();
            }
            m_player.RunAndSubmit();
        }
    }
}
