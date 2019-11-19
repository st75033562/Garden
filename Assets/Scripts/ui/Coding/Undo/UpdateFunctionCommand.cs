using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Scheduling;

public class UpdateFunctionCommand : BaseWorkspaceCommand
{
    private readonly FunctionDeclaration m_oldDeclaration;
    private readonly FunctionDeclaration m_newDeclaration;
    private readonly Dictionary<int, List<DeletedArgInfo>> m_argInfoDict = new Dictionary<int, List<DeletedArgInfo>>();
    private FunctionDeclarationNodeState m_oldNodeState;
    private FunctionDeclarationNodeState m_newNodeState;

    public UpdateFunctionCommand(
        UIWorkspace workspace, FunctionDeclaration oldDeclaration, FunctionDeclaration newDeclaration)
        : base(workspace)
    {
        if (oldDeclaration == null)
        {
            throw new ArgumentNullException("oldDeclaration");
        }
        if (newDeclaration == null)
        {
            throw new ArgumentNullException("newDeclaration");
        }
        if (oldDeclaration.functionId != newDeclaration.functionId)
        {
            throw new ArgumentException("function id mismatches");
        }

        m_oldDeclaration = oldDeclaration;
        m_newDeclaration = newDeclaration;
    }

    protected override bool isUndoAsync
    {
        get { return true; }
    }

    protected override void UndoImpl()
    {
        Scheduler.instance.Schedule(UpdateFunction(m_oldDeclaration, false));
    }

    protected override bool isRedoAsync
    {
        get { return true; }
    }

    protected override void RedoImpl()
    {
        Scheduler.instance.Schedule(UpdateFunction(m_newDeclaration, true));
    }

    private IEnumerator UpdateFunction(FunctionDeclaration declaration, bool redo)
    {
        // rebuild the decl node, nothing needs to be saved or restored
        var declNode = m_workspace.CodePanel.GetFunctionNode(declaration.functionId);
        if (redo && m_oldNodeState == null)
        {
            m_oldNodeState = declNode.GetState();
        }
        else if (!redo && m_newNodeState == null)
        {
            m_newNodeState = declNode.GetState();
        }
        declNode.Rebuild(declaration, redo ? m_newNodeState : m_oldNodeState);

        // rebuild all call nodes
        var callNodes = m_workspace.CodePanel.Nodes
            .OfType<FunctionCallNode>()
            .Where(x => x.Declaration.functionId == declaration.functionId)
            .ToList();

        foreach (var callNode in callNodes)
        {
            var deletedArgs = callNode.Rebuild(declaration);
            if (redo)
            {
                // save the state of the deleted nodes for undo
                m_argInfoDict[callNode.NodeIndex] = deletedArgs;
            }
            else if (m_argInfoDict.ContainsKey(callNode.NodeIndex))
            {
                // restore the inserted nodes
                foreach (var info in m_argInfoDict[callNode.NodeIndex])
                {
                    var request = m_workspace.CodePanel.AddNodesAsync(info.insertedNodeStates, false);
                    yield return request;
                    callNode.GetPluginById(info.pluginId).LoadPluginSaveData(info.pluginState);
                    // reconnect
                    callNode.Connect(request.result[0], callNode.GetConnection(info.connectionId));
                }
                callNode.LayoutBottomUp();
            }
        }

        m_workspace.m_NodeTempList.RebuildFuncCall(declaration);
        FireCompleted();
    }
}
