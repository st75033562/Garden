using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DeleteVariableCommand : BaseWorkspaceCommand
{
    private readonly BaseVariable m_variable;
    private readonly Dictionary<int, List<int>> m_affectedPlugins = new Dictionary<int, List<int>>();
    private Vector2 m_contentOffset;

    public DeleteVariableCommand(UIWorkspace workspace, string name)
        : base(workspace)
    {
        m_variable = workspace.CodeContext.variableManager.get(name);

        if (m_variable == null)
        {
            throw new ArgumentException("name");
        }

        SaveDataPlugins();
    }

    private void SaveDataPlugins()
    {
        foreach (var node in m_workspace.CodePanel.Nodes)
        {
            var plugins = node.Plugins
                .OfType<DataMenuPlugins>()
                .Where(x => x.GetPluginsText() == m_variable.name)
                .Select(x => x.PluginID)
                .ToList();
            if (plugins.Count > 0)
            {
                m_affectedPlugins[node.NodeIndex] = plugins;
            }
        }
    }

    protected override void UndoImpl()
    {
        m_workspace.CodeContext.variableManager.add(m_variable);
        RestoreDataPlugins();
        m_workspace.m_NodeTempList.RefreshDataNode();

        NodeUtils.OffsetFreeNodes(m_workspace.CodePanel, -m_contentOffset);
        m_workspace.CodePanel.RecalculateContentSize();
    }

    private void RestoreDataPlugins()
    {
        foreach (var kv in m_affectedPlugins)
        {
            var node = m_workspace.CodePanel.GetNode(kv.Key);
            if (!node)
            {
                Debug.LogError("node not found");
                continue;
            }

            foreach (var id in kv.Value)
            {
                var plugin = node.GetPluginById(id);
                plugin.SetPluginsText(m_variable.name);
            }
            node.LayoutBottomUp();
        }
    }

    protected override void RedoImpl()
    {
        m_workspace.CodeContext.variableManager.remove(m_variable.name);
        m_workspace.m_NodeTempList.RefreshDataNode();

        m_contentOffset = m_workspace.CodePanel.RecalculateContentSize();
    }
}
