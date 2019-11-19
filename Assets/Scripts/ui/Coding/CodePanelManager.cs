using Gameboard;
using RobotSimulation;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using g_WebRequestManager = Singleton<WebRequestManager>;
using System.Collections.Generic;
using System.Collections;

public class CodePanelManager : RobotCodeControllerBase
{
    public Button m_RunButton;
    public Button m_StopButton;
    public UIPauseButton m_PauseButton;

    public Button m_SimulationButton;
    public SimulatorController m_UISimulator;
    public ARSceneManager m_ARSceneManager;

    private Action m_onExit;

    protected void Awake()
    {
        onExit = null;
    }

    protected override void Start()
    {
        base.Start();

        m_Workspace.CodeContext.arSceneManager = m_ARSceneManager;
        NetworkSessionController.instance.onBeforeLogout += OnBeforeLogout;

        SetPhysicalRobotManager();
        m_PauseButton.interactable = false;

        ApplicationEvent.onQuit += OnQuit;
    }

    void SetPhysicalRobotManager()
    {
        m_Workspace.CodeContext.robotManager = Robomation.RobotManager.instance;
    }

    public Action onExit
    {
        get { return m_onExit; }
        set { m_onExit = value ?? delegate { SceneDirector.Pop(); }; }
    }

    public IEnumerator Init(CodeSceneArgs args, bool isRestored)
    {
        Project project = null;
        bool isTempView = false;

        if (args != null)
        {
            if (args.session != null)
            {
                project = args.session.project;
                m_Workspace.WorkingDirectory = args.session.workingDirectory;
            }
            else if (!string.IsNullOrEmpty(args.path))
            {
                if (Path.GetFileName(args.path) != "")
                {
                    project = CodeProjectRepository.instance.loadCodeProject(args.path);
                    if (project == null)
                    {
                        UINoticeDialog.Ok(
                            "ui_dialog_notice".Localize(),
                            !isRestored ? "ui_failed_to_load_project".Localize()
                                        : "ui_project_already_deleted".Localize(args.path)
                        );
                    }
                }
                m_Workspace.WorkingDirectory = FileUtils.getDirectoryName(args.path);
            }
            else
            {
                project = args.project;
                isTempView = args.isTempView;
                tempViewCallBack = args.tempViewCallBack;
            }
        }

        yield return m_Workspace.Init(VoiceRepository.instance, NodeFilterData.GetFilter(NodeFilterType.Robot), false, null);
        yield return m_Workspace.Load(project);
        IsTempView = isTempView;

        if (args != null && args.session != null)
        {
            m_Workspace.IsChanged = true;
        }
    }

    public override void OnStartRunningCode()
    {
        base.OnStartRunningCode();

        m_Workspace.CodeContext.arSceneManager.RemoveObjects();
        m_RunButton.gameObject.SetActive(false);
        m_StopButton.gameObject.SetActive(true);

        if (UserManager.Instance.appRunModel == AppRunModel.Guide)
        {
            return;
        }

        m_PauseButton.interactable = true;
        UpdateModeButtons();
    }

    private void UpdateModeButtons()
    {
        m_SimulationButton.interactable = !m_Workspace.CodePanel.IsRunning;
    }

    public override void OnStopRunningCode()
    {
        if (UserManager.Instance.appRunModel != AppRunModel.Guide)
        {
            base.OnStopRunningCode();
        }

        m_RunButton.gameObject.SetActive(true);
        m_StopButton.gameObject.SetActive(false);
        Time.timeScale = 1.0f;

        if (UserManager.Instance.appRunModel == AppRunModel.Guide)
        {
            return;
        }

        m_PauseButton.interactable = false;
        m_PauseButton.isPaused = false;
        UpdateModeButtons();
    }

    protected override void ConfigOperationButton(UIOperationButtonConfig config)
    {
        if (IsTempView && config.button != UIOperationButton.Save)
        {
            config.interactable = false;
        }
    }

