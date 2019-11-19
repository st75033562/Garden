using System;
using UnityEngine;

public class MoveNodeCommandArgs
{
    public int firstNodeId;
    public int lastNodeId = -1;
    public int oldRenderOrder = -1;

    public Vector2 oldLocalPos;
    public ConnectionId oldConnId = ConnectionId.invalid;

    public void SetOldConnection(Connection conn)
    {
        oldConnId = conn != null ? conn.globalId : ConnectionId.invalid;
    }

    public Vector2 newLocalPos;
    public ConnectionId newConnId = ConnectionId.invalid;

    public void SetNewConnection(Connection conn)
    {
        newConnId = conn != null ? conn.globalId : ConnectionId.invalid;
    }
}

/// <summary>
/// represent a node move operation, should be created before moving
/// </summary>
public class MoveNodeCommand : BaseUndoCommand
{
    private readonly UIWorkspace m_workspace;
    private readonly MoveNodeCommandArgs m_args;
    private MoveNodeCommandArgs m_unplugNodeArgs;
    private Vector2 m_contentOffset;

    public MoveNodeCommand(UIWorkspace workspace, MoveNodeCommandArgs args)
        : base(true)
    {
        if (workspace == null)
        {
            throw new ArgumentNullException("workspace");
        }

        if (args == null)
        {
            throw new ArgumentNullException("args");
        }

        m_workspace = workspace;
        m_args = args;
    }

    protected override void UndoImpl()
    {
        Move(m_args, m_contentOffset, true);
        if (m_unplugNodeArgs != null)
        {
            Move(m_unplugNodeArgs, Vector2.zero, true);
        }

        m_workspace.CodePanel.RecalculateContentSize();
        m_workspace.m_MessagePanel.Layout();
    }

    protected override void RedoImpl()
    {
        Move(m_args, Vector2.zero, false);

        m_contentOffset = m_workspace.CodePanel.RecalculateContentSize();
        m_workspace.m_MessagePanel.Layout();
    }

    // break the connection to the current target and connect to the new target
    private void Move(MoveNodeCommandArgs args, Vector2 nodeOffset, bool isUndo)
    {
        Vector2 targetPos = isUndo ? args.oldLocalPos : args.newLocalPos;
        ConnectionId curConnId = isUndo ? args.newConnId : args.oldConnId;
        ConnectionId newConnId = isUndo ? args.oldConnId : args.newConnId;

        var firstNode = m_workspace.CodePanel.GetNode(args.firstNodeId);
        if (!firstNode)
        {
            Debug.LogError("first node not found");
            return;
        }

        FunctionNode lastNode = null;
        if (args.lastNodeId != -1 && isUndo)
        {
            lastNode = m_workspace.CodePanel.GetNode(args.lastNodeId);
            if (lastNode == null)
            {
                Debug.LogError("last node not found");
                return;
            }
        }

        FunctionNode targetNode = null;
        if (newConnId.isValid)
        {
            targetNode = m_workspace.CodePanel.GetNode(newConnId.nodeId);
            if (!targetNode)
            {
                Debug.LogError("target node does not exist");
                return;
            }
        }

        FunctionNode curTarget = null;
        if (curConnId.isValid)
        {
            curTarget = m_workspace.CodePanel.GetNode(curConnId.nodeId);
            if (!curTarget)
            {
                Debug.LogError("current target does not exist");
                return;
            }
        }

        if (isUndo)
        {
            NodeUtils.OffsetFreeNodes(m_workspace.CodePanel, -nodeOffset);
        }

        if (curTarget)
        {
            var curConn = curTarget.GetConnection(curConnId.localId);
            if (curConn.type == ConnectionTypes.Top)
            {
                curTarget.Disconnect();
            }
            else
            {
                firstNode.Disconnect(lastNode);
            }
        }

        firstNode.MoveTo(targetPos);

        if (isUndo)
        {
            firstNode.UpdateRenderOrder(args.oldRenderOrder, true);
        }
        else if (!isUndo)
        {
            firstNode.UpdateRenderOrder(m_workspace.CodePanel.NextNodeRenderOrder, true);
        }

        if (targetNode)
        {
            var conn = targetNode.GetConnection(newConnId.localId);
            var unpluggedEvent = targetNode.Connect(firstNode, conn);
            if (unpluggedEvent != null)
            {
                m_unplugNodeArgs = new MoveNodeCommandArgs();
                m_unplugNodeArgs.firstNodeId = unpluggedEvent.node.NodeIndex;
                m_unplugNodeArgs.SetOldConnection(unpluggedEvent.oldConn);
            }
        }
    }
}
