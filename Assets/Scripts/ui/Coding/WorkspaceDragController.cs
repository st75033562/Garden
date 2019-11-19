using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WorkspaceDragController : MonoBehaviour
{
    public event Action<FunctionNode, NodeDropResult> OnEndDrag;

    public UIWorkspace m_Workspace;
    public NodeTemplateList m_NodeTempList;
    public ScrollRect m_CodePanelScrollRect;

    public RectTransform m_TrashArea;
    public RectTransform m_LeftTopTrans;
    public RectTransform m_RightTopTrans;

    const float s_kPressTime = 0.1f;
    const float s_kPickTime = 0.2f;
    // squared distance threshold
    // a move command is generated only when the actual movement distance is larger than the threshold
    const float s_kSqrMoveDistThreshold = 1.0f;

    private FunctionNode m_pressedNode;
    private FunctionNode m_draggingNode;
    private FunctionNode m_draggingTailNode;
    private Vector2 m_oldLocalPos;
    private Connection m_oldConnection;
    private Connection m_dropTarget;

    private Coroutine m_coStartPress;
    private Coroutine m_coPrepareDrag;

    private Vector2 m_pointerOffset;
    private Vector2 m_originalNodePos;
    private Vector3 m_lastInputPosition;
    private PointerEventData m_dragEventData;
    private bool m_dragFromTemplate;

    private MoveNodeCommandArgs m_moveNodeArgs;
    private DeleteNodeCommand m_deleteCommand;
    private AddNodeCommand m_addNodeCommand;

    public bool IsDragging
    {
        get { return m_pressedNode != null || m_draggingNode != null; }
    }

    void Update()
    {
        if (m_draggingNode != null && m_lastInputPosition != Input.mousePosition)
        {
            m_lastInputPosition = Input.mousePosition;
            m_dragEventData.position = Input.mousePosition;
            DragNode(m_dragEventData);
        }
    }

    public bool OnFunctionNodePointerDown(FunctionNode node, PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left && !IsDragging)
        {
            Assert.IsNull(m_pressedNode);
            Assert.IsTrue(!m_draggingNode && m_coPrepareDrag == null);

            if (!node.Interactable)
            {
                Debug.Log("node is not movable");
                return true;
            }

            if (node.Draggable)
            {
                m_pointerOffset = node.transform.position.xy() - eventData.position;
                m_pressedNode = node;
                m_originalNodePos = node.transform.position;
                m_lastInputPosition = eventData.position;

                //Debug.Log("pointer down: " + m_originalNodePos);

                m_coStartPress = StartCoroutine(StartPress());
                m_coPrepareDrag = StartCoroutine(PrepareDrag(eventData));
            }
            else
            {
                // for OnFunctionNodeDrag to work
                m_pressedNode = node;
            }

            return true;
        }

        return false;
    }

    IEnumerator StartPress()
    {
        yield return new WaitForSeconds(s_kPressTime);
        m_pressedNode.SetColor(NodeColor.Press, true, true);
    }

    IEnumerator PrepareDrag(PointerEventData eventData)
    {
        yield return new WaitForSeconds(s_kPickTime);

        InternalBeginDrag(m_pressedNode);
        m_coPrepareDrag = null;
        m_pressedNode = null;

        eventData.eligibleForClick = false;
    }

    public bool OnFunctionNodePointerUp(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (m_draggingNode == null)
            {
                CancelDrag();
            }
            else
            {
                if (eventData.pointerPress.gameObject == m_draggingNode.gameObject)
                {
                    eventData.eligibleForClick = false;
                }
                EndDrag();
            }

            return true;
        }

        return false;
    }

    public bool OnFunctionNodePointerClick(PointerEventData eventData)
    {
        if (m_draggingNode && eventData.button == PointerEventData.InputButton.Right)
        {
            CancelDrag();
            return true;
        }

        return false;
    }

    public void OnFunctionNodeDrag(PointerEventData eventData)
    {
        if (!m_draggingNode && m_pressedNode)
        {
            if (m_pressedNode.IsTemplate && m_pressedNode.NodeIndex == 0)
            {
                m_NodeTempList.DragScroll(eventData);
            }
            else
            {
                InitializeCodePanelScroll(eventData);
            }

            CancelDrag();
        }
    }

    void InitializeCodePanelScroll(PointerEventData eventData)
    {
        // this make sure that all events are sent to the scroll rect
        eventData.pointerEnter    = m_CodePanelScrollRect.gameObject;
        eventData.pointerPress    = m_CodePanelScrollRect.gameObject;
        eventData.rawPointerPress = m_CodePanelScrollRect.gameObject;
        eventData.pointerDrag     = m_CodePanelScrollRect.gameObject;
        m_CodePanelScrollRect.OnInitializePotentialDrag(eventData);
        m_CodePanelScrollRect.OnBeginDrag(eventData);
    }

    private void InternalBeginDrag(FunctionNode sourceNode)
    {
        if (sourceNode.IsTemplate)
        {
            sourceNode.SetColor(NodeColor.Normal, true, true);

            // a template in code panel
            var saveState = sourceNode.TemplateHasState ? sourceNode.GetNodeSaveData() : null;
            m_addNodeCommand = new AddNodeCommand(m_Workspace, sourceNode.NodeTemplateId, saveState);
            m_addNodeCommand.recalculateContent = false;
            m_addNodeCommand.localPos = m_Workspace.CodePanel.transform.WorldToLocal(sourceNode.transform.position);
            m_addNodeCommand.Redo();

            m_draggingNode = m_addNodeCommand.clone;

            m_NodeTempList.ScrollEnabled = false;
            m_dragFromTemplate = true;
        }
        else
        {
            m_draggingNode = sourceNode;
            m_TrashArea.gameObject.SetActive(true);
            InitMoveNodeEventArgs();
        }

        m_deleteCommand = new DeleteNodeCommand(m_Workspace, m_draggingNode);

        m_oldLocalPos = NodeUtils.GetPanelLocalPos(m_draggingNode);
        m_oldConnection = m_draggingNode.GetPrevConnection();

        m_draggingNode.Disconnect();
        m_draggingNode.SetColor(NodeColor.Drag, true, true);
        m_Workspace.CodeContext.eventBus.AddEvent(EventId.PickUpNode, this);

        NodeUtils.SetParent(m_draggingNode, transform);
        m_draggingNode.MoveTo(m_draggingNode.LogicTransform.localPosition);

        m_draggingTailNode = m_draggingNode.GetLastNode();
        CheckDropTarget();

        if (m_dragEventData == null)
        {
            m_dragEventData = new PointerEventData(EventSystem.current);
        }
        m_dragEventData.position = m_lastInputPosition;

        m_Workspace.UndoManager.undoEnabled = false;
    }

    private void InitMoveNodeEventArgs()
    {
        m_moveNodeArgs = new MoveNodeCommandArgs();
        m_moveNodeArgs.firstNodeId = m_draggingNode.NodeIndex;
        m_moveNodeArgs.lastNodeId = m_draggingNode.GetLastNode().NodeIndex;
        m_moveNodeArgs.oldRenderOrder = m_draggingNode.RenderOrder;
        m_moveNodeArgs.SetOldConnection(m_draggingNode.GetPrevConnection());
        m_moveNodeArgs.oldLocalPos = NodeUtils.GetPanelLocalPos(m_draggingNode);
    }

    public void BeginDrag(FunctionNode node)
    {
        if (IsDragging)
        {
            throw new InvalidOperationException();
        }

        if (!node.Interactable || !node.Draggable)
        {
            Debug.Log("node is not movable");
            return;
        }

        m_lastInputPosition = Input.mousePosition;
        m_pointerOffset = (node.transform.position - m_lastInputPosition).xy();
        m_originalNodePos = node.transform.position;
        //Debug.Log("begin drag: " + m_originalNodePos);

        InternalBeginDrag(node);
    }

    public void EndDrag()
    {
        if (m_draggingNode != null)
        {
            //Debug.Log("end dragging");

            var dropResult = ClassifyDrop(m_dragEventData.position);
            if (dropResult == NodeDropResult.Success)
            {
                DropDraggingNode();
            }
            else if (dropResult == NodeDropResult.Cancel)
            {
                CancelDrag();
                return;
            }
            else
            {
                // make sure all of the call nodes have been deleted
                if (m_draggingNode is FunctionDeclarationNode)
                {
                    var declNode = m_draggingNode as FunctionDeclarationNode;
                    var numExternalCallNodes = m_Workspace.CodePanel.Nodes.Where(x => {
                        var callNode = x as FunctionCallNode;
                        return callNode && callNode.Declaration == declNode.Declaration &&
                            callNode.GetFirstNode() != declNode;
                    }).Count();

                    if (numExternalCallNodes > 0)
                    {
                        PopupManager.Notice("ui_error_delete_call_nodes_first".Localize());
                        CancelDrag();
                        return;
                    }
                }

                DeleteDraggingNode();
            }

            CleanupDragState();
        }
    }

    private NodeDropResult ClassifyDrop(Vector2 position)
    {
        if (RectTransformUtility.RectangleContainsScreenPoint(m_TrashArea, position))
        {
            return NodeDropResult.Delete;
        }

        if (RectTransformUtility.RectangleContainsScreenPoint(
            m_CodePanelScrollRect.GetComponent<RectTransform>(), position))
        {
            return NodeDropResult.Success;
        }

        if (RectTransformUtility.RectangleContainsScreenPoint(m_RightTopTrans, position))
        {
            return NodeDropResult.Success;
        }

        if (!m_NodeTempList.gameObject.activeSelf && 
            RectTransformUtility.RectangleContainsScreenPoint(m_LeftTopTrans, position))
        {
            return NodeDropResult.Success;
        }

        return NodeDropResult.Cancel;
    }

    void DropDraggingNode()
    {
        CheckDropTarget();
        
        var curLocalPos = NodeUtils.GetPanelLocalPos(m_draggingNode);
        // only record the action when the block is moved
        if (m_oldConnection != m_dropTarget ||
            m_oldConnection == null && (m_oldLocalPos - curLocalPos).sqrMagnitude > s_kSqrMoveDistThreshold)
        {
            ResetDraggingNodeState();

            var undoManager = m_Workspace.UndoManager;

            if (m_addNodeCommand != null)
            {
                undoManager.BeginMacro();
                m_addNodeCommand.localPos = curLocalPos;
                m_addNodeCommand.contentOffset = m_Workspace.CodePanel.RecalculateContentSize();
                m_addNodeCommand.recalculateContent = true;
                undoManager.AddUndo(m_addNodeCommand, false);

                InitMoveNodeEventArgs();
            }

            m_moveNodeArgs.SetNewConnection(m_dropTarget);
            m_moveNodeArgs.newLocalPos = NodeUtils.GetPanelLocalPos(m_draggingNode);

            undoManager.AddUndo(new MoveNodeCommand(m_Workspace, m_moveNodeArgs));

            if (m_addNodeCommand != null)
            {
                undoManager.EndMacro();
            }

            if (OnEndDrag != null)
            {
                OnEndDrag(m_draggingNode, NodeDropResult.Success);
            }
        }
        else
        {
            CancelDrag();
        }
    }

    void DeleteDraggingNode()
    {
        if (!m_dragFromTemplate)
        {
            var undoManager = m_Workspace.UndoManager;

            undoManager.BeginMacro();
            foreach (var node in NodeUtils.GetDescendants(m_draggingNode, m_draggingNode.GetLastNode()))
            {
                var cmd = new DeleteNodeMessagesCommand(m_Workspace.m_MessagePanel, node.NodeIndex);
                undoManager.AddUndo(cmd);
            }

            undoManager.AddUndo(m_deleteCommand);
            undoManager.EndMacro();
        }
        else
        {
            CancelDrag();
        }

        m_moveNodeArgs = null;

        if (OnEndDrag != null)
        {
            OnEndDrag(m_draggingNode, NodeDropResult.Delete);
        }
    }

    public void CancelDrag()
    {
        if (m_coStartPress != null)
        {
            StopCoroutine(m_coStartPress);
            m_coStartPress = null;
        }

        if (m_coPrepareDrag != null)
        {
            StopCoroutine(m_coPrepareDrag);
            m_coPrepareDrag = null;
        }

        if (m_draggingNode)
        {
            if (m_dragFromTemplate)
            {
                m_addNodeCommand.Undo();
            }
            else
            {
                Debug.Log("cancel drag: " + m_originalNodePos);

                ResetDraggingNodeState();
                new MoveNodeCommand(m_Workspace, m_moveNodeArgs).Undo();
            }

            if (OnEndDrag != null)
            {
                OnEndDrag(m_draggingNode, NodeDropResult.Cancel);
            }
        }
        else if (m_pressedNode != null)
        {
            m_pressedNode.SetColor(NodeColor.Normal, true, true);
            m_pressedNode = null;
        }

        CleanupDragState();
    }

    void ResetDraggingNodeState()
    {
        m_draggingNode.SetColor(NodeColor.Normal, true, true);
        NodeUtils.SetParent(m_draggingNode, m_Workspace.CodePanel.transform);
        m_Workspace.m_MessagePanel.SetLayoutDirty();
    }

    void CleanupDragState()
    {
        m_NodeTempList.ScrollEnabled = true;
        m_TrashArea.gameObject.SetActive(false);
        m_dragFromTemplate = false;
        m_draggingNode = null;
        m_draggingTailNode = null;
        m_oldConnection = null;

        m_moveNodeArgs = null;
        m_deleteCommand = null;
        m_addNodeCommand = null;

        ClearDropTarget();

        m_Workspace.UndoManager.undoEnabled = true;
    }

    void DragNode(PointerEventData eventData)
    {
        // calculate the logic world position in the visual parent
        var logicWorldPos = transform.InverseTransformPoint(eventData.position + m_pointerOffset);
        m_draggingNode.MoveTo(logicWorldPos);
        if (RectTransformUtility.RectangleContainsScreenPoint(m_TrashArea, eventData.position))
        {
            ClearDropTarget();
        }
        else
        {
            CheckDropTarget();
        }
    }

    void ClearDropTarget()
    {
        if (m_dropTarget != null)
        {
            m_dropTarget.active = false;
            m_dropTarget = null;
        }
    }

    void CheckDropTarget()
    {
        var newDropTarget = m_draggingNode.GetClosestMatchingConnection(m_Workspace.ConnectionFilter);
        newDropTarget = ValidateDropTarget(newDropTarget);

        if (newDropTarget == null && m_draggingNode != m_draggingTailNode)
        {
            newDropTarget = m_draggingTailNode.GetClosestMatchingConnection(m_Workspace.ConnectionFilter);
            newDropTarget = ValidateDropTarget(newDropTarget);
        }

        if (newDropTarget != m_dropTarget)
        {
            if (m_dropTarget != null)
            {
                m_dropTarget.active = false;
            }
            m_dropTarget = newDropTarget;
            if (m_dropTarget != null)
            {
                m_dropTarget.active = true;
            }
        }
    }

    Connection ValidateDropTarget(Connection target)
    {
        if (target != null && !target.node.CanConnect(m_draggingNode, target))
        {
            target = null;
        }
        return target;
    }
}
