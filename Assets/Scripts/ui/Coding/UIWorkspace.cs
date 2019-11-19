using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DataAccess;
using Scheduling;

public enum NodeCategory
{
    Hamster,
    Phone,
    Events,
    Control,
    Sound,
    Sensing,
    Operators,
    Data,
    Function,
    AR,
    World,

    Count,
}

public enum NodeDropResult
{
    Success,
    Cancel,
    Delete,
}

public class UIWorkspace : UIBehaviour, IPinchHandler
{
    public event Action<UIWorkspace> OnBeforeLoadingCode;
    public event Action<UIWorkspace> OnDidLoadCode;
    public event Action<UIWorkspace> OnSceneViewClicked;
    public event Action OnSystemMenuClicked;
    public event Action OnBackClicked;
    public event Action<FunctionNode, NodeDropResult> OnEndDrag;

    public UnityEvent m_OnStartRunning;
    public UnityEvent m_OnStopRunning;
    public BoolUnityEvent m_OnVisibleChanged;

    public RectTransform m_TemplateViewList;
    public Canvas m_RootCanvas;

    public Button m_btn_ZoomIn;
    public Button m_btn_ZoomOut;

    public NodeFilter m_filter;
    public UIMaskBase m_Mask;

    public NodeTemplateList m_NodeTempList;
    public EventBus m_EventManager;
    public ScreenTextPanel m_ScreenTextPanel;

    public Text m_TextProjectName;

    public GameObject m_PanelTemplate;
    public GameObject m_CodePanelRuntimeMask;

    private CodePanel m_CodePanel;
    public ScrollRect m_CodePanelScrollRect;
	public LeaveMessagePanel m_MessagePanel;
    public RectTransform m_TrashArea;
    public CodePanelToolbar m_Toolbar;
    public BlockInput m_Input;

    public RectTransform m_SceneViewContainer;
    public float m_NormalizedSceneViewHeight;

    public RectTransform m_LeftTopTrans;
    public RectTransform m_RightTopTrans;

    public Text m_FilterTypeText;
    public LayoutElement m_NodeListLayout;
    public RectTransform m_LeftPanelTrans;
    public RectTransform m_RightPanelTrans;
    public RectTransform m_LeftPanelLayoutRoot;

    public GameObject m_OpenNodeListButton;
    public GameObject m_CloseNodeListButton;

    public GameObject m_OpenSceneViewButton;
    public GameObject m_CloseSceneViewButton;

    public UnityCommandManager m_CmdManager;
    public WorkspaceDragController m_DragController;
    public Button m_UndoButton;
    public Button m_RedoButton;
    public CodeType m_CodeType;

    FunctionNode m_MainNode;

    float m_CurZoom;
    private bool m_zoomIn;
    private bool m_zoomOut;
    private Coroutine m_zoomUpdator;

	const float s_kCodePanelEdge = 200.0f;
#if UNITY_EDITOR || UNITY_STANDALONE
    const float m_InitialZoom = 0.6f;
    const float m_MinZoom = 0.5f;
#else
    const float m_InitialZoom = 1.0f;
    const float m_MinZoom = 0.5f;
#endif
    const float m_MaxZoom = 1.25f;
    const float m_ZoomStep = 0.05f;
    const float m_ZoomUpdateInitialWait = 0.5f;
    const float m_ZoomUpdateInterval = 0.1f;

    private bool m_readOnly;

    private string m_currentWorkingDir = string.Empty;
    private ISceneView m_sceneView;
    private float m_originalLeftPanelMaxAnchorX = -1;

    private bool m_closeNodeListButtonVisible = true;
    private bool m_layoutDirty = true;
    private bool m_nodeListVisible = true;

    private bool m_needLayoutTopPanels = true;

    const float s_kScaleDisUnit = 600.0f;

    private UndoManager m_undoManager;

    private NodeMenuHandler m_nodeMenuHandler;
    private string m_id;

