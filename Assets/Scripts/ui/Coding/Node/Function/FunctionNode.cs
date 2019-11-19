using DataAccess;
using Google.Protobuf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityEngine.Profiling;
using UnityEngine.Assertions;

public struct ConnectionId
{
    public static readonly ConnectionId invalid = new ConnectionId(-1, -1);

    public ConnectionId(int nodeId, int localId)
        : this()
    {
        this.nodeId = nodeId;
        this.localId = localId;
    }

    public int nodeId { get; private set; }

    public int localId { get; private set; }

    public bool isValid { get { return nodeId != -1 && localId != -1; } }
}

[Serializable]
public class Connection
{
    // the top level node this line belongs to
    public FunctionNode node { get; internal set; }

    [FormerlySerializedAs("m_Line")]
    public GameObject line;

    public ConnectionTypes type; // should have only 1 bit set

    [EnumFlags]
    public ConnectionTypes matchingTypes;

    public bool active
    {
        get { return line.gameObject.activeSelf; }
        set
        {
            if (value)
            {
                node.HighlightConnection(this);
            }
            else
            {
                node.UnhighlightConnection();
            }
        }
    }

    /// <summary>
    /// the id used to identify the connection in a block.
    /// only unique within a block
    /// </summary>
    public int id { get; internal set; }

    public Func<bool> sourceStateChecker { get; internal set; }

    /// <summary>
    /// true if the connection can be used as the matching source
    /// </summary>
    public bool enabledAsSource
    {
        get
        {
            return sourceStateChecker != null ? sourceStateChecker() : true;
        }
    }

    public Func<bool> targetStateChecker { get; internal set; }

    /// <summary>
    /// true if the connection is considered a matching target
    /// </summary>
    public bool enabledAsTarget
    {
        get
        {
            return targetStateChecker != null ? targetStateChecker() : true;
        }
    }

    public ConnectionId globalId
    {
        get { return new ConnectionId(node.NodeIndex, id); }
    }

    public bool IsMatched(Connection conn)
    {
        if (conn == null) { return false; }
        return (matchingTypes & conn.type) != 0;
    }
}

[Flags]
public enum ConnectionTypes
{
    Top           = 1 << 0,
    Bottom        = 1 << 1,
    RoundInsert   = 1 << 2, // use by InsertNode
    RoundSlot     = 1 << 3,
    CuspInsert    = 1 << 4, // use by InsertNode
    CuspSlot      = 1 << 5,
    SubTop        = 1 << 6,
    SubBottom     = 1 << 7,
    RectangleSlot = 1 << 8, // accept both RoundInsert and CuspInsert

    AllInserts = RoundInsert | CuspInsert,
}

public enum NodeColor
{
    Normal,
    Press,
    Drag,
    Play
}

public class UnPluggedNodeInfo
{
    public UnPluggedNodeInfo(FunctionNode node, Connection oldConn, Vector2 newPos)
    {
        this.node = node;
        this.oldConn = oldConn;
        this.newLocalPos = newPos;
    }

    public FunctionNode node;
    public Connection oldConn;
    public Vector2 newLocalPos;
}

