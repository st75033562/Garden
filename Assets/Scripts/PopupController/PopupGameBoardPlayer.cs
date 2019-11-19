using Gameboard;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProjectPath
{
    public ProjectPath(string path, bool isLocal)
    {
        if (string.IsNullOrEmpty(path))
        {
            throw new ArgumentException("path");
        }

        this.path = path;
        this.isLocal = isLocal;
    }

    public string path { get; private set; }

    public bool isLocal { get; private set; }

    public static ProjectPath Remote(string path)
    {
        return new ProjectPath(path, false);
    }

    public static ProjectPath Local(string path)
    {
        return new ProjectPath(path, true);
    }
}

public class RobotCodeInfo
{
    public string nickname { get; private set;}
    public ProjectPath path { get; private set; }

    public RobotCodeInfo(string nickname, ProjectPath path)
    {
        if (path == null)
        {
            throw new ArgumentException("path");
        }
        this.path = path;
        this.nickname = nickname;
    }

    public static RobotCodeInfo Remote(string path, string nickname)
    {
        return new RobotCodeInfo(nickname, ProjectPath.Remote(path));
    }

    public static RobotCodeInfo Local(string path, string nickname)
    {
        return new RobotCodeInfo(nickname, ProjectPath.Local(path));
    }
}

public class GameboardPlayerConfig
{
    public bool editable;
    public ProjectPath gameboardPath;
    // apply modification before passing to UIGameboard
    public Action<Gameboard.Gameboard> gameboardModifier;
    public SubmitHandler submitHandler;
    public SaveHandler saveHandler;
    public bool showPySubmit;
    public bool showEditButton;
    public bool isPreview;

    // 1 path for single player
    // 2 paths for PK
    public RobotCodeInfo[] robotCodes;

    public Gameboard.GameboardCustomCodeGroups customBindings;
    public Action<PopupILPeriod.PassModeType, Gameboard.GameboardResult> onResultSet;
    public List<string> relations;
    public bool isWebGb;
    public bool showBottomBts;
    public bool NoTopBarMode;
}

public class PopupGameBoardPlayer : PopupController
{
    // sub canvases that needs to adjust sorting order
    public Canvas[] subCanvases;
    public UIGameboard uiGameboardTemplate;

    public Text errorText;
    public GameObject retryButton;
    public GameObject retryTextObj;

    private GameboardProject m_gameboardProject;
    private string[] m_answerNames;
    private Gameboard.GameboardCustomCodeGroups m_robotCodeBindings;

    private ProjectDownloadRequest m_currentRequest;
    private int m_currentRobotCodeIndex;
    private UIGameboard m_uiGameboard;

    // Use this for initialization
    protected override void Start()
    {
        base.Start();

        m_uiGameboard = Instantiate(uiGameboardTemplate.gameObject).GetComponent<UIGameboard>();
        m_uiGameboard.onResultSet.AddListener(OnResultSet);
        SetBaseSortingOrder(BaseSortingOrder);

        StartCoroutine(Init());
    }

    IEnumerator Init()
    {
        HideError();

        m_uiGameboard.SetLanguage(Preference.scriptLanguage);

        m_uiGameboard.onClosing.AddListener(Close);
        m_uiGameboard.ConfigureUI(new GameboardUIConfig {
            showRobotList = config.editable,
            showInfoPanel = config.editable,
            showPySubmit = config.showPySubmit,
            showEditor = false,
            showEditButton = config.showEditButton,
            showSaveButton = config.saveHandler != null,
            isPreview = config.isPreview,
            showBottomBtns = config.showBottomBts,
            NoTopBarMode = config.NoTopBarMode
        });
        m_uiGameboard.SetSubmitHandler(config.submitHandler);
        m_uiGameboard.SetSaveHandler(config.saveHandler);
        yield return m_uiGameboard.Open();

        if (config.gameboardPath != null)
        {
            DownloadGameboard();
        }
        else
        {
            DebugPlay();
        }
    }

    private void OnResultSet()
    {
        if (config.onResultSet != null)
        {
            config.onResultSet(m_uiGameboard.passMode, m_uiGameboard.result);
        }
    }

    GameboardPlayerConfig config
    {
        get { return payload as GameboardPlayerConfig; }
    }

