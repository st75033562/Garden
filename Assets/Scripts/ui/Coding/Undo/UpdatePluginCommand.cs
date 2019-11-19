using System;
using UnityEngine;

public class UpdatePluginCommand : BaseWorkspaceCommand
{
    private readonly int m_nodeId;
    private readonly int m_pluginId;
    private readonly Save_PluginsData m_oldState;
    private readonly Save_PluginsData m_newState;
    private Vector2 m_contentOffset;

    public UpdatePluginCommand(
        UIWorkspace workspace, NodePluginsBase plugin, Save_PluginsData oldState)
        : base(workspace)
    {
        if (plugin == null)
        {
            throw new ArgumentNullException("plugin");
        }

        if (oldState == null)
        {
            throw new ArgumentNullException("oldState");
        }

        m_nodeId = plugin.ParentNode.NodeIndex;
        m_pluginId = plugin.PluginID;
        m_oldState = oldState;
        m_newState = plugin.GetPluginSaveData();
    }

    protected override void UndoImpl()
    {
        LoadPluginState(m_oldState);

        NodeUtils.OffsetFreeNodes(m_workspace.CodePanel, -m_contentOffset);
        m_workspace.CodePanel.RecalculateContentSize();
    }

    protected override void RedoImpl()
    {
        LoadPluginState(m_newState);
        m_contentOffset = m_workspace.CodePanel.RecalculateContentSize();
    }

    private void LoadPluginState(Save_PluginsData state)
    {
        var node = m_workspace.CodePanel.GetNode(m_nodeId);
        if (!node)
        {
            Debug.LogError("invalid node");
            return;
        }

        var plugin = node.GetPluginById(m_pluginId);
        if (!plugin)
        {
            Debug.LogError("invalid plugin");
            return;
        }

        plugin.LoadPluginSaveData(state);
        plugin.LayoutChanged();
    }
}