    protected override void LoadProject(IRepositoryPath path)
    {
        var code = CodeProjectRepository.instance.loadCodeProject(path.ToString());
        if (code != null)
        {
            m_Workspace.Id = path.ToString();
            m_Workspace.WorkingDirectory = path.parent.ToString();
            StartCoroutine(m_Workspace.Load(code));
        }
        else
        {
            PopupManager.Notice("ui_failed_to_load_project".Localize());
        }
    }

    void OnBeforeLogout()
    {
        if (m_Workspace.IsChanged)
        {
            var session = new CodeSession(m_Workspace.WorkingDirectory, m_Workspace.GetProject());
            try
            {
                CodeSession.Save(session);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        NetworkSessionController.instance.onBeforeLogout -= OnBeforeLogout;
        Time.timeScale = 1.0f;
        ApplicationEvent.onQuit -= OnQuit;
    }

    public UIWorkspace Workspace
    {
        get { return m_Workspace; }
    }

    public bool IsTempView
    {
        get;
        set;
    }

    public Action<Project> tempViewCallBack;

    protected void OnQuit(ApplicationQuitEvent quit)
    {
        if (m_saveController.isChanged)
        {
            Save(quit.Accept, quit.Ignore);
        }
        else
        {
            quit.Accept();
        }
    }

    public override void Exit()
    {
        if (UserManager.Instance.appRunModel == AppRunModel.Guide)
        {
            onExit();
        }
        else
        {
            Save(onExit, null);
        }
    }

    void Save(Action done, Action onCancel)
    {
        if (IsTempView)
        {
            if (UserManager.Instance.IsTeacher &&
                UserManager.Instance.CurTask != null &&
                m_Workspace.IsChanged) // TODO: need a separate dirty flag for comments
            {
                PopupManager.YesNo("ui_task_comments_save_hint".Localize(),
                    () => {
                        var request = new WebRequestBuilder()
                                        .Success(done)
                                        .Fail(onCancel)
                                        .BlockingRequest();

                        var project = Workspace.GetProject();
                        g_WebRequestManager.instance.UpLoadComment(
                              UserManager.Instance.CurTask.m_ID.ToString()
                            , UserManager.Instance.CurClass.m_ID.ToString()
                            , UserManager.Instance.CurSubmit.m_CommentID.ToString()
                            , project
                            , request);
                    },
                    done);
            }
            else
            {
                done();
                if (m_saveController.isChanged && tempViewCallBack != null)
                {
                    tempViewCallBack(Workspace.GetProject());
                    tempViewCallBack = null;
                }
            }
        }
        else
        {
            if(UserManager.Instance.IsStudent) {
                //学生在作业提交窗口上可以选择本地文件，进入这个分支
                UserManager.Instance.CurSubmit = null;
                UserManager.Instance.CurTask = null;
            }
           
            m_saveController.SaveWithConfirm(delegate { done(); }, onCancel);
        }
    }

    public void RunRobotCode()
    {
        SetPhysicalRobotManager();
        m_Workspace.Run();
    }

    public void StopRobotCode()
    {
        m_Workspace.Stop();
    }

    public void TogglePauseRobotCode()
    {
        bool newPaused = !m_Workspace.IsPaused;

        m_PauseButton.isPaused = newPaused;
        m_Workspace.SetPaused(newPaused);

        Time.timeScale = newPaused ? 0.0f : 1.0f;
    }

    public void ShowSimulator()
    {
        m_UISimulator.Show(true);
        m_Workspace.Show(false);
        m_ARSceneManager.ActivateSceneObjects(false);
    }

    public void OnClickGameboard()
    {
        m_saveController.SaveWithConfirm(delegate {
            if (!UserManager.Instance.IsGameboardUser)
            {
                PopupManager.ActivationCode(PopupActivation.Type.GameBoard);
                return;
            }
            SelectGameboard();
        });
    }

    private void SelectGameboard()
    {
        m_Workspace.Show(false);
        m_ARSceneManager.ActivateSceneObjects(false);

        GameboardUtils.SelectGameboard(
            (result) => {
                SceneDirector.Push("GameboardScene", new GameboardScenePayload(result.templateId, result.path));
            },
            () => {
                m_Workspace.Show(true);
            });
    }
}
