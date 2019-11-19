using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PasteNodeCommand : BaseWorkspaceCommand
{
    private readonly BlockSaveStates m_saveData;
    private readonly List<int> m_pastedNodeIds = new List<int>();
    private readonly Vector2 m_topLeftPastePos;
    private Vector2 m_contentOffset;
    private int m_nextNodeIndex;
    private int m_nextFuncCallTempId;
    private object m_nodePosGenState;

    public PasteNodeCommand(UIWorkspace workspace, bool statesFromCurWorkspace, BlockSaveStates clipboardStates, Vector2 topLeftPastePos)
        : base(workspace)
    {
        if (clipboardStates == null)
        {
            throw new ArgumentNullException("clipboardStates");
        }
        m_saveData = clipboardStates.MakeNewFunctions(!statesFromCurWorkspace);
        m_topLeftPastePos = topLeftPastePos;
        m_nodePosGenState = m_workspace.NodePosGenerator.state;
    }

    protected override void UndoImpl()
    {
        foreach (var nodeId in m_pastedNodeIds)
        {
            var node = m_workspace.CodePanel.GetNode(nodeId);
            if (node && node.IsFreeNode)
            {
                node.ChainedDelete();
            }
            if (node is FunctionDeclarationNode)
            {
                var funcId = (node as FunctionDeclarationNode).Declaration.functionId;
                m_workspace.m_NodeTempList.RemoveFuncCall(funcId);
            }
        }
        m_pastedNodeIds.Clear();
        m_workspace.CodePanel.NextNodeIndex = m_nextNodeIndex;

        NodeUtils.OffsetFreeNodes(m_workspace.CodePanel, -m_contentOffset);
        m_workspace.CodePanel.RecalculateContentSize();

        m_workspace.m_NodeTempList.NextFuncCallTemplateId = m_nextFuncCallTempId;
        m_workspace.m_NodeTempList.RefreshCategory(NodeCategory.Function);

        m_workspace.NodePosGenerator.state = m_nodePosGenState;
    }

    protected override bool isRedoAsync
    {
        get { return true; }
    }

    protected override void RedoImpl()
    {
        m_nextNodeIndex = m_workspace.CodePanel.NextNodeIndex;
        m_nextFuncCallTempId = m_workspace.m_NodeTempList.NextFuncCallTemplateId;

        // add all referenced variables and messages
        foreach (var msg in m_saveData.referencedMessages)
        {
            if (!m_workspace.CodeContext.messageManager.has(msg.name))
            {
                m_workspace.CodeContext.messageManager.add(msg);
            }
        }
        m_workspace.m_NodeTempList.RefreshCategory(NodeCategory.Events);

        foreach (var variable in m_saveData.referencedVariables)
        {
            if (!m_workspace.CodeContext.variableManager.has(variable.name))
            {
                m_workspace.CodeContext.variableManager.add(variable);
            }
        }
        m_workspace.m_NodeTempList.RefreshCategory(NodeCategory.Data);

        m_workspace.CodePanel.AddNodesAsync(m_saveData, true)
            .OnCompleted(request => {
                var nodes = request.result;
                var boundingRect = Utils.ComputeLocalRect(
                    (RectTransform)m_workspace.CodePanel.transform,
                    nodes.Select(x => x.RectTransform));

                var topLeft = new Vector2(boundingRect.xMin, boundingRect.yMax);
                NodeUtils.OffsetNodes(nodes.Where(x => x.IsFreeNode), m_topLeftPastePos - topLeft);

                m_pastedNodeIds.AddRange(nodes.Select(x => x.NodeIndex));
                m_contentOffset = m_workspace.CodePanel.RecalculateContentSize();

                foreach (var node in nodes.OfType<FunctionDeclarationNode>())
                {
                    m_workspace.m_NodeTempList.AddFuncCall(node.Declaration);
                }
                m_workspace.m_NodeTempList.RefreshCategory(NodeCategory.Function);

                FireCompleted();
            });
    }
}
