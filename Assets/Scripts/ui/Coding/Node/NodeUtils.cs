using System.Collections.Generic;
using UnityEngine;

public static class NodeUtils
{
    public static Vector2 GetPanelLocalPos(FunctionNode node)
    {
        return node.CodePanel.transform.WorldToLocal(node.RectTransform.position);
    }

    public static void OffsetFreeNodes(CodePanel panel, Vector2 offset)
    {
        if (offset == Vector2.zero)
        {
            return;
        }

        OffsetNodes(panel.FreeNodes, offset);
    }

    public static void OffsetNodes(IEnumerable<FunctionNode> nodes, Vector2 offset)
    {
        foreach (var node in nodes)
        {
            Debug.Assert(!node.ParentNode, "node must be of top level");
            node.MoveTo(node.LogicTransform.localPosition + (Vector3)offset);
        }
    }

    public static IEnumerable<FunctionNode> GetDescendants(FunctionNode firstNode, FunctionNode lastNode)
    {
        while (firstNode != lastNode.NextNode)
        {
            foreach (var desc in firstNode.GetDescendants())
            {
                yield return desc;
            }
            firstNode = firstNode.NextNode;
        }
    }

    /// <summary>
    /// set the visual parent of all the nodes in the stack
    /// </summary>
    public static void SetParent(FunctionNode firstNode, Transform parent)
    {
        var stackNodes = GetDescendants(firstNode, firstNode.GetLastNode());
        // change all the visual parents before updating the logic transforms
        foreach (var node in stackNodes)
        {
            node.RectTransform.SetParent(parent);
        }
        foreach (var node in stackNodes)
        {
            node.LogicTransform.SyncPosition();
        }
    }
}
