using System;
using System.Collections;
using System.IO;

public class CodeSceneArgs
{
    public Project project { get; private set; }
    public bool isTempView { get; private set; }
    public string path { get; private set; }
    
    public CodeSession session { get; private set; }

    public Action<Project> tempViewCallBack;

    private CodeSceneArgs() { } 

    /// <summary>
    /// create args from project path
    /// </summary>
    public static CodeSceneArgs FromPath(string path)
    {
        if (path == null)
        {
            throw new ArgumentNullException("path");
        }

        return new CodeSceneArgs { path = path };
    }

    public static CodeSceneArgs FromSession(CodeSession session)
    {
        if (session == null)
        {
            throw new ArgumentNullException("session");
        }

        return new CodeSceneArgs { session = session };
    }

    public static CodeSceneArgs FromCode(Project code)
    {
        return new CodeSceneArgs { project = code, isTempView = false };
    }

    public static CodeSceneArgs FromTempCode(Project code)
    {
        return new CodeSceneArgs { project = code, isTempView = true };
    }

    public static CodeSceneArgs FromTempCodeCallBack(Project code, Action<Project> tempViewCallBack)
    {
        return new CodeSceneArgs { project = code, isTempView = true , tempViewCallBack  = tempViewCallBack };
    }
}

public class CodeSceneController : SceneController
{
    public UIWorkspace m_robotCodeWorkspace;
    public CodePanelManager m_robotCodePanelManager;
    public GuideController m_guideController;

    private CodeSceneArgs m_initialSceneArgs;

    public override void Init(object userData, bool isRestored)
    {
        base.Init(userData, isRestored);

        StartCoroutine(InitImpl(userData, isRestored));
    }

    // TODO: make Init a coroutine
    private IEnumerator InitImpl(object userData, bool isRestored)
    {
        // initialize before loading code
        m_guideController.Initialize();

        m_initialSceneArgs = (CodeSceneArgs)userData;
        yield return m_robotCodePanelManager.Init(m_initialSceneArgs, isRestored);

        if (m_guideController.enabled)
        {
            m_robotCodeWorkspace.UndoManager.stackEnabled = false;
        }
    }

    public override object OnSaveState()
    {
        CodeSceneArgs saveState = null;
        if (m_robotCodeWorkspace.ProjectName != string.Empty)
        {
            var path = m_robotCodeWorkspace.ProjectPath;
            saveState = CodeSceneArgs.FromPath(path);
        }
        else
        {
            if (m_initialSceneArgs != null && m_initialSceneArgs.isTempView)
            {
                saveState = m_initialSceneArgs;
            }
        }
        return saveState;
    }
}
