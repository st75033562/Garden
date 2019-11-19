using Gameboard;
using UnityEngine.Assertions;
using System;

public class CodeContext
{
    private IRobotManager m_robotManager;
    private readonly UIWorkspace m_workspace;

    public CodeContext(UIWorkspace workspace)
    {
        if (workspace == null)
        {
            throw new ArgumentNullException("workspace");
        }
        robotRuntime = new RobotRuntime();
        robotManager = new NullRobotManager();
        timer = new Timer();
        m_workspace = workspace;
        currentGlobalVarWriter = GlobalVarOwner.Robot;
        soundClipDataSource = new SoundClipDataSource();
    }

    // TODO: sync runtime state with robot manager by events ?
    public RobotRuntime robotRuntime { get; private set; }

    public IRobotManager robotManager
    {
        get { return m_robotManager; }
        set
        {
            m_robotManager = value;
            robotRuntime.SetStateCount(value.robotCount);
        }
    }

    public VariableManager variableManager { get; set; }

    public MessageManager messageManager { get; set; }

    public MessageHandler messageHandler { get; set; }

    public IWorldApi worldApi { get; set; }

    public IObjectResourceDataSource threeDObjectDataSource { get; set; }

    public IObjectResourceDataSource twoDObjectDataSource { get; set; }

    public float panelZoomFactor { get { return m_workspace.CurrentZoom; } }

    public EventBus eventBus { get; set; }

    public ScreenTextPanel textPanel { get; set; }

    public IGameboardService gameboardService { get; set; }

    public Timer timer { get; set; }

    public GlobalVarOwner currentGlobalVarWriter { get; set; }

    public ARSceneManager arSceneManager { get; set; }

    public BlockInput input { get; set; }

    public SoundClipDataSource soundClipDataSource { get; set; }

    public UnitySoundManager soundManager { get; set; }
}