    private const string s_kCancelDrag = "cancel_drag";
    private NodePosGenerator m_NodePosGenerator;
    private BlockLevel m_BlockLevel;
    private bool m_SoundEnabled;

    protected override void Awake()
    {
        // #TODO should be initialized by Init
        BlockLevel = Preference.blockLevel;
        m_CurZoom = m_InitialZoom;

        if (RuntimePlatform.Android == Application.platform ||
            RuntimePlatform.IPhonePlayer == Application.platform)
        {
            m_btn_ZoomIn.transform.parent.gameObject.SetActive(false);
            m_btn_ZoomOut.transform.parent.gameObject.SetActive(false);
        }

        CodeContext = new CodeContext(this);
        CodeContext.variableManager = new VariableManager();
        CodeContext.messageManager = new MessageManager();
        CodeContext.messageHandler = new MessageHandler(CodeContext.messageManager);
        CodeContext.eventBus = m_EventManager;
        CodeContext.soundManager = new UnitySoundManager() { maxNumSounds = Constants.MaxNumSounds };

        CodeContext.input = m_Input;

        m_MessagePanel.AvatarService = new CachedAvatarService(new RemoteAvatarService());

        m_filter.onCategorySelected += OnSelectedCategory;
        m_MessagePanel.OnActivating.AddListener(OnActivatingMessagePanel);

        m_CmdManager.Get(s_kCancelDrag).enableStateCheck = () => IsDragging;

        m_nodeMenuHandler = new NodeMenuHandler(this);

        UndoManager = new UndoManager();
        ResetUndoOnLoad = true;

        CodeContext.eventBus.AddListener(EventId.NodePluginChanged, OnNodePluginChanged);

        m_DragController.OnEndDrag += (node, res) => {
            if (OnEndDrag != null)
            {
                OnEndDrag(node, res);
            }
        };

        Id = null;
        // TODO: default value
        NodePosGenerator = new NodePosGenerator(new Vector2(200, -200), new FastRandom(0));
    }

    protected override void OnRectTransformDimensionsChange()
    {
        if (!IsActive())
        {
            return;
        }

        LayoutBottomPanels();
    }

    private void OnUndoStateChanged()
    {
        m_UndoButton.interactable = UndoManager.UndoStackSize > 0 && UndoManager.undoEnabled;
        m_RedoButton.interactable = UndoManager.RedoStackSize > 0 && UndoManager.undoEnabled;
    }

    private void OnUndoRunningChanged()
    {
        if (UndoManager.isRunning)
        {
            ShowMask("ui_workspace_operation_in_progress".Localize());
        }
        else
        {
            CloseMask();
        }
    }

    protected override void OnDestroy()
    {
        ResetStates();

        if (m_MessagePanel.AvatarService != null)
        {
            m_MessagePanel.AvatarService.Dispose();
        }

        CodeContext.eventBus.RemoveListener(EventId.NodePluginChanged, OnNodePluginChanged);
        CodeContext.soundManager.Dispose();
    }

    private void OnNodePluginChanged(object param)
    {
        var args = (NodePluginChangedEvent)param;

        if (!args.plugin.ParentNode.IsTemplate)
        {
            m_undoManager.AddUndo(new UpdatePluginCommand(this, args.plugin, args.oldState));
        }
    }

