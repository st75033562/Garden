using DataAccess;
using Google.Protobuf;
using Robomation;
using Scheduling;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CodePanel : MonoBehaviour, IPointerClickHandler
{
    public UnityEvent m_OnStartRunning;
    public UnityEvent m_OnStopRunning;

    public event Action<FunctionNode> OnNodeAdded;
    public event Action<PointerEventData> OnClicked;

    public UIWorkspace m_Workspace;
    public EventBus m_EventManager;
    public Canvas m_Canvas;

    private class RunningNodeState
    {
        public FunctionNode m_CurNode;
        public readonly MainNode m_MainNode;
        public readonly bool m_bLoop;
        public Coroutine runner;
        public readonly ThreadContext threadContext;
        public readonly MonoBehaviour coroutineService;

        public RunningNodeState(CodePanel panel, MainNode mainNode, CallStackOverflowHandler onOverflow)
        {
            m_CurNode = mainNode.GetComponent<FunctionNode>();
            m_bLoop = mainNode.GetComponent<LoopMainBlock>() != null;
            m_MainNode = mainNode;
            coroutineService = panel.gameObject.AddComponent<CoroutineService>();
            threadContext = new ThreadContext(coroutineService, onOverflow, DataAccess.Constants.CallStackMaxLimit);
        }

        public void Destroy()
        {
            if (coroutineService)
            {
                UnityEngine.Object.Destroy(coroutineService);
            }
        }

        public void Restart()
        {
            m_CurNode = m_MainNode.GetComponent<FunctionNode>();
        }

        public void Stop()
        {
            foreach (var node in threadContext.runningNodes)
            {
                node.SetColor(NodeColor.Normal, false);
            }
            threadContext.Reset();
            if (coroutineService)
            {
                coroutineService.StopAllCoroutines();
            }
        }
    }

    private class RobotPauseState
    {
        int m_leftWheelSpeed;
        int m_rightWheelSpeed;
        float m_buzzerFrequency;
        int m_note;

        public void Save(IRobot robot)
        {
            m_leftWheelSpeed = robot.read(Hamster.LEFT_WHEEL);
            robot.write(Hamster.LEFT_WHEEL, 0);

            m_rightWheelSpeed = robot.read(Hamster.RIGHT_WHEEL);
            robot.write(Hamster.RIGHT_WHEEL, 0);

            m_buzzerFrequency = robot.readFloat(Hamster.BUZZER);
            robot.writeFloat(Hamster.BUZZER, 0);

            m_note = robot.read(Hamster.NOTE);
            robot.write(Hamster.NOTE, 0);
        }

        public void Restore(IRobot robot)
        {
            robot.write(Hamster.LEFT_WHEEL, m_leftWheelSpeed);
            robot.write(Hamster.RIGHT_WHEEL, m_rightWheelSpeed);
            robot.writeFloat(Hamster.BUZZER, m_buzzerFrequency);
            robot.write(Hamster.NOTE, m_note);
        }
    }

    private List<FunctionNode> m_Nodes;
    private readonly List<FunctionDeclarationNode> m_FuncNodes = new List<FunctionDeclarationNode>();
    private ConnectionRegistry m_ConnectionRegistry;

    bool m_Running;

    private readonly List<RobotPauseState> m_robotPauseStates = new List<RobotPauseState>();

    List<MainNode> m_MainNodes;
    List<RunningNodeState> m_RunningNodeStates;
    bool m_RunningForever;

    Coroutine m_CoFinishRunning;

    RectTransform m_Rect;

    int m_NextNodeIndex;


    // Use this for initialization
    void Awake()
    {
        m_Nodes = new List<FunctionNode>();
        m_ConnectionRegistry = new ConnectionRegistry(transform);
        m_MainNodes = new List<MainNode>();
        m_RunningNodeStates = new List<RunningNodeState>();

        m_Running = false;
        m_Rect = GetComponent<RectTransform>();
        m_NextNodeIndex = 0;
        ResetRobotsOnRun = true;
    }

    void OnEnable()
    {
    }

    void OnDisable()
    {
    }

    void OnDestroy()
    {
        foreach (var state in m_RunningNodeStates)
        {
            state.Destroy();
        }
        ResetRobots();
    }

    // Update is called once per frame
    void Update()
    {
        if (IsPaused || !IsRunning)
        {
            return;
        }

        m_Workspace.CodeContext.timer.Update(Time.deltaTime);
        m_Workspace.CodeContext.messageHandler.Update();

        if (Input.anyKey)
        {
            // ignore escape
            if (!Input.GetKey(KeyCode.Escape))
            {
                m_EventManager.AddEvent(EventId.KeyPressed);
            }
        }

        CheckEndOfRun();
    }

    public ConnectionRegistry ConnectionRegistry
    {
        get { return m_ConnectionRegistry; }
    }

    public ClosesetConnectionTestResult FindClosestMatchingConnection(
        Connection source, IClosestConnectionFilter filter = null)
    {
        return m_ConnectionRegistry.FindClosestMatchingConnection(source, filter);
    }

    // for undo/redo
    public int NextNodeIndex
    {
        get { return m_NextNodeIndex; }
        set
        {
#if DEBUG
            var maxNodeIndex = m_Nodes.Max(x => x.NodeIndex);
            if (value < maxNodeIndex)
            {
                throw new ArgumentException("value");
            }
#endif
            m_NextNodeIndex = value;
        }
    }

    public void AddNode(FunctionNode node, bool updateNodeIndex = true)
    {
#if UNITY_EDITOR
        Assert.IsNotNull(node, "node should not be null");
        Assert.IsFalse(m_Nodes.Contains(node), "duplicate node");
        Assert.IsTrue(updateNodeIndex || GetNode(node.NodeIndex) == null, "duplicate node index");
#endif

        m_Nodes.Add(node);
        node.CodePanel = this;

        m_Workspace.RegisterNodeCallbacks(node);
        if (updateNodeIndex)
        {
            ++m_NextNodeIndex;
            node.NodeIndex = m_NextNodeIndex;
        }
        MainNode mainNode = node.GetComponent<MainNode>();
        if (mainNode)
        {
            m_MainNodes.Add(mainNode);
        }
        if (node is FunctionDeclarationNode)
        {
            var declNode = (FunctionDeclarationNode)node;
#if UNITY_EDITOR
            Assert.IsTrue(m_FuncNodes.Find(x => x.Declaration.functionId == declNode.Declaration.functionId) == null,
                          "duplicate function");
#endif
            m_FuncNodes.Add(declNode);
        }

        m_ConnectionRegistry.Register(node);
    }

    public void Run(bool resetGlobalVars, bool keepRunning)
    {
        //ScreenDebug.ScreenPrint("code will go");
        if (m_Running)
        {
            return;
        }

        Assert.IsNotNull(RobotManager, "RobotManager not set");

        m_RunningForever = keepRunning;
        if (ResetRobotsOnRun)
        {
            m_Workspace.CodeContext.robotRuntime.SetStateCount(m_Workspace.CodeContext.robotManager.robotCount);
            m_Workspace.CodeContext.robotRuntime.ResetStates();
        }
        m_RunningNodeStates.Clear();
        for (int i = m_MainNodes.Count - 1; i >= 0; --i)
        {
            // key block is triggered manually
            if (!m_MainNodes[i].GetComponent<WhenKeyPressedBlock>())
            {
                PrepareToRun(m_MainNodes[i]);
                m_MainNodes.RemoveAt(i);
            }
            else
            {
                m_RunningForever = true;
            }
        }

        // reset all variables
        m_Workspace.CodeContext.variableManager.reset(resetGlobalVars);
        m_Workspace.CodeContext.timer.Reset();
        m_Workspace.CodeContext.timer.Pause();
        m_Workspace.CodeContext.messageHandler.Reset();

        m_Running = true;

        if (m_OnStartRunning != null)
        {
            m_OnStartRunning.Invoke();
        }
    }

    private void PrepareToRun(MainNode node)
    {
        var state = new RunningNodeState(this, node, OnThreadAborted);
        m_RunningNodeStates.Add(state);
        state.runner = state.coroutineService.StartCoroutine(CoroutineUtils.Run(Run(state)));
    }

    private void OnThreadAborted(ThreadContext context)
    {
        PopupManager.Notice("ui_error_max_call_stack_limit_reached".Localize(context.callStackLimit));
    }

    private IEnumerator Run(RunningNodeState state)
    {
        // wait until everything is ready
        while (!m_Running)
        {
            yield return null;
        }

        while (state.m_CurNode)
        {
            while (IsPaused)
            {
                yield return null;
            }

            state.m_CurNode.SetColor(NodeColor.Play, false);

            yield return state.m_CurNode.ActionBlock(state.threadContext);

            state.m_CurNode.SetColor(NodeColor.Normal, false);
            state.m_CurNode = state.m_CurNode.NextNode;

            if (!state.m_CurNode || state.threadContext.isReturned || state.threadContext.isAborted)
            {
                // if loop node, restart
                if (state.m_bLoop)
                {
                    state.Restart();
                    // yield to other threads
                    yield return null;
                }
                else
                {
                    m_RunningNodeStates.Remove(state);
                    m_MainNodes.Add(state.m_MainNode);
                    state.Destroy();

                    if (state.threadContext.isReturned || state.threadContext.isAborted)
                    {
                        break;
                    }
                }
            }
        }
    }

    private void CheckEndOfRun()
    {
        if (m_RunningNodeStates.Count == 0 && !m_RunningForever)
        {
            if (m_Workspace.StopMode == AutoStop.Immediate)
            {
                Finish();
            }
            else if (m_Workspace.StopMode == AutoStop.AfterOneSec && m_CoFinishRunning == null)
            {
                m_CoFinishRunning = StartCoroutine(WaitAndStop(1.0f));
            }
        }
    }

    private IEnumerator WaitAndStop(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        Finish();
    }

    private void Finish()
    {
        //ScreenDebug.ScreenPrint("Finish");
        m_Running = false;
        m_RunningForever = false;
        ResetRobots();
        StopFinishCoroutine();

        m_Workspace.CodeContext.soundManager.isPaused = false;
        m_Workspace.CodeContext.soundManager.ReleaseAll();

        if (m_OnStopRunning != null)
        {
            m_OnStopRunning.Invoke();
        }
    }

    private void ResetRobots()
    {
        if (ResetRobotsOnRun)
        {
            RobotManager.resetRobots();
        }
    }

    public bool ResetRobotsOnRun
    {
        get;
        set;
    }

    public void Stop()
    {
        m_Running = false;
        IsPaused = false;

        foreach (var state in m_RunningNodeStates)
        {
            state.Stop();
            state.Destroy();
            m_MainNodes.Add(state.m_MainNode);
        }
        m_RunningNodeStates.Clear();

        Finish();
    }

    public void SetPaused(bool paused)
    {
        if (m_Running && IsPaused != paused)
        {
            IsPaused = paused;
            m_Workspace.CodeContext.soundManager.isPaused = paused;

            if (ResetRobotsOnRun)
            {
                if (paused)
                {
                    for (int i = 0; i < RobotManager.robotCount; ++i)
                    {
                        if (i == m_robotPauseStates.Count)
                        {
                            m_robotPauseStates.Add(new RobotPauseState());
                        }
                        m_robotPauseStates[i].Save(RobotManager.get(i));
                    }
                }
                else
                {
                    for (int i = 0; i < RobotManager.robotCount; ++i)
                    {
                        m_robotPauseStates[i].Restore(RobotManager.get(i));
                    }
                }
            }
        }
    }

    public bool IsPaused
    {
        get;
        private set;
    }

    private void StopFinishCoroutine()
    {
        if (m_CoFinishRunning != null)
        {
            StopCoroutine(m_CoFinishRunning);
            m_CoFinishRunning = null;
        }
    }

    public bool IsRunning
    {
        get { return m_Running; }
    }

    /// <summary>
    /// The edge size around the content
    /// </summary>
    public float EdgeSize
    {
        get;
        set;
    }

    public Vector2 RecalculateContentSize()
    {
        float edge = EdgeSize;
        float panelLeft, panelRight, panelTop, panelBottom;
        panelLeft = panelRight = panelTop = panelBottom = 0.0f;

        // calculate the bounding rect of all nodes
        foreach (var node in m_Nodes)
        {
            RectTransform rectTrans = node.RectTransform;
            Vector3 pos = transform.InverseTransformPoint(rectTrans.position);

            panelLeft = Mathf.Min(pos.x - edge, panelLeft);
            panelTop = Mathf.Max(pos.y + edge, panelTop);
            panelRight = Mathf.Max(pos.x + rectTrans.rect.size.x + edge, panelRight);
            panelBottom = Mathf.Min(pos.y - rectTrans.rect.size.y - edge, panelBottom);
        }

        var parentTrans = transform.parent.GetComponent<RectTransform>();
        var viewportSize = parentTrans.rect.size;

        float finalWidth = Mathf.Max(viewportSize.x / transform.localScale.x, panelRight - panelLeft);
        float finalHeight = Mathf.Max(viewportSize.y / transform.localScale.y, panelTop - panelBottom);

        m_Rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, finalWidth);
        m_Rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, finalHeight);

        var newPanelPos = m_Rect.localPosition;
        var nodeOffset = new Vector3(-panelLeft, -panelTop);
        if (nodeOffset != Vector3.zero)
        {
            // make sure the anchor is at the top left corner
            newPanelPos += new Vector3(panelLeft * m_Rect.localScale.x, panelTop * m_Rect.localScale.y);

            foreach (var freeNode in m_Nodes.Where(x => x.IsFreeNode))
            {
                // compensate for the movement of the panel, make sure the node is not moved on the screen
                freeNode.LogicTransform.localPosition += nodeOffset;
                freeNode.PositionSiblingsTopDown();
            }
        }

        // calculate the offset from the right edge to the parent's right
        float panelRightOffset = parentTrans.rect.size.x - (newPanelPos.x + m_Rect.rect.size.x * m_Rect.localScale.x);
        // calculate the offset from the bottom edge to the parent's bottom
        float panelBottomOffset = parentTrans.rect.size.y - (m_Rect.rect.size.y * m_Rect.localScale.y - newPanelPos.y);

        newPanelPos.x += Mathf.Max(panelRightOffset, 0);
        newPanelPos.y += Mathf.Min(-panelBottomOffset, 0);
        m_Rect.localPosition = newPanelPos;

        return nodeOffset;
    }

    public void RemoveNode(FunctionNode node)
    {
        m_ConnectionRegistry.Unregister(node);

        m_Nodes.Remove(node);

        if (node.GetComponent<MainNode>())
        {
            m_MainNodes.Remove(x => x.gameObject == node.gameObject);
        }

        if (node is FunctionDeclarationNode)
        {
            var funcNode = (FunctionDeclarationNode)node;
            m_FuncNodes.Remove(funcNode);
        }
    }

    public byte[] SaveCode()
    {
        var projectData = new Save_ProjectData();
        projectData.SoundState = m_Workspace.SoundEnabled;
        projectData.StopMode = (int)m_Workspace.StopMode;
        projectData.BlockLevel = (int)m_Workspace.BlockLevel;
        projectData.NodeIndex = m_NextNodeIndex;
        projectData.PanelScale = m_Rect.localScale.x;
        projectData.PanelPosX = m_Rect.position.x;
        projectData.PanelPosY = m_Rect.position.y;
        projectData.PanelPosZ = m_Rect.position.z;
        projectData.PanelPos = new Save_Vector2(m_Rect.localPosition.x, m_Rect.localPosition.y);

        // to preserve the z ordering of different stacks after loading
        m_Nodes.Sort((x, y) => x.transform.GetSiblingIndex() - y.transform.GetSiblingIndex());

        // only save non-transient nodes
        foreach (var node in m_Nodes.Where(x => !x.IsTransient))
        {
            projectData.NodeList.Add(node.GetNodeSaveData());
        }

        // save function decls
        foreach (var node in m_FuncNodes)
        {
            projectData.FuncDecls.Add(node.Declaration.Serialize());
            projectData.DeclNodes.Add(node.NodeIndex, projectData.FuncDecls.Count - 1);
        }

        // save references to func decls
        foreach (var node in m_Nodes.OfType<FunctionCallNode>())
        {
            var dict = node.Insertable ? projectData.CallReturnNodes : projectData.CallNodes;
            var declIndex = m_FuncNodes.FindIndex(x => x.Declaration.functionId == node.Declaration.functionId);
            dict.Add(node.NodeIndex, declIndex);
        }

        foreach (var item in m_Workspace.CodeContext.variableManager)
        {
            if (item.scope == NameScope.Global)
            {
                projectData.GlobalVariables.Add(item.serialize());
            }
            else
            {
                projectData.LocalVariables.Add(item.serialize());
            }
        }

        m_Workspace.CodeContext.messageManager.save(projectData);

        return projectData.ToByteArray();
    }

    public IEnumerator LoadCode(byte[] data)
    {
        var projectData = Save_ProjectData.Parser.ParseFrom(data);
        m_Workspace.SoundEnabled = projectData.SoundState;
        m_Workspace.StopMode = (AutoStop)projectData.StopMode;
        m_Workspace.BlockLevel = (BlockLevel)Mathf.Min(projectData.BlockLevel, (int)BlockLevel.Advanced);
        m_NextNodeIndex = projectData.NodeIndex;

        LoadVariables(projectData);
        m_Workspace.CodeContext.messageManager.load(projectData);

        var saveStates = new BlockSaveStates(
            projectData.NodeList, 
            projectData.FuncDecls.Select(x => FunctionDeclaration.Deserialize(x)), 
            projectData.DeclNodes, 
            projectData.CallNodes,
            projectData.CallReturnNodes);

        yield return AddNodesAsync(saveStates, false);

        if (projectData.PanelPos != null)
        {
            m_Rect.localPosition = projectData.PanelPos.ToVector2();
        }
        else
        {
            m_Rect.localPosition = Vector3.zero;
        }
        m_Workspace.SetZoom(projectData.PanelScale);
    }

    // add nodes by loading from the states
    // nodes are returned in the same order as the states
    public ITask<List<FunctionNode>> AddNodesAsync(BlockSaveStates nodeStates, bool updateIds)
    {
        return Scheduler.instance.Schedule<List<FunctionNode>>(AddNodesImpl(nodeStates, updateIds));
    }

    private IEnumerator AddNodesImpl(BlockSaveStates nodeStates, bool updateIds)
    {
        transform.hierarchyCapacity = transform.hierarchyCount + nodeStates.stateCount;

        var linkMap = new Dictionary<int, FunctionNode>();
        var nodes = new List<FunctionNode>();
        var dynamicNodes = new Dictionary<int, FunctionNode>();
        var unresolvedFuncNodes = new List<FunctionNode>();

        var graphics = new List<Graphic>();
        // create function decl nodes
        foreach (var declInfo in nodeStates.GetFuncDecls())
        {
            var node = (FunctionDeclarationNode)m_Workspace.m_NodeTempList.FuncDeclNode.Clone(transform);
            node.gameObject.SetActive(true);
            node.Rebuild(declInfo.decl);
            dynamicNodes.Add(declInfo.nodeIndex, node);
            if (!declInfo.resolved)
            {
                unresolvedFuncNodes.Add(node);
            }
            Hide(node, graphics);
        }

        yield return null;

        // create function call nodes
        foreach (var declInfo in nodeStates.funcCalls)
        {
            var node = Instantiate(m_Workspace.m_NodeTempList.m_FuncCallTemplate, transform, false)
                .GetComponent<FunctionCallNode>();
            node.CodeContext = m_Workspace.CodeContext;
            node.Rebuild(declInfo.Value);
            dynamicNodes.Add(declInfo.Key, node);
            Hide(node, graphics);
        }

        yield return null;

        // create function call return nodes
        foreach (var declInfo in nodeStates.funcReturnCalls)
        {
            var node = Instantiate(m_Workspace.m_NodeTempList.m_FuncCallReturnTemplate, transform, false)
                .GetComponent<FunctionCallNode>();
            node.CodeContext = m_Workspace.CodeContext;
            node.Rebuild(declInfo.Value);
            dynamicNodes.Add(declInfo.Key, node);
            Hide(node, graphics);
        }

        yield return null;

        foreach (var state in nodeStates.GetNodeStates())
        {
            FunctionNode node;
            if (!dynamicNodes.TryGetValue(state.NodeIndex, out node))
            {
                if (state.NodeTemplateId == 0)
                {
                    state.NodeTemplateId = NodeLegacyIdMapping.GetTemplateIdById(state.NodeId);
                }

                var template = m_Workspace.m_NodeTempList.GetTemplateByID(state.NodeTemplateId);
                if (!template)
                {
                    Debug.LogError("invalid template id: " + state.NodeTemplateId);
                    continue;
                }
                node = template.Clone(transform);
                node.gameObject.SetActive(true);
            }

            node.IsTemplate = false;

            linkMap.Add(state.NodeIndex, node);
            AddNode(node, updateIds);
            nodes.Add(node);

            node.LoadNodeSaveData(state, !updateIds);

            Hide(node, graphics);
            yield return null;
        }

        foreach (var node in unresolvedFuncNodes)
        {
            node.MoveTo(m_Workspace.NodePosGenerator.Generate(this));
        }

        var readonlyLinkMap = new ReadOnlyMap<int, FunctionNode>(linkMap);
        for (int i = 0; i < nodes.Count; ++i)
        {
            nodes[i].RelinkNodes(readonlyLinkMap);
        }

        foreach (var node in nodes)
        {
            node.PostLoad();
        }

        foreach (var node in nodes.Where(x => !x.ParentNode))
        {
            node.LayoutTopDown(false);
            yield return null;
        }

        int nextRenderOrder = NextNodeRenderOrder;
        foreach (var node in nodes.Where(x => x.IsFreeNode))
        {
            node.PositionSiblingsTopDown();
            nextRenderOrder = node.UpdateRenderOrder(nextRenderOrder);
        }

        foreach (var g in graphics)
        {
            g.enabled = true;
            yield return null;
        }
        yield return nodes;
    }

    private static void Hide(FunctionNode node, List<Graphic> graphics)
    {
        var nodeGraphics = node.GetComponentsInChildren<Graphic>();
        foreach (var g in nodeGraphics)
        {
            g.enabled = false;
        }
        graphics.AddRange(nodeGraphics);
    }

    private void LoadVariables(Save_ProjectData saveData)
    {
        foreach (var name in saveData.LocalVariables)
        {
            var variable = BaseVariable.createFrom(name, NameScope.Local);
            m_Workspace.CodeContext.variableManager.add(variable);
        }

        foreach (var name in saveData.GlobalVariables)
        {
            var variable = BaseVariable.createFrom(name, NameScope.Global);
            m_Workspace.CodeContext.variableManager.add(variable);
        }
    }

    public bool TryStart(MainNode node)
    {
        if (!m_Running)
        {
            return false;
        }

        if (m_MainNodes.Remove(node))
        {
            StopFinishCoroutine();
            PrepareToRun(node);
            return true;
        }

        return false;
    }

    /// <summary>
    /// The currently active simulation robot manager, physical or virtual
    /// </summary>
    public IRobotManager RobotManager
    {
        get
        {
            return m_Workspace.CodeContext.robotManager;
        }
    }

    public List<MainNode> GetMainNodes()
    {
        return m_MainNodes;
    }

    public IEnumerable<FunctionNode> Nodes
    {
        get { return m_Nodes; }
    }

    public IEnumerable<FunctionNode> FreeNodes
    {
        get { return Nodes.Where(x => x.IsFreeNode); }
    }

    public FunctionNode GetNode(int nodeIndex)
    {
        return m_Nodes.Find(x => x.NodeIndex == nodeIndex);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (OnClicked != null)
        {
            OnClicked(eventData);
        }
    }

    public FunctionDeclarationNode GetFunctionNode(Guid functionId)
    {
        return m_FuncNodes.Find(x => x.Declaration.functionId == functionId);
    }

    public IEnumerable<FunctionDeclarationNode> FunctionNodes
    {
        get { return m_FuncNodes; }
    }

    public Vector2 GetLocalPos(Vector2 relPosInViewport, bool excludingEdges)
    {
        var localPos = relPosInViewport;
        if (excludingEdges)
        {
            localPos += new Vector2(EdgeSize, -EdgeSize);
        }
        localPos -= (Vector2)transform.localPosition / transform.localScale.x;
        return localPos;
    }

    public int NextNodeRenderOrder
    {
        get { return m_Rect.childCount; }
    }

    public bool IsVisible
    {
        get { return m_Canvas.enabled; }
        set { m_Canvas.enabled = value; }
    }
}
