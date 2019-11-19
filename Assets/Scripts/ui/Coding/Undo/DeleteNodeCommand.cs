using System;
using System.Collections.Generic;
using UnityEngine;

public class DeleteNodeCommand : BaseWorkspaceCommand
{
    private readonly int m_firstNodeId;
    private readonly int m_lastNodeId;
    private readonly BlockSaveStates m_nodeStates;
    private readonly ConnectionId m_oldConnId;
    private Vector2 m_contentOffset;

    private FunctionDeclaration m_funcDecl;
    private object m_callState;

    public DeleteNodeCommand(UIWorkspace workspace, FunctionNode firstNode, FunctionNode lastNode = null)
        : base(workspace)
    {
        if (firstNode == null)
        {
            throw new ArgumentNullException("node");
        }

        if (lastNode == null)
        {
            lastNode = firstNode.GetLastNode();
        }

        m_firstNodeId = firstNode.NodeIndex;
        m_lastNodeId = lastNode.NodeIndex;

        m_nodeStates = new BlockSaveStates(workspace, new[] { firstNode }, false);

        var conn = firstNode.GetPrevConnection();
        m_oldConnId = conn != null ? conn.globalId : ConnectionId.invalid;

        if (firstNode is FunctionDeclarationNode)
        {
            m_funcDecl = (firstNode as FunctionDeclarationNode).Declaration;
            m_callState = m_workspace.m_NodeTempList.GetFuncCallState(m_funcDecl.functionId);
        }
    }

    protected override bool isUndoAsync
    {
        get { return true; }
    }

    protected override void UndoImpl()
    {
        FunctionNode targetNode = null;
        if (m_oldConnId.isValid)
        {
            targetNode = m_workspace.CodePanel.GetNode(m_oldConnId.nodeId);
            if (!targetNode)
            {
                Debug.LogError("target node does not exist");
                return;
            }
        }

        NodeUtils.OffsetFreeNodes(m_workspace.CodePanel, -m_contentOffset);

        m_workspace.CodePanel.AddNodesAsync(m_nodeStates, false)
            .OnCompleted(request => {
                if (targetNode)
                {
                    var conn = targetNode.GetConnection(m_oldConnId.localId);
                    targetNode.Connect(request.result[0], conn);
                }

                m_workspace.CodePanel.RecalculateContentSize();

                if (m_funcDecl != null)
                {
                    m_workspace.m_NodeTempList.AddFuncCall(m_funcDecl, m_callState);
                    m_workspace.m_NodeTempList.RefreshCategory(NodeCategory.Function);
                }

                FireCompleted();
            });
    }

    protected override void RedoImpl()
    {
        var firstNode = m_workspace.CodePanel.GetNode(m_firstNodeId);
        if (firstNode == null)
        {
            Debug.LogError("invalid first node");
            return;
        }

        var lastNode = m_workspace.CodePanel.GetNode(m_lastNodeId);
        if (lastNode == null)
        {
            Debug.LogError("invalid last node");
            return;
        }

        firstNode.Disconnect(lastNode);
        firstNode.ChainedDelete();

        m_contentOffset = m_workspace.CodePanel.RecalculateContentSize();

        if (m_funcDecl != null)
        {
            m_workspace.m_NodeTempList.RemoveFuncCall(m_funcDecl.functionId);
            m_workspace.m_NodeTempList.RefreshCategory(NodeCategory.Function);
        }
    }
}