    private void DownloadGameboard()
    {
        if (!config.gameboardPath.isLocal)
        {
            var request = new ProjectDownloadRequest();
            request.preview = true;
            request.basePath = config.gameboardPath.path;
            request.defaultErrorHandling = false;
            request.Success(dir => {
                    m_gameboardProject = dir.ToGameboardProject();
                    OnGameboardDownloaded(m_gameboardProject);
                })
                .Error(() => {
                    ShowError("ui_gameboard_player_get_gameboard_failed", true);
                })
                .Execute();
        }
        else
        {
            m_gameboardProject = GameboardRepository.instance.loadGameboardProject(config.gameboardPath.path);
            OnGameboardDownloaded(m_gameboardProject);
        }
    }

    void OnGameboardDownloaded(GameboardProject gameboardProject)
    {
        m_gameboardProject = gameboardProject;

        if (m_gameboardProject == null)
        {
            ShowError("ui_gameboard_player_invalid_gameboard".Localize(), false);
            return;
        }

        if (config.robotCodes != null && config.robotCodes.Length > m_gameboardProject.gameboard.robots.Count)
        {
            PopupManager.Notice("ui_gameboard_player_invalid_robot_count".Localize(config.robotCodes.Length));
            Close();
            return;
        }

        if (config.gameboardModifier != null)
        {
            config.gameboardModifier(m_gameboardProject.gameboard);
            m_gameboardProject.gameboard.isDirty = false;
        }

        InitRobotCodeBindings();

        StartCoroutine(Play());
    }

    private void InitRobotCodeBindings()
    {
        if (config.robotCodes != null && config.robotCodes.Length > 0)
        {
            Debug.Assert(config.customBindings == null);

            var robotCodes = config.robotCodes;
            if (robotCodes.Length == 1)
            {
                // duplicate for each robot
                robotCodes = robotCodes.Repeat(m_gameboardProject.gameboard.robots.Count);
            }
            m_answerNames = new string[robotCodes.Length];

            m_robotCodeBindings = new GameboardCustomCodeGroups();
            var bindings = m_robotCodeBindings[ScriptLanguage.Visual];
            for (int i = 0; i < robotCodes.Length; ++i)
            {
                var robotCode = robotCodes[i];
                m_answerNames[i] = robotCode.nickname;
                if (robotCode.path.isLocal)
                {
                    bindings.SetRobotCode(i, robotCode.path.path);
                }
                else
                {
                    bindings.SetRobotCode(i, ProjectUrl.ToRemote(robotCode.path.path));
                }
            }
        }
    }

    private IEnumerator Play()
    {
        yield return m_uiGameboard.Open(m_gameboardProject, config.customBindings ?? m_robotCodeBindings, config.relations, config.isWebGb);
        if (m_answerNames != null)
        {
            m_uiGameboard.SetRobotNames(m_answerNames);
        }
        m_uiGameboard.Run();
    }

    private void DebugPlay()
    {
        m_gameboardProject = new GameboardProject();
        m_gameboardProject.gameboard = new Gameboard.Gameboard();
        m_gameboardProject.gameboard.themeId = 1;

        StartCoroutine(Play());
    }

    private void ShowError(string locId, bool showRetryButton)
    {
        errorText.enabled = true;
        errorText.text = locId.Localize();
        retryButton.SetActive(showRetryButton);
        retryTextObj.SetActive(showRetryButton);
    }

    private void HideError()
    {
        errorText.enabled = false;
        retryButton.SetActive(false);
        retryTextObj.SetActive(false);
    }

    public void OnClickRetry()
    {
        HideError();

        if (m_gameboardProject == null)
        {
            DownloadGameboard();
        }
    }

    protected override void DoClose()
    {
        if (m_currentRequest != null)
        {
            m_currentRequest.Abort();
            m_currentRequest = null;
        }
        StartCoroutine(StartClose());
    }

    private IEnumerator StartClose()
    {
        yield return m_uiGameboard.ResetGameboard();
        Destroy(m_uiGameboard.gameObject);

        base.DoClose();
    }

    protected override void SetBaseSortingOrder(int order)
    {
        if (m_uiGameboard)
        {
            m_uiGameboard.SetSortingOrder(order);
        }
        order += UIGameboard.numCanvases;
        for (int i = 0; i < subCanvases.Length; ++i)
        {
            subCanvases[i].sortingOrder = order + i;
        }
    }

    public override int SortingLayers
    {
        get { return subCanvases.Length + UIGameboard.numCanvases; }
    }
}
