using System;
using UnityEngine;

public class CopyNodeCommand : BaseWorkspaceCommand
{
    private int m_nodeIndexSeed;
    private int m_nodeIndex;
    private Vector2 m_contentOffset;
    private int m_cloneNodeIndex = -1;
    private Vector2 m_localPos;

    private const float Offset = 20;

    public CopyNodeCommand(UIWorkspace workspace, FunctionNode firstNode)
        : base(workspace)
    {
        var worldPos = firstNode.transform.position + new Vector3(Offset, -Offset);
        Init(firstNode, workspace.CodePanel.transform.WorldToLocal(worldPos));
    }

    public CopyNodeCommand(UIWorkspace workspace, FunctionNode firstNode, Vector2 localPos)
        : base(workspace)
    {
        Init(firstNode, localPos);
    }

    void Init(FunctionNode firstNode, Vector2 localPos)
    {
        if (firstNode == null)
        {
            throw new ArgumentNullException("firstNode");
        }

        m_nodeIndex = firstNode.NodeIndex;
        m_nodeIndexSeed = m_workspace.CodePanel.NextNodeIndex;
        m_localPos = localPos;
    }

    public FunctionNode clone
    {
        get { return m_workspace.CodePanel.GetNode(m_cloneNodeIndex); }
    }

    protected override void UndoImpl()
    {
        var firstNode = m_workspace.CodePanel.GetNode(m_cloneNodeIndex);
        if (!firstNode)
        {
            Debug.LogError("clone not found");
            return;
        }

        m_cloneNodeIndex = -1;
        NodeUtils.OffsetFreeNodes(m_workspace.CodePanel, -m_contentOffset);
        firstNode.ChainedDelete();

        m_workspace.CodePanel.NextNodeIndex = m_nodeIndexSeed;
        m_workspace.CodePanel.RecalculateContentSize();
    }

    protected override void RedoImpl()
    {
        var firstNode = m_workspace.CodePanel.GetNode(m_nodeIndex);
        if (!firstNode)
        {
            Debug.LogError("invalid source node");
            return;
        }

        var clone = firstNode.ChainedClone();
        m_cloneNodeIndex = clone.NodeIndex;

        clone.MoveTo(m_localPos);

        m_contentOffset = m_workspace.CodePanel.RecalculateContentSize();
    }
}
