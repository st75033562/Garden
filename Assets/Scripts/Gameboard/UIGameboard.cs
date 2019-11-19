using AR;
using DataAccess;
using OpenCVForUnitySample;
using RobotSimulation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

namespace Gameboard
{
    public class GameboardCustomCodeGroups
    {
        private readonly RobotCodeGroups[] m_groups;

        public GameboardCustomCodeGroups()
        {
            m_groups = new RobotCodeGroups[(int)ScriptLanguage.Num];
            for (int i = 0; i < m_groups.Length; ++i)
            {
                m_groups[i] = new RobotCodeGroups();
            }
        }

        public GameboardCustomCodeGroups(RobotCodeGroups[] groups)
        {
            if (groups == null)
            {
                throw new ArgumentNullException();
            }
            if (groups.Length < (int)ScriptLanguage.Num)
            {
                throw new ArgumentException("not enough groups");
            }
            if (groups.Any(x => x == null))
            {
                throw new ArgumentException("cannot have null group");
            }
            m_groups = groups;
        }

        public RobotCodeGroups this[ScriptLanguage lang]
        {
            get { return m_groups[(int)lang]; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException();
                }
                m_groups[(int)lang] = value;
            }
        }
    }

    public class GameboardUIConfig
    {
        public bool showEditButton;
        public bool showInfoPanel;
        public bool showRobotList;
        public bool enableRobotEditing;
        public bool showRecordingButton;
        public bool showRobotSwitchButton;
        public bool showEditor;
        public bool showSaveButton;
        public bool showPySubmit;
        public bool isPreview;
        public bool showBottomBtns;
        public bool NoTopBarMode;
    }

    public delegate void SubmitHandler(IRepositoryPath path,PopupILPeriod.PassModeType passMode, GameboardResult result);
    public delegate void SaveHandler();

    public class UIGameboard : MonoBehaviour, IGameboardService, IGameboardPlayer
    {
        public static UIGameboard instance;
        public UnityEvent onClosing;
        public UnityEvent onStartRunning;
        public UnityEvent onStopRunning;
        public BoolUnityEvent onPauseRunning;
        public UnityEvent onResultSet;
        public PopupILPeriod.PassModeType passMode;

        public GameboardSceneManager gameboardSceneManager;
        public RobotCodeManager codeManager;
        public UIWorkspace workspaceTemplate;
        public Canvas canvas;
        public Canvas uiCanvas;

        public Button editButton;
        public ButtonColorEffect cameraButton;
        public Button runButton;
        public Button stopButton;
        public UIPauseButton pauseButton;

        public UISwitchButton switchRobotButton;
        public GameObject arOptionButton;

        public UIGameboardInfoPanel infoPanel;
        public MouseWorldPosition mouseWorldPosition;
        public MouseScreenPosition mouseScreenPosition;
        public UIRobotListView robotListView;
        public RectTransform gameboardUIRoot;
        public GameObject topBarUI;
        public GameObject NoTopBarBackBtn;
        public GameObject openTopBarButton;

        // recording stuff
        public Button startRecordButton;
        public Button stopRecordButton;
        public UIRecordTimer recordTimer;
        public GameObject recordingUI;

        public Button saveButton;

        public GameObject loadingMask;
        public GameObject arRoot;
        public ARObjectManager arSceneManager;
        public MarkerTracker markerTracker;
        public ArWebCamTextureToMatHelper webCamTexHelper;

        public UIAssetListView assetListView;
        public Button addObjectButton;
        public EditorController editorController;
        public UIObjectListView objectListView;
        public Button openObjectListViewButton;

        public Camera overlayCamera;

        public GameObject errorPrompt;

        public Button undoButton;
        public Button redoButton;

        public Button btnSubmit;
        public List<PlayNetSound> netAudioSources = new List<PlayNetSound>();
        public GameObject[] bottomBtns;

        private bool m_initialized;
        private Project m_codeProject;
        private GameboardSaveController m_saveController;

        private Gameboard m_gameboard;

        private readonly BundleObjectDataSource m_3dObjectDataSource = new BundleObjectDataSource();
        private readonly BundleObjectDataSource m_2dObjectDataSource = new BundleObjectDataSource();

        private GameboardResult m_result;
        private readonly List<int> m_robotWithResult = new List<int>();
        private readonly List<string> m_robotNicknames = new List<string>();
        private IScriptController m_scriptController;
        private ScriptLanguage m_language = ScriptLanguage.Visual;
        private bool m_isTopbarVisible = true;

        private SubmitHandler m_submitHandler;
        private bool m_needSubmitResult;
        private GameboardUIConfig m_uiConfig = new GameboardUIConfig {
            showEditButton = true,
            showInfoPanel = true,
            showRobotList = false,
            enableRobotEditing = true,
            showRecordingButton = true,
            showRobotSwitchButton = true,
            showEditor = true,
            showSaveButton = true,
            isPreview = false,
            showBottomBtns = true
        };

        private SaveHandler m_saveHandler;

        public enum Mode
        {
            VirtualRobot, // virtual robot with 3d environment
            RealRobot, // real robot with 3d environment
            AR, // ar with real robot, no 3d environment
        }

        private Mode m_mode = Mode.VirtualRobot;
        private bool m_isPreparingRunning;

        private bool m_refreshAssetListView;
        private bool m_isObjectListVisible = true;
        private string m_workingDirectory = "";

        private bool m_needRestart;
        private bool m_retryClicked;

        private bool m_isChangingMode;
        private bool m_isRunning;
        private Vector2 m_loadingMaskOffsetMax;

        public static LobbyManager.eHideEditorUI _curHideEditWhenRun;

        /// <summary>
        /// virtualized scene view to be shared among workspaces
        /// Use render texture instead of hacking viewport rect?
        /// </summary>
        private class VirtualSceneView : ISceneView
        {
            private readonly UIGameboard m_uiGameboard;
            private bool m_enabled = true;
            private Rect m_sceneViewRect = new Rect(Vector2.zero, Vector2.one);
            private bool m_active;

            public VirtualSceneView(UIGameboard uiGameboard)
            {
                m_uiGameboard = uiGameboard;
            }

            public bool enabled
            {
                get { return m_enabled; }
                set
                {
                    m_enabled = value;
                    if (m_active)
                    {
                        m_uiGameboard.EnableSceneCameras(enabled);

                        if (enabled)
                        {
                            m_uiGameboard.StartTrackingInARMode();
                        }
                        else
                        {
                            m_uiGameboard.StopTracking();
                        }
                    }
                }
            }

            public void SetNormalizedRect(Rect rc)
            {
                m_sceneViewRect = rc;
                if (m_active)
                {
                    m_uiGameboard.SetSceneViewRect(rc);
                }
            }

            public void Activate(bool active)
            {
                m_active = active;
                if (active)
                {
                    enabled = enabled;
                    SetNormalizedRect(m_sceneViewRect);
                }
            }
        }

        void Awake()
        {
            instance = this;
            switchRobotButton.onValueChanging += OnModeChanging;
            switchRobotButton.onValueChanged.AddListener(OnModeChanged);

            m_saveController = new GameboardSaveController(this, () => isChanged);
            recordingUI.SetActive(false);
            UpdateTopBar();

            errorPrompt.SetActive(false);

            ApplicationEvent.onQuit += OnQuit;

            undoManager = new UndoManager();
            undoManager.onUndoEnabledChanged += OnUndoStateChanged;
            undoManager.onStackSizeChanged += OnUndoStateChanged;
            undoManager.onRunningChanged += OnUndoStateChanged;
            OnUndoStateChanged();

            new UndoContext(this);

            m_loadingMaskOffsetMax = loadingMask.GetComponent<RectTransform>().offsetMax;
        }

        void OnDestroy()
        {
            if (m_scriptController != null)
            {
                m_scriptController.Uninitialize();
            }
            if (codingSpace)
            {
                Destroy(codingSpace.gameObject);
            }
            Time.timeScale = 1.0f;
			ApplicationEvent.onQuit -= OnQuit;
        }

        private void OnUndoStateChanged()
        {
            undoButton.interactable = undoManager.UndoStackSize > 0 && !undoManager.isRunning;
            redoButton.interactable = undoManager.RedoStackSize > 0 && !undoManager.isRunning;
        }

        // show gameboard selection ui and open the choice. do nothing if user selects nothing
        public void SelectAndOpen(Action onSelected = null, Action onCancel = null)
        {
            GameboardUtils.SelectGameboard(
                (result) => {
                    if (result.templateId != 0)
                    {
                        New(result.templateId);
                        SetWorkingDirectory(result.path.ToString());
                    }
                    else
                    {
                        Open(result.path.ToString());
                    }
                    if (onSelected != null)
                    {
                        onSelected();
                    }
                }, onCancel);
        }

        // this must be called before calling Open
        public void ConfigureUI(GameboardUIConfig uiConfig)
        {
            if (uiConfig == null)
            {
                throw new ArgumentNullException("uiConfig");
            }

            if (isRecording || robotListView.isEditing)
            {
                Debug.LogError("cannot configure UI when recording or editing");
                return;
            }

            m_uiConfig = uiConfig;
            editorController.inputEnabled = m_uiConfig.showEditor;
            saveButton.gameObject.SetActive(uiConfig.showSaveButton);
        }

        public void SetLanguage(ScriptLanguage language)
        {
            if (m_initialized)
            {
                throw new InvalidOperationException();
            }
            m_language = language;
        }

        public void SetSubmitHandler(SubmitHandler handler)
        {
            m_submitHandler = handler;
        }

        public void SetSaveHandler(SaveHandler handler)
        {
            m_saveHandler = handler;
        }

        // open the gameboard without resetting the old states
        public Coroutine Open()
        {
            return StartCoroutine(OpenImpl(null, null, false, false, null));
        }

        // TODO: add an overload to open a local file by path
        public Coroutine New(int templateId)
        {
            var project = new GameboardProject();
            project.gameboard.themeId = templateId;
            return StartCoroutine(OpenImpl(project, null, true, false, null));
        }

        public Coroutine Open(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("path");
            }

            var project = GameboardRepository.instance.loadGameboardProject(path);
            if (project == null)
            {
                PopupManager.Notice("ui_gameboard_loading_code_failed".Localize());
                return null;
            }

            SetWorkingDirectory(Path.GetDirectoryName(path));
            return StartCoroutine(OpenImpl(project, null, false, true, null));
        }

        // open a new gameboard with given settings
        public Coroutine Open(GameboardProject project, GameboardCustomCodeGroups customBindings = null, List<string> relations = null, bool isWebGb = false)
        {
            if (project == null)
            {
                throw new ArgumentException("gameboard cannot be null");
            }
            return StartCoroutine(OpenImpl(project, customBindings, false, false, relations, isWebGb));
        }

        private IEnumerator OpenImpl(
            GameboardProject project, 
            GameboardCustomCodeGroups customBindings, 
            bool isNew, 
            bool isLocal,
            List<string> relations,
            bool isWebGb = false)
        {
            canvas.enabled = true;

            ShowLoadingMask(true);

            UpdateUI();

            if (!m_initialized)
            {
                yield return Initialize();
                m_initialized = true;
            }

            gameboardSceneManager.renderingOn = true;
            gameboardSceneManager.ActivateSceneObjects(true);

            if (project != null)
            {
                undoManager.Reset();

                editorController.SetGameboard(project.gameboard);
                editorController.StartEditing();

                m_refreshAssetListView = true;
                var template = GameboardTemplateData.Get(project.gameboard.themeId);
                m_3dObjectDataSource.Initialize(template.objectsBundle, GameboardThemeBundleData.Global3DBundle);
                m_2dObjectDataSource.Initialize(template.objects2dBundle, GameboardThemeBundleData.Global2DBundle);
                if (template.soundBundle != null)
                {
                    codingSpace.CodeContext.soundClipDataSource.AddClips(
                        template.soundBundle.Select(x => new SoundClipData(x)));
                }

                yield return CacheObjects(project.gameboard);
                m_gameboard = project.gameboard;

                RemoveInvalidObjects(project.gameboard);
               
                yield return gameboardSceneManager.Load(m_gameboard, isNew);

                objectListView.SetGameboard(project.gameboard);

                m_codeProject = project;
                yield return codingSpace.Load(m_codeProject);
                // no dialog to enable the sound, force enabled
                codingSpace.SoundEnabled = true;

                // initializing scripting controller is cancellable
                ShowLoadingMask(false);
                yield return InitializeScriptController(isLocal, customBindings, relations, isWebGb);

                m_robotNicknames.Clear();

                robotListView.SetGameboard(project.gameboard);
            }

            CloseLoadingMask();

            UpdateUI();
        }

        private void RemoveInvalidObjects(Gameboard gameboard)
        {
            var invalidObjects = gameboard.objects.Where(x => BundleAssetData.Get(x.assetId) == null).ToList();
            foreach (var invalidObj in invalidObjects)
            {
                gameboard.RemoveObject(invalidObj);
            }
            if (invalidObjects.Count > 0)
            {
                undoManager.SetClean(false);
            }
        }

        private IEnumerator Initialize()
        {
            arRoot.SetActive(false);
            yield return gameboardSceneManager.Initialize();
            gameboardSceneManager.objectFactory.inEditor = true;
            arSceneManager.objectManager = gameboardSceneManager.objectManager;

            codingSpace = Instantiate(workspaceTemplate.gameObject).GetComponent<UIWorkspace>();

            codingSpace.name = "Gameboard Workspace";
            codingSpace.WorkingDirectory = m_workingDirectory;
            var controller = codingSpace.gameObject.GetComponent<GameboardCodingController>();
            controller.uiGameboard = this;

            codingSpace.Show(false);
            codingSpace.BlockLevel = BlockLevel.Advanced;
            yield return codingSpace.Init(VoiceRepository.instance,
                             NodeFilterData.GetFilter(NodeFilterType.Gameboard),
                             true,
                             new VirtualSceneView(this));

            codingSpace.CodeContext.textPanel = GetComponent<ScreenTextPanel>();
            codingSpace.CodeContext.worldApi = new WorldApi(gameboardSceneManager);
            codingSpace.CodeContext.threeDObjectDataSource = m_3dObjectDataSource;
            codingSpace.CodeContext.twoDObjectDataSource = m_2dObjectDataSource;
            codingSpace.CodeContext.gameboardService = this;
            codingSpace.CodeContext.currentGlobalVarWriter = GlobalVarOwner.Gameboard;
            codingSpace.CodeContext.input.enabled = false;
            codingSpace.CodeContext.arSceneManager = arSceneManager;
            codingSpace.UndoManager = undoManager;
            codingSpace.ResetUndoOnLoad = false;

            SetRobotManager(gameboardSceneManager.robotManager);
            switchRobotButton.option = (int)Mode.VirtualRobot;

            if (m_language == ScriptLanguage.Visual)
            {
                m_scriptController = new VisualScriptController(this);

                codeManager.Initialize(canvas.sortingOrder + 1,
                                       CodeProjectRepository.instance,
                                       codingSpace,
                                       arSceneManager,
                                       this,
                                       () => new VirtualSceneView(this),
                                       (VisualScriptController)m_scriptController,
                                       m_submitHandler != null);
                codeManager.onWorkspaceOpened += OnWorkspaceOpened;
                codeManager.onWorkspaceClosed += OnWorkspaceClosed;
            }
            else
            {
                var pythonController = gameObject.AddComponent<PythonScriptController>();
                pythonController.Initialize(this);
                m_scriptController = pythonController;
            }

            editorController.Initialize(codingSpace, undoManager);
            yield return null;

            objectListView.Initialize(gameboardSceneManager.objectManager);
            yield return null;
        }

        public void OnClickSubmitPy() {
            int robotIndex = 0;
            if(m_scriptController.IsCodeAssigned(robotIndex) && m_scriptController.GetRobotCodePath(0) != PythonScriptController.absPath) {
                RunAndSubmit();
            } else {
                m_scriptController.AssignCode(robotIndex, false, () => {
                    RunAndSubmit();
                });
            }
        }

        private IEnumerator InitializeScriptController(
            bool isLocal,
            GameboardCustomCodeGroups customBindings,
            List<string> relations,
            bool isWebGb = false)
        {
            m_scriptController.SetGameboard(m_gameboard);

            bool isDone = false;
            bool showError = false;

            // HACK: remove remote urls
            if (isLocal)
            {
                RemoveRemoteUrls(m_gameboard.GetCodeGroups(m_language));
            }

            // if custom binding is provided, we use groups from gameboard as a fallback
            var userGroups = customBindings != null ? customBindings[m_language] : null;
            RobotCodeGroups defaultGroups = null;
            if (userGroups == null)
            {
                userGroups = m_gameboard.GetCodeGroups(m_language);
                if(isWebGb) {
                    defaultGroups = m_gameboard.GetCodeGroups(m_language);
                }
            }
            else
            {
                defaultGroups = m_gameboard.GetCodeGroups(m_language);
            }

            while (!isDone)
            {
                yield return CoroutineUtils.Async(done => {
                    m_scriptController.InitCodeBindings(userGroups, defaultGroups,
                        () => {
                            isDone = true;
                            done();
                        },
                        () => {
                            showError = true;
                            done();
                        }, relations);
                });

                if (showError)
                {
                    showError = false;
                    errorPrompt.SetActive(true);

                    while (!m_retryClicked)
                    {
                        yield return null;
                    }

                    errorPrompt.SetActive(false);
                    m_retryClicked = false;
                }
            }
        }

        private void RemoveRemoteUrls(RobotCodeGroups groups)
        {
            var remoteGroups = groups.codeGroups
                                    .Where(x => ProjectUrl.IsRemote(x.projectPath))
                                    .ToList();
            foreach (var group in remoteGroups)
            {
                groups.RemoveCodeGroup(group);
            }
        }

        private IEnumerator CacheObjects(Gameboard gameboard)
        {
            var newTheme = m_gameboard == null || m_gameboard.themeId != gameboard.themeId;
            if (newTheme)
            {
                if (m_gameboard != null)
                {
                    var curTemplate = GameboardTemplateData.Get(m_gameboard.themeId);
                    gameboardSceneManager.objectFactory.Evict(curTemplate.objectsBundle);
                }

                gameboardSceneManager.objectFactory.Cache(m_3dObjectDataSource.objectResources);

                yield return CoroutineUtils.WaitUntil(() => gameboardSceneManager.objectFactory.isCacheReady);
            }
        }

        private void SetRobotManager(IRobotManager robotManager)
        {
            codingSpace.CodeContext.robotManager = robotManager;
            codeManager.SetRobotManager(robotManager);
        }

        public IEnumerator ResetGameboard()
        {
            yield return gameboardSceneManager.Reset();
            UpdateUI();
            errorPrompt.SetActive(false);
        }

        public IRobotManager robotManager
        {
            get { return codingSpace.CodeContext.robotManager; }
        }
        
        public bool isRunning
        {
            get {
                return m_isRunning;
            }
            private set {
                m_isRunning = value;
                btnSubmit.interactable = !value;
            }
        }

        public bool isLoading
        {
            get { return gameboardSceneManager.isLoading; }
        }

        private void UpdateUI()
        {
            robotListView.isRobotEditable = m_uiConfig.enableRobotEditing && m_mode != Mode.AR;
            robotListView.gameObject.SetActive(m_uiConfig.showRobotList && !isRecording);

            infoPanel.gameObject.SetActive(m_uiConfig.showInfoPanel && !isRunning);

            bool objectListVisible = m_uiConfig.showEditor && !isRunning  && !isRecording && m_mode != Mode.AR;
            addObjectButton.gameObject.SetActive(objectListVisible);
            addObjectButton.interactable = m_initialized;

            objectListView.gameObject.SetActive(objectListVisible && m_isObjectListVisible);
            openObjectListViewButton.gameObject.SetActive(objectListVisible && !m_isObjectListVisible);

            editButton.gameObject.SetActive(m_uiConfig.showEditButton && !isRecording);

            
            
            btnSubmit.gameObject.SetActive(m_language == ScriptLanguage.Python && m_uiConfig.showPySubmit);

            editButton.interactable = m_gameboard != null &&
                                      m_gameboard.sourceCodeAvailable;

            runButton.gameObject.SetActive(!isRunning);
            runButton.interactable = m_gameboard != null &&
                                     !gameboardSceneManager.isLoading &&
                                     !isRunning;

            pauseButton.interactable = isRunning;

            cameraButton.gameObject.SetActive(m_mode != Mode.AR);
            cameraButton.interactable = gameboardSceneManager.cameraManager != null;

            if (m_uiConfig.showBottomBtns)
            {
                stopButton.gameObject.SetActive(isRunning);
            }
            else
            {
                foreach (var go in bottomBtns)
                {
                    go.SetActive(m_uiConfig.showBottomBtns);
                }
            }

            startRecordButton.gameObject.SetActive(m_uiConfig.showRecordingButton && VideoRecorder.isAvailable && !isRecording);
            startRecordButton.interactable = !isLoading;

            switchRobotButton.gameObject.SetActive(m_uiConfig.showRobotSwitchButton && !isRecording);
            switchRobotButton.interactable = !isRunning && m_initialized;

            UpdateTopBar();
        }

        public void OpenCodingSpace()
        {
            // #TODO need change if we allow scripting gameboard
            if (codeManager.activeRobotIndex != -1)
            {
                throw new InvalidOperationException();
            }

            codingSpace.Show(true);
            OnWorkspaceOpened(codingSpace);
        }

        public void CloseCodingSpace()
        {
            if (!codingSpace.IsVisible)
            {
                return;
            }

            codingSpace.Show(false);
            OnWorkspaceClosed(codingSpace);
        }

        public bool isCodingSpaceVisible
        {
            get { return codingSpace.IsVisible; }
        }

        internal void OnGameboardSaved()
        {
            // save the changed project
            m_codeProject = codingSpace.GetProject();
            undoManager.SetClean(true);
            m_gameboard.isDirty = false;
        }

        public void SetRobotNames(string[] names)
        {
            if (names == null)
            {
                throw new ArgumentNullException("names");
            }

            m_robotNicknames.Clear();
            m_robotNicknames.AddRange(names);
            InitRobotNames();
        }

        public GameboardResult result
        {
            get { return m_result; }
        }

        public void OnClickCamera()
        {
            if (!gameboardSceneManager.cameraManager) { return; }

            var nextCameraType = gameboardSceneManager.cameraManager.cameraType + 1;
            if (nextCameraType == RobotSimulation.CameraType.Max)
            {
                nextCameraType = RobotSimulation.CameraType.Normal;
            }
            gameboardSceneManager.cameraManager.ActivateCamera(nextCameraType);
        }

        public void OnClickNew()
        {
            m_saveController.SaveWithConfirm(delegate {
                // disable the gameboard since user can create a new gameboard from the popup
                // which will conflict with us
                EnableGameboard(false);
                SelectAndOpen(onCancel: () => {
                    EnableGameboard(true);
                });
            });
        }

        private void EnableGameboard(bool enabled)
        {
            canvas.enabled = enabled;
            gameboardSceneManager.ActivateSceneObjects(enabled);
        }

        public void OnClickEdit()
        {
            OpenCodingSpace();
        }

        public void OnClickReturn()
        {
            if(m_uiConfig.isPreview) {
                if(onClosing != null) {
                    onClosing.Invoke();
                }
                return;
            }
            m_saveController.SaveWithConfirm(delegate {
                if (onClosing != null)
                {
                    onClosing.Invoke();
                }
            });
        }

        public Gameboard GetGameboard()
        {
            return m_gameboard;
        }

        public GameboardProject GetGameboardProject()
        {
            var project = new GameboardProject();
            project.gameboard = m_gameboard;
            var code = codingSpace.GetProject();
            project.code = code.code;
            project.leaveMessageData = code.leaveMessageData;
            return project;
        }

        public bool isChanged
        {
            get { return !undoManager.IsClean(); }
        }

        private void OnWorkspaceOpened(UIWorkspace workspace)
        {
            canvas.enabled = false;
            mouseWorldPosition.enabled = mouseScreenPosition.enabled = false;

            var sceneView = (VirtualSceneView)workspace.sceneView;
            sceneView.Activate(true);
        }

        private void OnWorkspaceClosed(UIWorkspace workspace)
        {
            canvas.enabled = true;
            mouseWorldPosition.enabled = mouseScreenPosition.enabled = true;

            var sceneView = (VirtualSceneView)workspace.sceneView;
            sceneView.Activate(false);

            ResetSceneView();
            StartTrackingInARMode();
        }

        public void Restart()
        {
            if (isRunning || m_isPreparingRunning)
            {
                m_needRestart = true;
                Stop();
            }
        }

        private List<GameObject> hideObjListWhenRun = new List<GameObject>();
        void PrepareRun()
        {
            hideObjListWhenRun.Clear();
            hideObjListWhenRun.Add(switchRobotButton.gameObject);
            if(UIGameboard._curHideEditWhenRun == LobbyManager.eHideEditorUI.NO_HIDE)
            {
                //DoNothing
            }
            else if(UIGameboard._curHideEditWhenRun == LobbyManager.eHideEditorUI.YES_HIDE)
            {
                hideObjListWhenRun.Add(editButton.gameObject);
                hideObjListWhenRun.Add(robotListView.gameObject);
            }

            foreach (GameObject o in hideObjListWhenRun)
            {
                o.SetActive(false);
            }
            CloseTopBar();
        }
        void ReleaseHideObj()
        {
            OpenTopBar();
            foreach(GameObject o in hideObjListWhenRun)
            {
                o.SetActive(true);
            }
            hideObjListWhenRun.Clear();
        }

        public void Run()
        {
            if (isRunning || m_isPreparingRunning)
            {
                return;
            }

            StartCoroutine(RunImpl());
        }

        private IEnumerator RunImpl()
        {
            undoManager.undoEnabled = false;

            m_isPreparingRunning = true;

            editorController.StopEditing();
            objectListView.editable = false;

            arSceneManager.RemoveObjects();
            gameboardSceneManager.objectFactory.inEditor = false;

            yield return gameboardSceneManager.ResetObjects(false);

            InitRobotNames();

            yield return m_scriptController.PrepareRunning();

            gameboardSceneManager.objectManager.ActivateAll();
            
            // we do not own the robots, so we should not reset the robot
            codingSpace.CodePanel.ResetRobotsOnRun = false;
            codingSpace.Run(true, true);
            InitializeObjectVariables();

            m_result = new GameboardResult(codingSpace.CodeContext.robotManager.robotCount);
            m_robotWithResult.Clear();

            isRunning = true;
            m_isPreparingRunning = false;

            UpdateUI();

            if (onStartRunning != null)
            {
                onStartRunning.Invoke();
            }
            PrepareRun();
        }

        private void InitializeObjectVariables()
        {
            if (m_mode == Mode.AR)
            {
                return;
            }

            foreach (var objInfo in m_gameboard.objects)
            {
                var entity = gameboardSceneManager.objectManager.Get(objInfo.name);
                // in case the code was discarded, corresponding variables will not be found
                var data = (VariableData)codingSpace.CodeContext.variableManager.get(objInfo.name);
                if (data != null)
                {
                    data.setValue(entity.id);
                }
            }
        }

        private void InitRobotNames()
        {
            if (m_robotNicknames.Count >= 2)
            {
                for (int i = 0; i < gameboardSceneManager.robotManager.robotCount && i < m_robotNicknames.Count; ++i)
                {
                    var robot = gameboardSceneManager.robotManager.GetRobot(i);
                    var robotName = robot.GetComponent<RobotName>();
                    robotName.robotName = m_robotNicknames[i];
                    robotName.Show(true);
                }
            }
            else
            {
                foreach (var robot in gameboardSceneManager.robotManager)
                {
                    robot.GetComponent<RobotName>().Show(false);
                }
            }
        }

        public void Stop()
        {
            if (!isRunning)
            {
                return;
            }
            foreach (var netSound in netAudioSources) {
                netSound.StopSound();
            }
            netAudioSources.Clear();
            StopAllCoroutines();
            StartCoroutine(StopImpl());
            OpenTopBar();
        }

        private IEnumerator StopImpl()
        {
            codingSpace.Stop();
            codingSpace.CodeContext.textPanel.Clear();

            m_scriptController.Stop();

            arSceneManager.RemoveObjects();
            // all objects should be removed before editing
            gameboardSceneManager.RemoveObjects();

            editorController.StartEditing();
            editorController.inputEnabled = m_uiConfig.showEditor;
            objectListView.editable = m_uiConfig.showEditor;

            gameboardSceneManager.objectFactory.inEditor = true;
            yield return gameboardSceneManager.ResetObjects(true);

            isRunning = false;
            m_needSubmitResult = false;

            pauseButton.isPaused = false;
            Time.timeScale = 1.0f;

            UpdateUI();

            undoManager.undoEnabled = true;

            if (onStopRunning != null)
            {
                onStopRunning.Invoke();
            }

            if (m_needRestart)
            {
                m_needRestart = false;
                Run();
            }
        }

        public void TogglePause()
        {
            Pause(!codingSpace.IsPaused);
        }

        public void StartRecording()
        {
            if (isRecording) { return; }

            VideoRecorder.StartRecording();
            DoRecording(true);
            recordTimer.Begin();
            stopRecordButton.interactable = false;
            StartCoroutine(EnableStopRecordingButton());
        }

        private IEnumerator EnableStopRecordingButton()
        {
            yield return new WaitForSecondsRealtime(VideoRecorder.minimumRecordingSeconds);
            stopRecordButton.interactable = true;
        }

        public void StopRecording()
        {
            if (!isRecording) { return; }

            int popupId = PopupManager.ShowMask("ui_video_muxing_hint".Localize());
            VideoRecorder.StopRecording(() => {
                PopupManager.Close(popupId);
                OnStopRecording();
            });

            recordTimer.End();
        }

        private void OnStopRecording()
        {
            stopRecordButton.interactable = true;
            DoRecording(false);

            if (VideoRecorder.lastVideoPath != null)
            {
                PopupManager.VideoPreview(SharedVideo.LocalFile(VideoRecorder.lastVideoPath));
            }
            else
            {
                PopupManager.Notice("ui_video_recording_failed".Localize());
            }
        }

        private void DoRecording(bool start)
        {
            isRecording = start;
            stopRecordButton.gameObject.SetActive(start);
            recordingUI.SetActive(start);

            undoManager.undoEnabled = !start && !isRunning && !m_isPreparingRunning;

            UpdateUI();
        }

        public void OpenGameboardCodeMonitor()
        {
            var dialog = UIDialogManager.g_Instance.GetDialog<UIMonitorDialog>();
            dialog.Configure(codingSpace.CodeContext.robotManager,
                             codingSpace.CodeContext.variableManager,
                             Application.isMobilePlatform);
            dialog.OpenDialog();
        }

        public void OpenTopBar()
        {
            if (isRecording) { return; }

            m_isTopbarVisible = true;
            UpdateTopBar();
        }

        public void CloseTopBar()
        {
            if (isRecording) { return; }

            m_isTopbarVisible = false;
            UpdateTopBar();
        }

        private void UpdateTopBar()
        {
            if (m_uiConfig.NoTopBarMode)
            {
                NoTopBarBackBtn.SetActive(true);
                topBarUI.SetActive(false);
                openTopBarButton.SetActive(false);
            }
            else
            {
                NoTopBarBackBtn.SetActive(false);
                topBarUI.SetActive(!isRecording && m_isTopbarVisible);
                openTopBarButton.SetActive(!isRecording && !m_isTopbarVisible);
            }
        }

        public void OpenAssetListView()
        {
            assetListView.Show(true);

            if (m_refreshAssetListView)
            {
                m_refreshAssetListView = false;
                assetListView.Initialize(m_3dObjectDataSource.objectResources);
            }
        }

        public void OpenObjectListView()
        {
            objectListView.Open();
            m_isObjectListVisible = true;
            openObjectListViewButton.gameObject.SetActive(false);
        }

        public void CloseObjectListView()
        {
            objectListView.Close();
            m_isObjectListVisible = false;
            openObjectListViewButton.gameObject.SetActive(true);
        }

        public bool isRecording
        {
            get;
            private set;
        }

        private bool OnModeChanging(int mode)
        {
            if ((Mode)mode == Mode.AR && !UserManager.Instance.IsArUser)
            {
                PopupManager.ActivationCode(PopupActivation.Type.AR);
                return false;
            }
            return true;
        }

        private void OnModeChanged(int mode)
        {
            var newMode = (Mode)mode;
            // guard against recursive update in case the callback is triggered
            // by calling ChangeMode programmatically, e.g. by undo.
            if (newMode != m_mode)
            {
                undoManager.AddUndo(new SwitchModeCommand(this, (Mode)mode));
            }
        }

        public IEnumerator ChangeMode(Mode mode)
        {
            if (m_mode == mode)
            {
                yield break;
            }

            yield return ChangeModeImpl(mode);
        }

        private IEnumerator ChangeModeImpl(Mode mode)
        {
            if (m_isChangingMode)
            {
                Debug.LogWarning("changing mode already in progress");
                yield break;
            }

            m_isChangingMode = true;
            loadingMask.SetActive(true);

            var oldMode = m_mode;
            m_mode = mode;

            // update after mode is updated to avoid recursion
            switchRobotButton.option = (int)mode;

            bool isARMode = mode == Mode.AR;
            if (!isARMode)
            {
                if (mode == Mode.VirtualRobot)
                {
                    SetRobotManager(gameboardSceneManager.robotManager);
                }
                else
                {
                    SetRobotManager(Robomation.RobotManager.instance);
                }

                arRoot.SetActive(false);
                webCamTexHelper.Stop();
                arSceneManager.RemoveObjects();

                editorController.StartEditing();
                // scene is not loaded in AR mode, reload it
                if (oldMode == Mode.AR)
                {
                    yield return gameboardSceneManager.Load(m_gameboard, false);
                }
            }
            else
            {
                arSceneManager.RemoveObjects();
                editorController.StopEditing();
                yield return gameboardSceneManager.InitARMode();

                arRoot.SetActive(true);
                StartTrackingInARMode();
                SetRobotManager(Robomation.RobotManager.instance);
            }

            UpdateUI();
            infoPanel.ShowMouseWorldPosition(!isARMode);

            loadingMask.SetActive(false);
            m_isChangingMode = false;
        }

        private void StartTrackingInARMode()
        {
            if (m_mode == Mode.AR)
            {
                webCamTexHelper.Play();
                markerTracker.StartTracking();
            }
        }

        private void StopTracking()
        {
            webCamTexHelper.Stop();
            markerTracker.StopTracking();
        }

        public Mode mode
        {
            get { return m_mode; }
        }

        public void RunAndSubmit()
        {
            m_needSubmitResult = true;
            Run();
        }

        private void OnQuit(ApplicationQuitEvent quit)
        {
            m_saveController.SaveWithConfirm(delegate { quit.Accept(); }, quit.Ignore);
        }

        public void CommitAndReturn()
        {
            OnClickReturn();
        }


        void IGameboardService.SetRobotScore(int robotIndex, int score)
        {
            if (robotIndex < 0 || robotIndex >= m_result.robotScores.Length)
            {
                Debug.LogError("invalid robot index");
                return;
            }

            if (!m_robotWithResult.Contains(robotIndex))
            {
                m_robotWithResult.Add(robotIndex);
                m_result.robotScores[robotIndex] = score;

                if (m_robotWithResult.Count == m_result.robotScores.Length)
                {
                    if (onResultSet != null) {
                        passMode = PopupILPeriod.PassModeType.Submit;
                        onResultSet.Invoke();
                    }

                    if (m_needSubmitResult && m_submitHandler != null)
                    {
                        var path = m_scriptController.GetRobotCodePath(0);
                        if (path != "")
                        {
                            var repoPath = GameboardRepository.instance.createFilePath(path);
                            m_submitHandler(repoPath, PopupILPeriod.PassModeType.Submit, m_result);
                        }
                        else
                        {
                            m_submitHandler(null, PopupILPeriod.PassModeType.Submit, m_result);
                        }

                    }
                    m_needSubmitResult = false;
                }
            }
        }

        void IGameboardService.SetRobotScore(int score)
        {
            m_result.sceneScore = score;
            if (onResultSet != null)
            {
                passMode = PopupILPeriod.PassModeType.Play;
                onResultSet.Invoke();
            }
            if (m_needSubmitResult && m_submitHandler != null)
            {
                var path = m_scriptController.GetRobotCodePath(0);
                if (path != "")
                {
                    var repoPath = GameboardRepository.instance.createFilePath(path);
                    m_submitHandler(repoPath, PopupILPeriod.PassModeType.Play, m_result);
                }
                else
                {
                    m_submitHandler(null, PopupILPeriod.PassModeType.Play, m_result);
                }

            }
            m_needSubmitResult = false;
        }

        string IGameboardService.GetRobotNickname(int index)
        {
            if (m_robotNicknames.Count == 0)
            {
                return UserManager.Instance.Nickname;
            }

            if (index < 0 || index >= m_robotNicknames.Count)
            {
                return string.Empty;
            }

            return m_robotNicknames[index] ?? string.Empty;
        }

        IEnumerator IGameboardService.StartRobotCode()
        {
            // wait until rigid bodies finish updating
            yield return new WaitForFixedUpdate();

            m_scriptController.Run();
            m_scriptController.SetPaused(codingSpace.IsPaused);
        }

        void IGameboardService.StopRobotCode()
        {
            m_scriptController.Stop();
        }

        Vector2 IGameboardService.mouseWorldPosition
        {
            get { return this.mouseWorldPosition.position.xz(); }
        }

        Vector2 IGameboardService.mouseScreenPosition
        {
            get { return this.mouseScreenPosition.position; }
        }

        Vector2 IGameboardService.screenSize
        {
            get { return uiCanvas.GetComponent<RectTransform>().sizeDelta; }
        }

        Vector3 IGameboardService.GetMarkerPosition(int markerId)
        {
            var marker = markerTracker.GetMarker(markerId);
            if (marker != null)
            {
                var wm = marker.WorldMatrix;
                var pos = ARUtils.ExtractTranslationFromMatrix(ref wm);
                return Coordinates.ConvertVector(pos);
            }
            return Vector3.zero;
        }

        float IGameboardService.GetMarkerRotation(int markerId)
        {
            var marker = markerTracker.GetMarker(markerId);
            if (marker != null)
            {
                var wm = marker.WorldMatrix;
                var rotation = ARUtils.ExtractRotationFromMatrix(ref wm);
                return GeometryUtils.NormalizeAngle(-rotation.eulerAngles.y);
            }
            return 0.0f;
        }

        internal IScriptController scriptController
        {
            get { return m_scriptController; }
        }

        void ResetSceneView()
        {
            SetSceneViewRect(new Rect(Vector2.zero, Vector2.one));
            EnableSceneCameras(true);
        }

        void EnableSceneCameras(bool enabled)
        {
            gameboardSceneManager.renderingOn = enabled;
            arSceneManager.RenderingOn = enabled;
            markerTracker.VideoCamera.enabled = enabled;
            overlayCamera.enabled = enabled;
        }

        void SetSceneViewRect(Rect rc)
        {
            if (gameboardSceneManager.currentCamera)
            {
                gameboardSceneManager.currentCamera.rect = rc;
            }
            if (arSceneManager.ARCamera)
            {
                arSceneManager.ARCamera.rect = rc;
            }
            if (markerTracker.VideoCamera)
            {
                markerTracker.VideoCamera.rect = rc;
            }
            if (overlayCamera)
            {
                overlayCamera.rect = rc;
            }
        }

        public void Pause(bool paused)
        {
            if (!isRunning)
            {
                return;
            }
            foreach (var audio in netAudioSources)
            {
                audio.PauseSound(paused);
            }

            codingSpace.SetPaused(paused);
            m_scriptController.SetPaused(paused);

            pauseButton.isPaused = paused;
            // NOTE: We don't support separate control of pause state of each workspace. All time related
            // block operations rely on the global timer. It's ok for now, because we change 
            // the pause states of all workspaces at the same time.
            Time.timeScale = paused ? 0.0f : 1.0f;

            if (onPauseRunning != null)
            {
                onPauseRunning.Invoke(paused);
            }
        }

        UnityEvent IGameboardPlayer.onStartRunning
        {
            get { return this.onStartRunning; }
        }

        UnityEvent IGameboardPlayer.onStopRunning
        {
            get { return this.onStopRunning; }
        }

        BoolUnityEvent IGameboardPlayer.onPauseRunning
        {
            get { return this.onPauseRunning; }
        }

        public bool isPaused
        {
            get { return codingSpace.IsPaused; }
        }

        internal UIWorkspace codingSpace { get; private set; }

        public void SetWorkingDirectory(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }

            if (codingSpace)
            {
                codingSpace.WorkingDirectory = path;
            }
            else
            {
                m_workingDirectory = path;
            }
        }

        public void SetSortingOrder(int baseOrder)
        {
            canvas.sortingOrder = baseOrder++;
            uiCanvas.sortingOrder = baseOrder++;
        }

        public static int numCanvases
        {
            get { return 2; }
        }

        public void OnInputMaskPointerDown()
        {
            codingSpace.CodeContext.input.enabled = true;
        }

        public void OnInputMaskPointerUp()
        {
            codingSpace.CodeContext.input.enabled = false;
        }

        public void OnClickSave()
        {
            if (m_saveHandler != null)
            {
                m_saveHandler();
            }
            else
            {
                m_saveController.SaveAs(null);
            }
        }

        public void OnClickRetry()
        {
            m_retryClicked = true;
        }

        public UndoManager undoManager
        {
            get;
            private set;
        }

        public void Undo()
        {
            undoManager.Undo();
        }

        public void Redo()
        {
            undoManager.Redo();
        }

        internal void ShowLoadingMask(bool fullScreen)
        {
            var rectTrans = loadingMask.GetComponent<RectTransform>();
            rectTrans.offsetMax = fullScreen ? Vector2.zero : m_loadingMaskOffsetMax;
            loadingMask.SetActive(true);
        }

        internal void CloseLoadingMask()
        {
            loadingMask.SetActive(false);
        }
    }

}