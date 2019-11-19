using System;
using UnityEngine;

public class AddNodeCommand : BaseWorkspaceCommand
{
    private readonly int m_oldNodeIndexSeed;
    private readonly int m_templateId;
    private readonly Save_NodeData m_restoreState;
    private int m_cloneNodeIndex = -1;
    private Vector2 m_localPos;
    private int m_nextFuncCallTempId;

    public AddNodeCommand(UIWorkspace workspace, int templateId, Save_NodeData restoreState)
        : base(workspace)
    {
        m_templateId = templateId;
        m_oldNodeIndexSeed = workspace.CodePanel.NextNodeIndex;
        m_restoreState = restoreState;
        recalculateContent = true;
    }

    public Vector2 localPos
    {
        get;
        set;
    }

    public FunctionNode clone
    {
        get { return m_workspace.CodePanel.GetNode(m_cloneNodeIndex); }
    }

    public bool recalculateContent
    {
        get;
        set;
    }

    public Vector2 contentOffset
    {
        get;
        set;
    }

    protected override void UndoImpl()
    {
        var clone = this.clone;
        if (!clone)
        {
            Debug.LogError("clone not found");
            return;
        }

        clone.ChainedDelete();
        m_workspace.CodePanel.NextNodeIndex = m_oldNodeIndexSeed;
        m_cloneNodeIndex = -1;
        m_workspace.m_NodeTempList.NextFuncCallTemplateId = m_nextFuncCallTempId;
        m_workspace.m_NodeTempList.RefreshCategory(NodeCategory.Function);

        NodeUtils.OffsetFreeNodes(m_workspace.CodePanel, -contentOffset);
        m_workspace.CodePanel.RecalculateContentSize();
    }

    protected override void RedoImpl()
    {
        m_nextFuncCallTempId = m_workspace.m_NodeTempList.NextFuncCallTemplateId;
        var clone = m_workspace.m_NodeTempList.GetTemplateByID(m_templateId).Clone(m_workspace.CodePanel.transform);
        clone.gameObject.SetActive(true);
        clone.IsTemplate = false;

        if (!clone.CodePanel)
        {
            m_workspace.CodePanel.AddNode(clone);
        }

        if (m_restoreState != null)
        {
            clone.LoadNodeSaveData(m_restoreState, false);
            clone.PostLoad();
            clone.Layout();
        }

        clone.MoveTo(localPos);
        m_workspace.m_NodeTempList.RefreshCategory(NodeCategory.Function);

        m_cloneNodeIndex = clone.NodeIndex;

        if (recalculateContent)
        {
            contentOffset = m_workspace.CodePanel.RecalculateContentSize();
        }
    }
}
