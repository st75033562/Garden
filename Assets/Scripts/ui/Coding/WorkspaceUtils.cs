using System;
using System.Collections.Generic;
using UnityEngine;

public static class WorkspaceUtils
{
    public static void CopyNodesToClipboard(UIWorkspace workspace, params FunctionNode[] headNodes)
    {
        CopyNodesToClipboard(workspace, (IEnumerable<FunctionNode>)headNodes);
    }

    public static void CopyNodesToClipboard(UIWorkspace workspace, IEnumerable<FunctionNode> headNodes)
    {
        if (workspace == null)
        {
            throw new ArgumentNullException("workspace");
        }
        if (headNodes == null)
        {
            throw new ArgumentNullException("headNodes");
        }

        Clipboard.nodeStates = new BlockSaveStates(workspace, headNodes, true);
        Clipboard.type = workspace.CodeType;
        Clipboard.workspaceId = workspace.Id;
    }

    public static bool CanPasteFromClipboard(UIWorkspace workspace)
    {
        if (workspace == null)
        {
            throw new ArgumentNullException("workspace");
        }

        return Clipboard.type == workspace.CodeType && !Clipboard.isEmpty;
    }

    public static void PasteNodesFromClipboard(UIWorkspace workspace, Vector2 screenPos)
    {
        if (workspace == null)
        {
            throw new ArgumentNullException("workspace");
        }

        if (!CanPasteFromClipboard(workspace))
        {
            return;
        }

        var pastePos = workspace.CodePanel.transform.WorldToLocal(screenPos);
        var sameWorkspace = Clipboard.workspaceId == workspace.Id;
        workspace.UndoManager.AddUndo(new PasteNodeCommand(workspace, sameWorkspace, Clipboard.nodeStates, pastePos));
    }
}