    public string WorkingDirectory
    {
        get { return m_currentWorkingDir; }
        set
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            m_currentWorkingDir = FileUtils.ensureSlashIfNonEmpty(value);
        }
    }

    public string ProjectPath
    {
        get { return WorkingDirectory + ProjectName; }
    }

    public IEnumerator Init(VoiceRepository voiceRepo, NodeFilterData filterData, bool globalData, ISceneView sceneView)
    {
        ShowMask("ui_workspace_initializing".Localize());

        if (filterData == null)
        {
            throw new ArgumentNullException("filterData");
        }

        m_filter.NodeFilterData = filterData;
        m_filter.Refresh();

        yield return Scheduler.instance.Schedule(InitTemplates());

        m_NodeTempList.NodeFilterData = filterData;
        m_NodeTempList.CanAddGlobalData = globalData;

        m_MessagePanel.VoiceRepo = voiceRepo;

        m_sceneView = sceneView;
        m_SceneViewContainer.gameObject.SetActive(sceneView != null);
        ResetLeftPanel();

        CloseMask();
    }

    IEnumerator InitTemplates()
    {
        foreach (var templateData in NodeTemplateData.Data)
        {
            m_NodeTempList.AddNodeTemplate(templateData.id, templateData.enabled);
            yield return null;
        }
        m_MainNode = NodeTemplateCache.Instance.MainNode;
    }

    public FunctionNode CloneNode(FunctionNode template, Transform parent)
    {
        // template's CodeContext is null before cloning
        template.CodeContext = CodeContext;
        var nodeFunc = template.Clone(parent);
        // avoid keeping temporary reference
        template.CodeContext = null;
        return nodeFunc;
    }

    public void RegisterNodeCallbacks(FunctionNode node)
    {
        node.onPointerDown += OnFunctionNodePointerDown;
        node.onPointerUp += OnFunctionNodePointerUp;
        node.onPointerClick += OnFunctionNodePointerClick;
        node.onDrag += OnFunctionNodeDrag;
    }

    /// <summary>
    /// Serialize the current workspace state as a Project
    /// </summary>
    public Project GetProject()
    {
        var project = new Project();
        project.name = ProjectName;
        project.code = CodePanel.SaveCode();
        project.leaveMessageData = m_MessagePanel.MessageDataSource.serializeMessages();
        return project;
    }

    public IEnumerator Load(Project project)
    {
        FireCodeEvent(OnBeforeLoadingCode);
        ShowMask("ui_workspace_loading_project".Localize());

        if (project == null)
        {
            project = new Project();
        }

        m_TextProjectName.text = project.name ?? string.Empty;
		m_PanelTemplate.SetActive(false);
        m_NodeTempList.RemoveAllFuncCalls();
        m_NodeTempList.ShowNodeByFilter(m_filter.ActiveCategories[0]);
		CreateCodePanel();
        ResetStates();
		if (project.code != null)
		{
            yield return m_CodePanel.LoadCode(project.code);
            foreach (var node in m_CodePanel.FunctionNodes)
            {
                m_NodeTempList.AddFuncCall(node.Declaration);
            }
        }
		else
		{
			CreateMainNode();
            SetZoom(m_CurZoom);
		}
        m_MessagePanel.SetActive(false);
        m_MessagePanel.LoadMessages(project.leaveMessageData);
        if (CodeContext.messageManager.count == 0)
        {
            CodeContext.messageManager.add(Message.defaultMessage);
        }
        m_filter.Level = BlockLevel;
        m_filter.Refresh();

        if (ResetUndoOnLoad)
        {
            m_undoManager.Reset();
        }

        IsChanged = false;
        CloseMask();
        FireCodeEvent(OnDidLoadCode);
    }

	void CreateCodePanel()
	{
        if (m_CodePanel)
        {
    		Destroy(m_CodePanel.gameObject);
        }
		GameObject panel = Instantiate(m_PanelTemplate, m_PanelTemplate.transform.parent, false);
		panel.SetActive(true);
		m_CodePanel = panel.GetComponent<CodePanel>();
        m_CodePanel.IsVisible = IsVisible;
        m_CodePanel.EdgeSize = s_kCodePanelEdge;
        m_CodePanel.OnClicked += OnClickCodePanel;
		m_CodePanelScrollRect.content = panel.GetComponent<RectTransform>();
	}

	void CreateMainNode()
	{
        var mainNode = CloneNode(m_MainNode, m_CodePanel.transform);
        mainNode.IsTemplate = false;

		m_CodePanel.AddNode(mainNode);
        mainNode.MoveTo(m_CodePanel.GetLocalPos(Vector2.zero, true));
    }

    void FireCodeEvent(Action<UIWorkspace> handlers)
    {
        if (handlers != null)
        {
            handlers(this);
        }
    }

    public void New()
    {
        StartCoroutine(Load(null));
    }

    void ResetStates()
    {
        CodeContext.variableManager.clear();
        CodeContext.messageManager.clear();
    }

    public void Run(bool resetGlobalVars = true, bool keepRunning = false)
    {
        m_CodePanel.Run(resetGlobalVars, keepRunning);
    }

    public void Stop()
    {
        m_CodePanel.Stop();
    }

    public bool IsRunning
    {
        get { return m_CodePanel.IsRunning; }
    }

    public void SetPaused(bool paused)
    {
        m_CodePanel.SetPaused(paused);
    }

    public bool IsPaused
    {
        get { return m_CodePanel && m_CodePanel.IsPaused; }
    }

    public string ProjectName
    {
        get { return m_TextProjectName.text; }
        set { m_TextProjectName.text = value ?? string.Empty; }
    }

    public CodePanel CodePanel
    {
        get { return m_CodePanel; }
    }

    public float GetCanvasScale()
    {
        return m_RootCanvas.scaleFactor;
    }

    public UndoManager UndoManager
    {
        get { return m_undoManager; }
        set
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            if (UndoManager != null)
            {
                UndoManager.onStackSizeChanged -= OnUndoStateChanged;
                UndoManager.onUndoEnabledChanged -= OnUndoStateChanged;
                UndoManager.onRunningChanged -= OnUndoRunningChanged;
            }

            m_undoManager = value;
            UndoManager.onStackSizeChanged += OnUndoStateChanged;
            UndoManager.onUndoEnabledChanged += OnUndoStateChanged;
            UndoManager.onRunningChanged += OnUndoRunningChanged;
            OnUndoStateChanged();
        }
    }

    public bool ResetUndoOnLoad { get; set; }

    #region zoom

    public void BeginZoomOut()
    {
        m_zoomIn = false;
        m_zoomOut = true;
        StartZoom();
    }

    public void EndZoomOut()
    {
        m_zoomOut = false;
    }

    public void BeginZoomIn()
    {
        m_zoomOut = false;
        m_zoomIn = true;
        StartZoom();
    }

    public void EndZoomIn()
    {
        m_zoomIn = false;
    }

    void StartZoom()
    {
        if (m_zoomUpdator == null)
        {
            m_zoomUpdator = StartCoroutine(UpdateZoom());
        }
    }

    IEnumerator UpdateZoom()
    {
        bool initialZoom = true;

        while (m_zoomOut || m_zoomIn)
        {
            if (m_zoomOut && !m_zoomIn)
            {
                ZoomOut();
                if (m_CurZoom == m_MinZoom)
                {
                    m_zoomOut = false;
                    break;
                }
            }
            else if (m_zoomIn && !m_zoomOut)
            {
                ZoomIn();
                if (m_CurZoom == m_MaxZoom)
                {
                    m_zoomIn = false;
                    break;
                }
            }

            if (initialZoom)
            {
                initialZoom = false;
                yield return new WaitForSecondsRealtime(m_ZoomUpdateInitialWait);
            }
            else
            {
                yield return new WaitForSecondsRealtime(m_ZoomUpdateInterval);
            }
        }

        m_zoomUpdator = null;
    }

    public void ZoomOut()
    {
        SetZoom(m_CurZoom - m_ZoomStep);
    }

    public void ResetZoom()
    {
        SetZoom(m_InitialZoom);
    }

    public void ZoomIn()
    {
        SetZoom(m_CurZoom + m_ZoomStep);
    }

    public void SetZoom(float zoom)
    {
        m_CurZoom = Mathf.Clamp(zoom, m_MinZoom, m_MaxZoom);
        UpdateZoomState();
    }

    void UpdateZoomState()
    {
        Vector3 curScale = Vector3.one * m_CurZoom;

        m_TemplateViewList.localScale = curScale;

        m_btn_ZoomOut.interactable = m_CurZoom > m_MinZoom;
        m_btn_ZoomIn.interactable = m_CurZoom < m_MaxZoom;

        m_CodePanel.transform.localScale = curScale;
        m_CodePanel.RecalculateContentSize();

        m_MessagePanel.ReleaseActiveTag();

        m_EventManager.AddEvent(EventId.PickUpNode, null);
        m_EventManager.AddEvent(EventId.PutDownNode_LateRefresh, null);
    }

    public float CurrentZoom
    {
        get { return m_CurZoom; }
    }

    #endregion zoom

    public BlockLevel BlockLevel
    {
        get { return m_BlockLevel; }
        set
        {
            m_BlockLevel = value;
            m_filter.Level = value;
        }
    }

    public AutoStop StopMode
    {
        get;
        set;
    }

    public bool SoundEnabled
    {
        get { return m_SoundEnabled; }
        set
        {
            m_SoundEnabled = value;
            CodeContext.soundManager.mute = !value;
        }
    }

    public void RefreshFilterAndTemplateList()
    {
        m_filter.Refresh();
        if (!m_filter.HasCategory(m_NodeTempList.CurrentCategory))
        {
            m_NodeTempList.ShowNodeByFilter(m_filter.ActiveCategories[0]);
        }
        else
        {
            m_NodeTempList.RefreshCurrentCategory();
        }
    }

    public void ShowMask(string text)
    {
        m_Mask.ShowMask(text);
    }

    public void CloseMask()
    {
        m_Mask.CloseMask();
    }

    void IPinchHandler.OnBeginPinch(PinchEventData eventData)
    {
        m_DragController.CancelDrag();
    }

    void IPinchHandler.OnPinch(PinchEventData eventData)
    {
        float tScale = (eventData.currentFingerDistance - eventData.lastFingerDistance) / s_kScaleDisUnit;
        SetZoom(m_CurZoom + tScale);
    }

    void IPinchHandler.OnEndPinch(PinchEventData eventData)
    { }


    public void Show(bool visible)
    {
        if (visible != IsVisible)
        {
            m_RootCanvas.enabled = visible;
            if (m_CodePanel)
            {
                m_CodePanel.IsVisible = visible;
            }

            if (m_sceneView != null)
            {
                m_sceneView.enabled = visible;
            }

            if (m_layoutDirty && visible)
            {
                LayoutBottomPanels();
            }

            m_OnVisibleChanged.Invoke(visible);
        }
    }

    public bool IsVisible
    {
        get { return m_RootCanvas.enabled; }
    }

    public bool IsReadOnly
    {
        get { return m_readOnly; }
        set
        {
            m_readOnly = value;
            UpdateUI();
        }
    }

    private void UpdateUI()
    {
        bool isReadOnly = m_CodePanel.IsRunning || m_readOnly;
        m_CodePanelRuntimeMask.SetActive(isReadOnly);
        m_NodeTempList.EnableMask(isReadOnly);
        m_MessagePanel.IsReadOnly = isReadOnly;
        m_Toolbar.CopyEnabled = !isReadOnly;
        m_Toolbar.SystemButtonEnabled = !isReadOnly;
    }

    public void OnCodeStartRunning()
    {
        UpdateUI();

        if (m_OnStartRunning != null)
        {
            m_OnStartRunning.Invoke();
        }
    }

    public void OnCodeStopRunning()
    {
        UpdateUI();

        if (m_OnStopRunning != null)
        {
            m_OnStopRunning.Invoke();
        }
    }

    #region FunctionNode handlers
    private void OnFunctionNodePointerDown(FunctionNode node, PointerEventData eventData)
    {
        m_DragController.OnFunctionNodePointerDown(node, eventData);
    }

    private void OnFunctionNodePointerUp(PointerEventData eventData)
    {
        m_DragController.OnFunctionNodePointerUp(eventData);
    }

    private void OnFunctionNodePointerClick(PointerEventData eventData)
    {
        if (!m_DragController.OnFunctionNodePointerClick(eventData))
        {
            m_nodeMenuHandler.OnPointerClick(eventData);
        }
    }

    private void OnFunctionNodeDrag(PointerEventData eventData)
    {
        m_DragController.OnFunctionNodeDrag(eventData);
    }
    #endregion FunctionNode handlers

    public bool IsDragging
    {
        get { return m_DragController.IsDragging; }
    }

    public void BeginDrag(FunctionNode node)
    {
        m_DragController.BeginDrag(node);
    }

    public void EndDrag()
    {
        m_DragController.EndDrag();
    }

    public void CancelDrag()
    {
        m_DragController.CancelDrag();
    }

    public IClosestConnectionFilter ConnectionFilter
    {
        get;
        set;
    }

    public CodeContext CodeContext
    {
        get;
        private set;
    }

    public bool IsChanged
    {
        get { return !UndoManager.IsClean(); }
        set { UndoManager.SetClean(!value); }
    }

    // the hook will be called right before deleting a variable.
    // to delete the given variable, invoke the passed delegate
    public DeletingVariableHandler deletingVariableHandler
    {
        get { return m_NodeTempList.deletingVariableHandler; }
        set { m_NodeTempList.deletingVariableHandler = value; }
    }

    public ISceneView sceneView
    {
        get { return m_sceneView; }
    }

    public void ResetLeftPanel()
    {
        m_OpenNodeListButton.SetActive(false);
        m_CloseNodeListButton.SetActive(m_sceneView == null && m_closeNodeListButtonVisible);

        m_OpenSceneViewButton.SetActive(false);
        m_CloseSceneViewButton.SetActive(m_sceneView != null);

        if (m_sceneView != null)
        {
            m_sceneView.enabled = true;
        }
        LayoutBottomPanels();
    }

    void LayoutBottomPanels()
    {
        if (!IsVisible)
        {
            m_layoutDirty = true;
            return;
        }

        m_layoutDirty = false;

        if (m_sceneView != null)
        {
            Assert.IsTrue(m_nodeListVisible);

            LayoutRebuilder.ForceRebuildLayoutImmediate(m_LeftPanelLayoutRoot);

            var screenSize = m_RootCanvas.pixelRect.size;
            var height = m_LeftPanelLayoutRoot.rect.height * m_NormalizedSceneViewHeight;
            var width = height * screenSize.x / screenSize.y;

            UpdateBottomPanelAnchors(width);

            if (m_needLayoutTopPanels)
            {
                m_needLayoutTopPanels = false;

                m_LeftTopTrans.SetAnchorMax(RectTransform.Axis.Horizontal, m_LeftPanelTrans.anchorMax.x);
                m_RightTopTrans.SetAnchorMin(RectTransform.Axis.Horizontal, m_LeftPanelTrans.anchorMax.x);
            }

            m_SceneViewContainer.GetComponent<LayoutElement>().preferredHeight = height;

            var min = m_SceneViewContainer.localToWorldMatrix.MultiplyPoint3x4(m_SceneViewContainer.rect.min);
            var max = m_SceneViewContainer.localToWorldMatrix.MultiplyPoint3x4(
                m_SceneViewContainer.rect.min + new Vector2(width, height));

            var pos = new Vector2(min.x / screenSize.x, min.y / screenSize.y);
            var size = new Vector2((max.x - min.x) / screenSize.x, (max.y - min.y) / screenSize.y);

            m_sceneView.SetNormalizedRect(new Rect(pos, size));
        }
        else if (!m_nodeListVisible)
        {
            var width = m_filter.GetComponent<RectTransform>().rect.width;
            UpdateBottomPanelAnchors(width);
        }
        else
        {
            if (m_originalLeftPanelMaxAnchorX < 0)
            {
                m_originalLeftPanelMaxAnchorX = m_LeftPanelTrans.anchorMax.x;
            }

            var width = ((RectTransform)m_LeftPanelTrans.parent).rect.width * m_originalLeftPanelMaxAnchorX;
            UpdateBottomPanelAnchors(width);
        }

        if (m_CodePanel)
        {
            m_CodePanel.RecalculateContentSize();
        }
    }

    void UpdateBottomPanelAnchors(float leftPanelWidth)
    {
        var parentWidth = ((RectTransform)m_LeftPanelTrans.parent).rect.width;

        var leftPanelMax = leftPanelWidth / parentWidth;
        m_LeftPanelTrans.SetAnchorMax(RectTransform.Axis.Horizontal, leftPanelMax);
        m_RightPanelTrans.SetAnchorMin(RectTransform.Axis.Horizontal, leftPanelMax);

        m_MessagePanel.SetMessageListNormalizedWidth(leftPanelMax);
    }

    public void ShowNodeList(bool visible)
    {
        if (!visible && m_sceneView != null)
        {
            Debug.LogError("cannot hide node list when scene view is present");
            return; 
        }

        if (m_nodeListVisible != visible)
        {
            m_nodeListVisible = visible;
            m_FilterTypeText.enabled = visible;
            m_CloseNodeListButton.SetActive(visible && m_closeNodeListButtonVisible);
            m_OpenNodeListButton.SetActive(!visible);
            m_NodeListLayout.flexibleWidth = visible ? 1 : 0;
            m_NodeTempList.ShowNodes(visible);
            LayoutBottomPanels();
        }
    }

    public void ShowSceneView(bool visible)
    {
        if (m_sceneView != null)
        {
            m_OpenSceneViewButton.SetActive(!visible);
            m_SceneViewContainer.gameObject.SetActive(visible);
            m_sceneView.enabled = visible;
        }
    }

    public void OnClickSceneView()
    {
        if (OnSceneViewClicked != null)
        {
            OnSceneViewClicked(this);
        }
    }

    void OnSelectedCategory(NodeCategory category)
    {
        if (category == NodeCategory.AR && !UserManager.Instance.IsArUser)
        {
            PopupManager.ActivationCode(PopupActivation.Type.AR);
            return;
        }

        ShowNodeList(true);
        m_NodeTempList.ShowNodeByFilter(category);
    }

    void OnActivatingMessagePanel(bool active)
    {
        if (active)
        {
            ShowNodeList(true);
        }
    }

    public void ShowCloseNodeListButton(bool visible)
    {
        m_closeNodeListButtonVisible = visible;
        if (m_CloseNodeListButton.activeSelf)
        {
            m_CloseNodeListButton.SetActive(visible);
        }
    }

    public void OnClickSystemMenu()
    {
        if (OnSystemMenuClicked != null)
        {
            OnSystemMenuClicked();
        }
    }

    public void OnClickBack()
    {
        if (OnBackClicked != null)
        {
            OnBackClicked();
        }
    }

	public void OpenMonitor()
	{
        var dialog = UIDialogManager.g_Instance.GetDialog<UIMonitorDialog>();
        dialog.Configure(CodeContext.robotManager, CodeContext.variableManager, Application.isMobilePlatform);
        dialog.OpenDialog();
	}

    public void Undo()
    {
        m_undoManager.Undo();
    }

    public void Redo()
    {
        m_undoManager.Redo();
    }

    public CodeType CodeType
    {
        get { return m_CodeType; }
    }

    /// <summary>
    /// get/set the workspace id, if id is null or empty, a unique id is generated
    /// </summary>
    public string Id
    {
        get { return m_id; }
        set { m_id = string.IsNullOrEmpty(value) ? Guid.NewGuid().ToString() : value; }
    }

    private void OnClickCodePanel(PointerEventData eventData)
    {
        m_nodeMenuHandler.OnPointerClick(eventData);
    }

    public NodePosGenerator NodePosGenerator
    {
        get { return m_NodePosGenerator; }
        set
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            m_NodePosGenerator = value;
        }
    }

    void Update()
    {
        CodeContext.soundManager.Update();
    }
}
