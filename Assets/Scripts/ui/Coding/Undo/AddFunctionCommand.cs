using System;
using UnityEngine;

public class AddFunctionCommand : BaseWorkspaceCommand
{
    private readonly FunctionDeclaration m_declaration;
    private Vector2 m_nextNodeViewportPos;
    private Vector2 m_contentOffset;
    private int m_nodeIndex;
    private int m_nextNodeIndex;
    private int m_nextFuncCallTemplateId;
    private Vector2 m_functionPos;

    public AddFunctionCommand(UIWorkspace workspace, FunctionDeclaration declaration)
        : base(workspace)
    {
        if (declaration == null)
        {
            throw new ArgumentNullException("declaration");
        }

        m_declaration = declaration;
        m_functionPos = m_workspace.NodePosGenerator.Generate(m_workspace.CodePanel);
    }

    protected override void UndoImpl()
    {
        NodeUtils.OffsetFreeNodes(m_workspace.CodePanel, -m_contentOffset);

        var node = m_workspace.CodePanel.GetFunctionNode(m_declaration.functionId);
        node.Delete(false);

        m_workspace.CodePanel.NextNodeIndex = m_nextNodeIndex;
        m_workspace.CodePanel.RecalculateContentSize();

        m_workspace.m_NodeTempList.RemoveFuncCall(m_declaration.functionId);
        m_workspace.m_NodeTempList.NextFuncCallTemplateId = m_nextFuncCallTemplateId;
        m_workspace.m_NodeTempList.RefreshCategory(NodeCategory.Function);
    }

    protected override void RedoImpl()
    {
        m_nextFuncCallTemplateId = m_workspace.m_NodeTempList.NextFuncCallTemplateId;

        var node = (FunctionDeclarationNode)m_workspace.m_NodeTempList.FuncDeclNode.Clone(m_workspace.CodePanel.transform);
        node.gameObject.SetActive(true);
        node.IsTemplate = false;
        node.Rebuild(m_declaration);

        m_nextNodeIndex = m_workspace.CodePanel.NextNodeIndex;
        m_workspace.CodePanel.AddNode(node);

        node.MoveTo(m_functionPos);

        m_workspace.m_NodeTempList.AddFuncCall(m_declaration);
        m_workspace.m_NodeTempList.RefreshCategory(NodeCategory.Function);

        m_contentOffset = m_workspace.CodePanel.RecalculateContentSize();
    }
}
