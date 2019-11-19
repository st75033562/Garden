using DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// predefined node filter type
/// </summary>
public enum NodeFilterType
{
    Robot,
    Gameboard,
    Num
}

public class NodeFilterData
{
    private static readonly NodeFilterData[] s_filters = new NodeFilterData[(int)NodeFilterType.Num];

    private readonly Dictionary<int, NodeTemplateData> m_templates = new Dictionary<int, NodeTemplateData>();
    private readonly List<NodeCategory>[] m_levelCategories = new List<NodeCategory>[(int)BlockLevel.Num];

    public NodeFilterData(IEnumerable<NodeTemplateData> templates)
    {
        if (templates == null)
        {
            throw new ArgumentNullException("templates");
        }

        for (int i = 0; i < m_levelCategories.Length; ++i)
        {
            m_levelCategories[i] = new List<NodeCategory>();
        }

        foreach (var template in templates)
        {
            m_templates.Add(template.id, template);

            foreach (var levelData in template.allLevelData)
            {
                var categories = m_levelCategories[levelData.level - 1];
                if (!categories.Contains((NodeCategory)template.type))
                {
                    categories.Add((NodeCategory)template.type);
                }
            }
        }

        foreach (var categories in m_levelCategories)
        {
            categories.Sort();
        }
    }

    public NodeData GetLevelData(int templateId, int level)
    {
        NodeTemplateData templateData;
        if (m_templates.TryGetValue(templateId, out templateData))
        {
            return templateData.GetLevelData(level);
        }
        return null;
    }

    /// <summary>
    /// get active categories for the given level
    /// </summary>
    public IList<NodeCategory> GetCategories(int level)
    {
        return m_levelCategories[level - 1];
    }

    /// <summary>
    /// get a predefined filter
    /// </summary>
    public static NodeFilterData GetFilter(NodeFilterType filter)
    {
        if (s_filters[(int)filter] == null)
        {
            var templates = NodeTemplateData.GetAllByFilter(1 << (int)filter);
            s_filters[(int)filter] = new NodeFilterData(templates);
        }
        return s_filters[(int)filter];
    }
}