public class FunctionNode
    : MonoBehaviour
    , IPointerDownHandler
    , IPointerUpHandler
    , IPointerClickHandler
    , IDragHandler
{
    private static readonly Color s_kNoEffect = new Color(0.44f, 0.44f, 0.44f, 1);
    private static readonly Color32 s_OpenedColor = new Color32(255, 0, 60, 153);
    private static readonly Color32 s_EnabledColor = new Color32(76, 255, 205, 153);
    private static readonly Color32 s_SelectedColor = new Color32(254, 215, 0, 153);

    private const float m_kClickDelay = 0.2f;

    public event Action<FunctionNode, PointerEventData> onPointerDown;
    public event Action<PointerEventData> onPointerUp;
    public event Action<PointerEventData> onPointerClick;
    public event Action<PointerEventData> onDrag;

    public Color m_Normal;
    public Color m_Pressed;
    public Color m_Playing;
    public Color m_DropDown;
    public float m_DefaultYOffset; // the relative offset from the rect center

    public GameObject[] m_Shadow;
    public Image[] m_frontImages;

    public GameObject m_Root;
    public GameObject[] m_LeaveMessageFlag;
    public int m_MinWidth;
    public float m_HeightAdjustment;

    protected int m_LastNodeIndexInSave = 0;
    protected int m_NextNodeIndexInSave = 0;
    protected int m_ParentIndexInSave = 0;

    [SerializeField] // for cloning
    [ReadOnly]
    private FunctionNode m_PrevNode;

    [SerializeField] // for cloning
    [ReadOnly]
    private FunctionNode m_NextNode;

    [SerializeField]
    [ReadOnly]
    private FunctionNode m_ParentNode;

    [ReadOnly]
    [SerializeField]
    private List<FunctionNode> m_Children = new List<FunctionNode>();

    [SerializeField]
    protected List<Connection> m_Connections = new List<Connection>();

    protected RectTransform m_RectTransform;

    Coroutine m_ClickCoroutine;

    private Connection m_HighlightedConnection;

    [SerializeField]
    private List<NodePluginsBase> m_Plugins = new List<NodePluginsBase>();
    private IList<NodePluginsBase> m_ReadOnlyPlugins;

    [SerializeField]
    private List<SlotPlugins> m_SlotPlugins = new List<SlotPlugins>();

    int m_MsgCount = 0;

    private bool m_Interactable = true;
    protected float m_OriginalHeight;

    private int m_nextConnId;
    private CodePanel m_CodePanel;
    private Connection m_TopConn;
    private Connection m_BottomConn;
    private LogicTransform m_LogicTransform;

    [Flags]
    enum NodeMessageStatus
    {
        None     = 0,
        // if set, display of message color will be enabled, 
        // otherwise Opened & Selected won't effect
        Enabled  = 1 << 0, 
        Opened   = 1 << 1,
        Selected = 1 << 2,
        All      = ~None,
    }

    private NodeMessageStatus m_messageStatus = NodeMessageStatus.None;

    protected virtual void Awake()
    {
        m_RectTransform = GetComponent<RectTransform>();
        m_OriginalHeight = m_RectTransform.rect.height;
        m_ReadOnlyPlugins = m_Plugins.AsReadOnly();
        m_LogicTransform = GetComponent<LogicTransform>();

        InitConnections();

        Draggable = true;

        foreach (var frontImage in m_frontImages)
        {
            frontImage.alphaHitTestMinimumThreshold = 0.5f;
        }

        foreach (var conn in m_Connections)
        {
            if ((conn.type & ConnectionTypes.AllInserts) != 0)
            {
                Insertable = true;
                break;
            }
        }
    }

    protected virtual void Start()
    {
    }

    // just in case someone uses the transform instead of RectTransform
    public new RectTransform transform
    {
        get { return RectTransform; }
    }

    // NOTE: DO NOT change the localPosition and position through the returned transform
    // Instead, you should change them through LogicTransform
    public RectTransform RectTransform
    {
        get
        {
            // sync the changes
            LogicTransform.UpdateTransform();
            return m_RectTransform;
        }
    }

    public bool Insertable
    {
        get;
        set;
    }

    public IList<Connection> Connections
    {
        get { return m_Connections; }
    }

    public Connection TopConn { get { return m_TopConn; } }
    public Connection BottomConn { get { return m_BottomConn; } }

    public Connection GetConnection(int id)
    {
        return m_Connections.Find(x => x.id == id);
    }

    /// <summary>
    /// return the first connection of the given type
    /// </summary>
    public Connection GetConnection(ConnectionTypes type)
    {
        return m_Connections.Find(x => x.type == type);
    }

    public Connection GetConnection(GameObject line)
    {
        return m_Connections.Find(x => x.line == line);
    }

    /// <summary>
    /// The previous connection used to connect this block to the target.
    /// For InsertNode, this is the connection of the parent slot,
    /// for other nodes, this is previous node's bottom or sub bottom connection
    /// </summary>
    public Connection GetPrevConnection()
    {
        if (ParentNode)
        {
            return ParentNode.GetPrevConnection(this);
        }
        else if (PrevNode)
        {
            return PrevNode.GetConnection(ConnectionTypes.Bottom);
        }
        else
        {
            return null;
        }
    }

    protected virtual Connection GetPrevConnection(FunctionNode childNode)
    {
        if (childNode.Insertable)
        {
            var slot = GetSlotPlugin(childNode);
            return GetConnection(slot.m_Line.gameObject);
        }
        else if (childNode.PrevNode)
        {
            return childNode.PrevNode.GetConnection(ConnectionTypes.Bottom);
        }
        else
        {
            return null;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!Interactable)
        {
            return;
        }

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            SendClickToNodePlugin(eventData.pointerPressRaycast.gameObject);
        }

        if (onPointerClick != null)
        {
            onPointerClick(eventData);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!m_Interactable)
        {
            return;
        }

        if (onPointerDown != null)
        {
            onPointerDown(this, eventData);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!m_Interactable)
        {
            return;
        }

        if (onDrag != null)
        {
            onDrag(eventData);
        }
    }

    public void MoveTo(Vector2 localPos)
    {
        LogicTransform.localPosition = localPos;
        PositionSiblingsTopDown();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!Interactable)
        {
            return;
        }

        if (onPointerUp != null)
        {
            onPointerUp(eventData);
        }
    }

    public void CancelNodePluginClick()
    {
        if (m_ClickCoroutine != null)
        {
            StopCoroutine(m_ClickCoroutine);
            m_ClickCoroutine = null;
        }
    }

    /// <summary>
    /// delete the current and all subsequent nodes
    /// </summary>
    public void ChainedDelete()
    {
        var cur = this;
        while (cur)
        {
            var next = cur.NextNode;
            cur.Delete(false);
            cur = next;
        }
    }

    /// <summary>
    /// delete the current block and all its children
    /// </summary>
    public void Delete(bool layout = true)
    {
        for (int i = m_Children.Count - 1; i >= 0; --i)
        {
            m_Children[i].Delete(false);
        }

        if (ParentNode)
        {
            ParentNode.RemoveChildren(this, this, layout);
        }
        else
        {
            var oldPrevNode = m_PrevNode;
            if (m_PrevNode)
            {
                m_PrevNode.NextNode = NextNode;
                m_PrevNode = null;
            }

            if (m_NextNode)
            {
                m_NextNode.PrevNode = oldPrevNode;
                m_NextNode = null;
            }

            if (oldPrevNode && layout)
            {
                oldPrevNode.PositionSiblingsTopDown();
            }
        }

        if (CodePanel)
        {
            CodePanel.RemoveNode(this);
        }

        Destroy(gameObject);
    }

    void SendClickToNodePlugin(GameObject go)
    {
        if (go && m_ClickCoroutine == null)
        {
            m_ClickCoroutine = StartCoroutine(FireClickImpl(go));
        }
    }

    IEnumerator FireClickImpl(GameObject go)
    {
        // no delay on desktop since right clicking is used for context menu
#if !UNITY_EDITOR && !UNITY_STANDALONE
        yield return new WaitForSeconds(m_kClickDelay);
#endif
        NodePluginsBase mPlugins = go.GetComponent<NodePluginsBase>();
        if (mPlugins)
        {
            mPlugins.Clicked();
        }

        m_ClickCoroutine = null;

        yield break;
    }

    protected void InitConnections()
    {
        for (int i = 0; i < m_Connections.Count; ++i)
        {
            // remove duplicate connections due to serialization
            for (int j = 0; j < m_SlotPlugins.Count; ++j)
            {
                if (m_SlotPlugins[j].m_Connection.line == m_Connections[i].line)
                {
                    m_Connections[i] = m_SlotPlugins[j].m_Connection;
                }
            }

            m_Connections[i].node = this;
            m_Connections[i].id = m_nextConnId++;
            m_Connections[i].line.SetActive(false);
            InitConnection(m_Connections[i]);
        }
        m_TopConn = GetConnection(ConnectionTypes.Top);
        m_BottomConn = GetConnection(ConnectionTypes.Bottom);
    }

    protected virtual void InitConnection(Connection conn)
    {
        switch (conn.type)
        {
        case ConnectionTypes.Top:
            conn.sourceStateChecker = IsTopConnectionEnabled;
            conn.targetStateChecker = IsTopConnectionEnabled;
            break;

        case ConnectionTypes.Bottom:
            conn.sourceStateChecker = IsBottomConnectionEnabled;
            break;
        }
    }

    protected bool IsTopConnectionEnabled()
    {
        return m_PrevNode == null;
    }

    protected bool IsBottomConnectionEnabled()
    {
        return m_NextNode == null;
    }

    public virtual bool CanConnect(FunctionNode node, Connection target)
    {
        if (node == null)
        {
            throw new ArgumentNullException("node");
        }
        if (target == null)
        {
            throw new ArgumentNullException("target");
        }
        if (target.node != this)
        {
            throw new ArgumentException("target");
        }

        if (target.type == ConnectionTypes.Top)
        {
            var lastNode = node.GetLastNode();
            if (!target.IsMatched(lastNode.m_BottomConn))
            {
                return false;
            }
            if (m_ParentNode)
            {
                return m_ParentNode.CanConnectChildTop(node, target);
            }
            else if (m_PrevNode)
            {
                return false;
            }
            return true;
        }
        else if (target.type == ConnectionTypes.Bottom)
        {
            if (!target.IsMatched(node.m_TopConn))
            {
                return false;
            }
            var lastNode = node.GetLastNode();
            if (m_NextNode && !m_NextNode.m_TopConn.IsMatched(lastNode.m_BottomConn))
            {
                return false;
            }
            return true;
        }
        else
        {
            if (node.Insertable)
            {
                if (GetSlot(target) && target.IsMatched(node.m_Connections[0]))
                {
                    return true;
                }
            }
            return false;
        }
    }

    // check if node is connectable to target, only need to check TopConnection of node
    protected virtual bool CanConnectChildTop(FunctionNode node, Connection target)
    {
        return false;
    }

    public Connection GetClosestMatchingConnection(IClosestConnectionFilter connFilter)
    {
        if (!CodePanel) { return null; }

        Connection closestMatchingConn = null;
        float minSqrDistance = float.MaxValue;

        foreach (var conn in m_Connections)
        {
            if (conn.enabledAsSource)
            {
                var result = CodePanel.FindClosestMatchingConnection(conn, connFilter);
                if (result.isValid && result.sqrDistance < minSqrDistance)
                {
                    minSqrDistance = result.sqrDistance;
                    closestMatchingConn = result.target;
                }
            }
        }

        return closestMatchingConn;
    }

    public void UnhighlightConnection()
    {
        if (m_HighlightedConnection != null)
        {
            m_HighlightedConnection.line.SetActive(false);
            m_HighlightedConnection = null;
        }
    }

    public void HighlightConnection(Connection target)
    {
        if (target != null && target.node != this)
        {
            throw new ArgumentException("target");
        }

        UnhighlightConnection();

        if (target != null)
        {
            target.line.SetActive(true);
            m_HighlightedConnection = target;
        }
    }

    public CodePanel CodePanel
    {
        get { return m_CodePanel; }
        internal set
        {
            m_CodePanel = value;
            if (value)
            {
                OnAddedToCodePanel();
            }
        }
    }

    protected virtual void OnAddedToCodePanel() { }

    /// <summary>
    /// connect the nodes headed by the given node to the target connection
    /// </summary>
    public virtual UnPluggedNodeInfo Connect(FunctionNode newNode, Connection target)
    {
        if (newNode == null)
        {
            throw new ArgumentNullException("newNode");
        }

        if (newNode.ParentNode)
        {
            throw new ArgumentException("newNode is already parented");
        }

        if (target == null)
        {
            throw new ArgumentNullException("conn");
        }

        if (target.node != this)
        {
            throw new ArgumentException("invalid connection");
        }

        if (target.type == ConnectionTypes.Top)
        {
            SetAsSiblings(newNode);

            if (PrevNode)
            {
                PrevNode.NextNode = newNode;
                newNode.PrevNode = PrevNode;
            }

            var lastNode = newNode.GetLastNode();
            lastNode.NextNode = this;
            PrevNode = lastNode;

            PositionSiblingsBottomUp();
            if (m_ParentNode)
            {
                m_ParentNode.LayoutBottomUp();
            }

            return null;
        }
        else if (target.type == ConnectionTypes.Bottom)
        {
            SetAsSiblings(newNode);

            if (NextNode)
            {
                var curEnd = newNode.GetLastNode();
                NextNode.PrevNode = curEnd;
                curEnd.NextNode = NextNode;
            }

            NextNode = newNode;
            newNode.PrevNode = this;

            PositionSiblingsTopDown();
            if (m_ParentNode)
            {
                m_ParentNode.LayoutBottomUp();
            }

            return null;
        }
        else
        {
            if (newNode.Insertable)
            {
                var slot = GetSlot(target);
                if (slot)
                {
                    newNode.ParentNode = this;

                    var oldInsert = slot.InsertedNode;
                    slot.RemoveInsertion();
                    slot.Insert(newNode);

                    UnPluggedNodeInfo unpluggedEvent = null;
                    if (oldInsert)
                    {
                        // TODO: should move the node to the right of the top most node
                        var oldInsertionPos = new Vector3(m_RectTransform.rect.width + 18, 0, 0);
                        oldInsert.LogicTransform.worldPosition = m_LogicTransform.worldPosition + oldInsertionPos;

                        unpluggedEvent = new UnPluggedNodeInfo(oldInsert, target, oldInsert.LogicTransform.worldPosition);
                    }
                    LayoutBottomUp();
                    return unpluggedEvent;
                }
            }

            return null;
        }
    }

    private void SetAsSiblings(FunctionNode node)
    {
        for (var cur = node; cur; cur = cur.NextNode)
        {
            cur.ParentNode = ParentNode;
            cur.LogicTransform.parent = m_LogicTransform.parent;
        }
    }

    /// <summary>
    /// disconnect nodes start with the current node and end with lastNode from the stack
    /// </summary>
    public void Disconnect(FunctionNode lastNode = null)
    {
        if (ParentNode)
        {
            if (lastNode != null && lastNode.ParentNode != ParentNode)
            {
                throw new ArgumentException("lastNode");
            }
            ParentNode.RemoveChildren(this, lastNode, true);
        }
        else
        {
            InternalDisconnect(lastNode, true);
        }
    }

    private void InternalDisconnect(FunctionNode lastNode, bool layout)
    {
        var prevNode = PrevNode;
        PrevNode = null;

        FunctionNode nextToLastNode = null;
        if (lastNode)
        {
            nextToLastNode = lastNode.NextNode;
            if (nextToLastNode)
            {
                nextToLastNode.PrevNode = prevNode;
            }

            lastNode.NextNode = null;
        }

        if (prevNode)
        {
            prevNode.NextNode = nextToLastNode;
        }

        for (var node = this; node; node = node.NextNode)
        {
            node.ParentNode = null;
        }

        SetParentTopDown(null);

        if (prevNode && layout)
        {
            prevNode.LayoutBottomUp();
        }
    }

    public BlockBehaviour BlockBehaviour { get; set; }

    public IEnumerator ActionBlock(ThreadContext context)
    {
        context.PushNode(this);
        if (BlockBehaviour)
        {
            yield return BlockBehaviour.ActionBlock(context);
        }
        context.PopNode();
    }

    public void SetColor(NodeColor color, bool recurse = true, bool updateSiblings = false)
    {
        SetColor(color);

        if (recurse)
        {
            foreach (var child in InnerChildren)
            {
                child.SetColor(color, true, false);
            }
        }

        if (updateSiblings)
        {
            for (var node = m_NextNode; node; node = node.NextNode)
            {
                node.SetColor(color, recurse, false);
            }
        }
    }

    protected virtual void SetColor(NodeColor color)
    {
        switch (color)
        {
        case NodeColor.Normal:
            SetFontImagesColor(m_Normal);
            break;

        case NodeColor.Play:
            SetFontImagesColor(m_Playing);
            break;

        case NodeColor.Press:
        case NodeColor.Drag:
            SetFontImagesColor(m_Pressed);
            break;
        }

        EnableShadows(color == NodeColor.Drag && ParentNode == null);

        foreach (var slot in m_SlotPlugins)
        {
            if (slot.InsertedNode)
            {
                slot.InsertedNode.SetColor(color);
            }
        }
    }

    protected virtual IEnumerable<FunctionNode> InnerChildren
    {
        get { return m_Children; }
    }

    private void SetFontImagesColor(Color color)
    {
        for (int i = 0; i < m_frontImages.Length; ++i)
        {
            m_frontImages[i].color = color;
        }
    }

    private void EnableShadows(bool on)
    {
        for (int i = 0; i < m_Shadow.Length; ++i)
        {
            m_Shadow[i].SetActive(on);
        }
    }

    /// <summary>
    /// set the parent of the current node and all the siblings below
    /// </summary>
    /// <param name="parent"></param>
    public void SetParentTopDown(LogicTransform parent)
    {
        var node = this;
        while (node)
        {
            node.LogicTransform.SetParent(parent, true);
            node = node.NextNode;
        }
    }

    public virtual void AddPlugins(NodePluginsBase plugin)
    {
        if (plugin == null)
        {
            throw new ArgumentNullException();
        }

        plugin.ParentNode = this;
        m_Plugins.Add(plugin);

        var slot = plugin as SlotPlugins;
        if (slot)
        {
            m_Connections.Add(slot.m_Connection);
            slot.m_Connection.id = m_nextConnId++;
            slot.m_Connection.node = this;
            m_SlotPlugins.Add(slot);

            if (CodePanel)
            {
                CodePanel.ConnectionRegistry.Register(slot.m_Connection);
            }
        }
    }

    protected void ResetConnectionIds()
    {
        m_nextConnId = 0;
        foreach (var conn in m_Connections)
        {
            conn.id = m_nextConnId++;
        }
    }

    public int SlotPluginsCount
    {
        get { return m_SlotPlugins.Count; }
    }

    public SlotPlugins GetSlotPlugin(int index)
    {
        if (index < 0 || index >= SlotPluginsCount)
        {
            throw new ArgumentOutOfRangeException("index");
        }

        return m_SlotPlugins[index];
    }

    public SlotPlugins GetSlotPlugin(FunctionNode node)
    {
        if (node == null)
        {
            throw new ArgumentNullException("node");
        }

        return m_SlotPlugins.Find(x => x.InsertedNode == node);
    }

    public IEnumerator GetSlotValues(ThreadContext context, List<string> retValue)
    {
        retValue.Clear();
        retValue.Capacity = m_SlotPlugins.Count;

        var value = new ValueWrapper<string>();
        foreach (var slot in m_SlotPlugins)
        {
            yield return slot.GetSlotValue(context, value);
            retValue.Add(value.value);
        }
    }

    public IEnumerator GetReturnValue(ThreadContext context, ValueWrapper<string> retValue)
    {
        if (BlockBehaviour)
        {
            yield return BlockBehaviour.GetNodeReturnValue(context, retValue);
        }
        else
        {
            retValue.value = "";
        }
    }

    protected void RemoveChildren(FunctionNode firstNode, FunctionNode lastNode, bool layout)
    {
        DoRemoveChildren(firstNode, lastNode ?? firstNode.GetLastNode());
        if (layout)
        {
            LayoutBottomUp();
        }
    }

    protected virtual void DoRemoveChildren(FunctionNode firstNode, FunctionNode lastNode)
    {
        if (firstNode.Insertable)
        {
            var slot = GetSlot(firstNode);
            if (slot)
            {
                slot.RemoveInsertion();
            }
        }
        else
        {
            firstNode.InternalDisconnect(lastNode, false);
        }
    }

    public SlotPlugins GetSlot(Connection conn)
    {
        if (conn == null)
        {
            throw new ArgumentNullException("conn");
        }

        return m_SlotPlugins.Find(x => x.m_Line.gameObject == conn.line);
    }

    public SlotPlugins GetSlot(FunctionNode insertion)
    {
        if (insertion == null)
        {
            throw new ArgumentNullException("insertion");
        }

        return m_SlotPlugins.Find(x => x.InsertedNode == insertion);
    }

    public void LayoutTopDown(bool positionSiblings = true)
    {
        LayoutTopDownInternal();

        if (!m_ParentNode && positionSiblings)
        {
            PositionSiblingsTopDown();
        }
    }

    private void LayoutTopDownInternal()
    {
        foreach (var child in m_Children)
        {
            child.LayoutTopDownInternal();
        }

        Layout();
    }

    public void LayoutBottomUp()
    {
        Layout();

        if (m_ParentNode)
        {
            m_ParentNode.LayoutBottomUp();
        }
        else
        {
            PositionSiblingsTopDown();
            UpdateRenderOrder(m_RectTransform.GetSiblingIndex(), true);
        }
    }

    public void PositionSiblingsTopDown()
    {
        var curNode = this;
        var nextNode = m_NextNode;
        while (nextNode)
        {
            var newPos = curNode.LogicTransform.localPosition;
            newPos.y -= nextNode.RectTransform.localScale.y * (curNode.RectTransform.rect.height + curNode.m_HeightAdjustment);
            nextNode.LogicTransform.localPosition = newPos;

            curNode = nextNode;
            nextNode = nextNode.NextNode;
        }
    }

    public void PositionSiblingsBottomUp()
    {
        var curNode = this;
        var prevNode = PrevNodeInStack;
        while (prevNode)
        {
            var newPos = curNode.LogicTransform.localPosition;
            newPos.y += prevNode.RectTransform.localScale.y * (prevNode.RectTransform.rect.height + prevNode.m_HeightAdjustment);
            prevNode.LogicTransform.localPosition = newPos;

            curNode = prevNode;
            prevNode = prevNode.PrevNodeInStack;
        }
    }

    public virtual void Layout()
    {
        Profiler.BeginSample("FunctionNode.Layout");

        var size = NodeLayoutUtil.Layout(m_RectTransform, m_Plugins, m_DefaultYOffset);
        size.y = Mathf.Max(size.y - m_HeightAdjustment, m_OriginalHeight);
        size.x = Mathf.Max(m_MinWidth, size.x);
        m_RectTransform.sizeDelta = size;

        Profiler.EndSample();
    }

    public void RefreshPluginText()
    {
        for (int i = 0; i < m_Plugins.Count; ++i)
        {
            var plugin = m_Plugins[i];
            plugin.SetPluginsText(plugin.GetPluginsText());
        }
    }

    public bool IsFreeNode
    {
        get
        {
            return !m_PrevNode && !m_ParentNode;
        }
    }

    public NodeCategory NodeCategory
    {
        get;
        set;
    }

    public int NodeTemplateId
    {
        get;
        set;
    }

    // whether the template has state to save
    public bool TemplateHasState
    {
        get;
        set;
    }

    public bool IsTransient
    {
        get;
        set;
    }

    public int NodeIndex
    {
        get;
        set;
    }

    public NodePluginsBase GetPluginById(int pluginId)
    {
        return m_Plugins.Find(x => x.PluginID == pluginId);
    }

    public IList<NodePluginsBase> Plugins
    {
        get { return m_ReadOnlyPlugins; }
    }

    public void RemovePlugin(NodePluginsBase plugin)
    {
        var index = m_Plugins.IndexOf(plugin);
        if (index != -1)
        {
            RemovePluginAt(index);
        }
    }

    public void RemovePluginAt(int index)
    {
        RemovePluginAt(index, true);
    }

    protected void RemovePluginAt(int index, bool removeInsertion)
    {
        var plugin = m_Plugins[index];
        plugin.ParentNode = null;
        m_Plugins.RemoveAt(index);
        var slot = plugin as SlotPlugins;
        if (slot)
        {
            if (CodePanel)
            {
                CodePanel.ConnectionRegistry.Unregister(slot.m_Connection);
            }

            m_SlotPlugins.Remove(slot);
            if (removeInsertion)
            {
                slot.RemoveInsertion();
            }
            m_Connections.Remove(slot.m_Connection);
        }
    }

    public void RemovePlugins()
    {
        for (int i = m_Plugins.Count - 1; i >= 0; --i)
        {
            RemovePluginAt(i);
        }
    }

    public Save_NodeData GetNodeSaveData()
    {
        Save_NodeData nodeSaveData = new Save_NodeData();
        // old app uses node id to instantiate node
        // a template id does not have a unique id, but any corresponding node id should work
        nodeSaveData.NodeId = NodeLegacyIdMapping.GetIdByTemplateId(NodeTemplateId);
        nodeSaveData.NodeTemplateId = NodeTemplateId;
        nodeSaveData.NodeIndex = NodeIndex;
        nodeSaveData.NodeData = PackNodeSaveData().ToByteString();
        return nodeSaveData;
    }

    protected Save_NodeBaseData PackBaseNodeSaveData()
    {
        Save_NodeBaseData tNodeData = new Save_NodeBaseData();
        tNodeData.PosX = m_RectTransform.localPosition.x;
        tNodeData.PosY = m_RectTransform.localPosition.y;

        tNodeData.LastNodeIndex = GetPrevNodeIndex();
        if (m_NextNode)
        {
            tNodeData.NextNodeIndex = m_NextNode.NodeIndex;
        }
        else
        {
            tNodeData.NextNodeIndex = -1;
        }

        for (int i = 0; i < m_Plugins.Count; ++i)
        {
            tNodeData.PluginList.Add(m_Plugins[i].GetPluginSaveData());
        }

        return tNodeData;
    }

    private int GetPrevNodeIndex()
    {
        if (m_PrevNode) { return m_PrevNode.NodeIndex; }

        return -1;
    }

    protected virtual IMessage PackNodeSaveData()
    {
        var nodeData = new Save_FunctionNode();
        nodeData.BaseData = PackBaseNodeSaveData();
        return nodeData;
    }

    public void LoadNodeSaveData(Save_NodeData save, bool loadNodeIndex = true)
    {
        NodeTemplateId = save.NodeTemplateId;
        if (loadNodeIndex)
        {
            NodeIndex = save.NodeIndex;
        }
        UnPackNodeSaveData(save.NodeData.ToByteArray());
    }

    protected void UnPackBaseNodeSaveData(Save_NodeBaseData baseData)
    {
        Vector3 tSavePos = new Vector3(baseData.PosX, baseData.PosY, 0.0f);
        LogicTransform.localPosition = tSavePos;
        m_LastNodeIndexInSave = baseData.LastNodeIndex;
        m_NextNodeIndexInSave = baseData.NextNodeIndex;

        for (int saveIndex = 0; saveIndex < baseData.PluginList.Count; ++saveIndex)
        {
            Save_PluginsData tCurPluginSaveData = baseData.PluginList[saveIndex];
            for (int pluginIndex = 0; pluginIndex < m_Plugins.Count; ++pluginIndex)
            {
                NodePluginsBase tCurPlugin = m_Plugins[pluginIndex];
                if (tCurPlugin.PluginID == tCurPluginSaveData.PluginId)
                {
                    tCurPlugin.LoadPluginSaveData(tCurPluginSaveData);
                    break;
                }
            }
        }
    }

    protected virtual void UnPackNodeSaveData(byte[] nodeData)
    {
        Save_FunctionNode tBaseNode = Save_FunctionNode.Parser.ParseFrom(nodeData);
        UnPackBaseNodeSaveData(tBaseNode.BaseData);
        m_ParentIndexInSave = tBaseNode.ParentNodeIndex;
    }

    protected int GetParentNodeIndex()
    {
        if (Insertable)
        {
            return (null == ParentNode ? 0 : ParentNode.NodeIndex);
        }
        return 0;
    }

    public Transform GetPluginRoot()
    {
        return m_Root.transform;
    }

    public void EnableMessageStatus(bool on)
    {
        UpdateMessageFlag(NodeMessageStatus.Enabled, on);
    }

    public void ClearMessageStatus()
    {
        UpdateMessageFlag(NodeMessageStatus.All, false);
    }

    public void SetMessageOpened(bool on)
    {
        UpdateMessageFlag(NodeMessageStatus.Opened, on);
    }

    public void SetMessageSelected(bool on)
    {
        UpdateMessageFlag(NodeMessageStatus.Selected, on);
    }

    public void AddLeaveMessageCount()
    {
        ++m_MsgCount;
        if (m_MsgCount == 1)
        {
            UpdateMessageFlag(NodeMessageStatus.None, true);
        }
    }

    public void SubLeaveMessageCount()
    {
        if (m_MsgCount == 0)
        {
            throw new InvalidOperationException();
        }
        --m_MsgCount;
        if (m_MsgCount == 0)
        {
            UpdateMessageFlag(NodeMessageStatus.None, true);
        }
    }

    private void UpdateMessageFlag(NodeMessageStatus flag, bool set)
    {
        if (set)
        {
            m_messageStatus |= flag;
        }
        else
        {
            m_messageStatus &= ~flag;
        }

        var flagEnabled = (m_messageStatus & NodeMessageStatus.Enabled) != 0;
        Color32 color = Color.white;
        if ((m_messageStatus & NodeMessageStatus.Selected) != 0)
        {
            color = s_SelectedColor;
        }
        else if ((m_messageStatus & NodeMessageStatus.Opened) != 0)
        {
            color = s_OpenedColor;
        }
        else if (m_MsgCount != 0)
        {
            color = s_EnabledColor;
        }
        else
        {
            // if no message, no need to show the flag
            flagEnabled = false;
        }

        foreach (var msgFlag in m_LeaveMessageFlag)
        {
            msgFlag.SetActive(flagEnabled);
            if (msgFlag.gameObject.activeSelf)
            {
                msgFlag.GetComponent<Image>().color = color;
            }
        }
    }

    /// <summary>
    /// Previous node in the block chain
    /// </summary>
    public FunctionNode PrevNode
    {
        get { return m_PrevNode; }
        internal set { m_PrevNode = value; }
    }

    /// <summary>
    /// Previous node in the same stack.
    /// A stack is a chain of nodes with equal or greater depth.
    /// Nodes nested in a StepNode have greater depth.
    /// </summary>
    public FunctionNode PrevNodeInStack
    {
        get
        {
            if (!PrevNode) { return null; }
            return PrevNode.NextNode == this ? PrevNode : null;
        }
    }

    /// <summary>
    /// Next node in the block chain
    /// </summary>
    public FunctionNode NextNode
    {
        get { return m_NextNode; }
        internal set { m_NextNode = value; }
    }

    public FunctionNode ParentNode
    {
        get { return m_ParentNode; }
        internal set
        {
            if (m_ParentNode != value)
            {
                if (m_ParentNode)
                {
                    m_ParentNode.m_Children.Remove(this);
                }
                m_ParentNode = value;
                if (m_ParentNode)
                {
                    m_ParentNode.m_Children.Add(this);
                }
            }
        }
    }

    public IList<FunctionNode> Children
    {
        get { return m_Children; }
    }

    /// <summary>
    /// get all descendants including the current node
    /// </summary>
    /// <returns></returns>
    public IEnumerable<FunctionNode> GetDescendants()
    {
        yield return this;

        foreach (var child in m_Children)
        {
            foreach (var grandChild in child.GetDescendants())
            {
                yield return grandChild;
            }
        }
    }

    public FunctionNode GetRootNode()
    {
        var root = this;
        var parent = m_ParentNode;
        while (parent)
        {
            root = parent;
            parent = parent.m_ParentNode;
        }
        return root;
    }

    /// <summary>
    /// First node in the stack
    /// </summary>
    public FunctionNode GetFirstNode()
    {
        var curNode = GetRootNode();
        while (curNode.PrevNode)
        {
            curNode = curNode.PrevNode;
        }
        return curNode;
    }

    /// <summary>
    /// Last node in the stack
    /// </summary>
    public FunctionNode GetLastNode()
    {
        FunctionNode curNode = this;
        while (curNode.NextNode)
        {
            curNode = curNode.NextNode;
        }
        return curNode;
    }

    public virtual void RelinkNodes(ReadOnlyMap<int, FunctionNode> linkMap)
    {
        if (0 != m_LastNodeIndexInSave)
        {
            linkMap.TryGetValue(m_LastNodeIndexInSave, out m_PrevNode);
        }
        if (0 != m_NextNodeIndexInSave)
        {
            linkMap.TryGetValue(m_NextNodeIndexInSave, out m_NextNode);
        }
        LinkParentNodeIfInsertable(linkMap);

        for (int i = 0; i < m_SlotPlugins.Count; ++i)
        {
            m_SlotPlugins[i].RelinkNodes(linkMap);
        }
    }

    protected void LinkParentNodeIfInsertable(ReadOnlyMap<int, FunctionNode> linkMap)
    {
        if (Insertable && 0 != m_ParentIndexInSave)
        {
            FunctionNode parentNode;
            if (linkMap.TryGetValue(m_ParentIndexInSave, out parentNode))
            {
                ParentNode = parentNode;
            }
        }
    }

    public virtual void PostLoad() { }

    protected static FunctionNode GetNode(List<FunctionNode> nodeList, int nodeIndex)
    {
        for (int i = 0; i < nodeList.Count; ++i)
        {
            if (nodeIndex == nodeList[i].NodeIndex)
            {
                return nodeList[i];
            }
        }
        return null;
    }

    /// <summary>
    /// clone current node only
    /// </summary>
    /// <param name="node"></param>
    public virtual FunctionNode Clone(Transform parent = null)
    {
        if (!parent)
        {
            parent = CodePanel ? CodePanel.transform : transform.parent;
        }
        var go = LogicTransformUtils.Instantitate(m_LogicTransform, parent);
        // prevent the name from growing too long
        go.name = gameObject.name;
        var clone = go.GetComponent<FunctionNode>();
        clone.PrevNode = null;
        clone.NextNode = null;
        clone.ParentNode = null;
        clone.PostClone(this);
        return clone;
    }

    /// <summary>
    /// Clone all nodes headed by `node'
    /// </summary>
    /// <param name="node"></param>
    /// <returns>new head node</returns>
    public FunctionNode ChainedClone(Transform parent = null)
    {
        var headNode = Clone(parent);

        // clone all subsequent nodes
        var prevClonedNode = headNode;
        var nextNode = NextNode;
        for (; nextNode; nextNode = nextNode.NextNode)
        {
            var clone = nextNode.Clone(parent);
            clone.PrevNode = prevClonedNode;
            prevClonedNode.NextNode = clone;
            prevClonedNode = clone;
        }

        return headNode;
    }

    /// <summary>
    /// copy runtime states, etc
    /// </summary>
    /// <param name="other"></param>
    internal virtual void PostClone(FunctionNode other)
    {
        IsTemplate = other.IsTemplate;
        NodeCategory = other.NodeCategory;
        NodeTemplateId = other.NodeTemplateId;
        IsTransient = other.IsTransient;
        CodeContext = other.CodeContext;
        m_OriginalHeight = other.m_OriginalHeight;

        foreach (var pair in m_Plugins.Zip(other.m_Plugins))
        {
            var myPlugin = pair.first.GetComponent<NodePluginsBase>();
            var otherPlugin = pair.second.GetComponent<NodePluginsBase>();
            myPlugin.PostClone(otherPlugin);
        }

        var otherSlots = other.m_SlotPlugins;
        Assert.AreEqual(m_SlotPlugins.Count, otherSlots.Count);

        foreach (var pair in m_SlotPlugins.Zip(otherSlots))
        {
            Assert.AreEqual(pair.first.InsertedNode != null, pair.second.InsertedNode != null);

            if (pair.first.InsertedNode)
            {
                pair.first.InsertedNode.PostClone(pair.second.InsertedNode);
            }
        }

        if (CodePanel)
        {
            CodePanel.AddNode(this);
        }
    }

    public bool IsTemplate
    {
        get;
        set;
    }

    public bool Interactable
    {
        get { return m_Interactable; }
        set
        {
            m_Interactable = value;
            var color = value ? m_Normal : s_kNoEffect;
            for (int i = 0; i < m_frontImages.Length; ++i)
            {
                if (m_frontImages[i] != null)
                {
                    m_frontImages[i].color = color;
                }
            }
        }
    }

    public bool Draggable
    {
        get;
        set;
    }

    public CodeContext CodeContext { get; set; }

    public LogicTransform LogicTransform { get { return m_LogicTransform; } }

    public int RenderOrder { get { return m_RectTransform.GetSiblingIndex(); } }

    /// <summary>
    /// update the render order for the node and all its siblings optionally
    /// </summary>
	/// <return>the next render order</return>
    public int UpdateRenderOrder(int renderOrder, bool updateSiblings = true)
    {
        renderOrder = DoUpdateRenderOrder(renderOrder);
        if (updateSiblings)
        {
            for (var nextNode = NextNode; nextNode; nextNode = nextNode.NextNode)
            {
                renderOrder = nextNode.DoUpdateRenderOrder(renderOrder);
            }
        }
        return renderOrder;
    }

    protected int DoUpdateRenderOrder(int renderOrder)
    {
        RectTransform.SetSiblingIndex(renderOrder);
        ++renderOrder;
        foreach (var child in m_Children)
        {
            renderOrder = child.DoUpdateRenderOrder(renderOrder);
        }
        return renderOrder;
    }
}
