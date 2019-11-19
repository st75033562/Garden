using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class NodeLayoutUtil
{
    public const float XPadding = 26;
    public const float Spacing = 5;

    public static float CalcMaxHeight(IEnumerable<NodePluginsBase> plugins)
    {
        return plugins.Where(x => x.gameObject.activeSelf)
                      .Select(x => x.RectTransform.rect.height)
                      .DefaultIfEmpty()
                      .Max();
    }

    public static Vector2 Layout(RectTransform nodeTrans, IEnumerable<NodePluginsBase> plugins, float defaultYOffset)
    {
        if (NodeTemplateCache.Instance.ShowBlockUI)
        {
            if (nodeTrans == null)
            {
                throw new ArgumentNullException("nodeTrans");
            }
            if (plugins == null)
            {
                throw new ArgumentNullException("plugins");
            }
        }

        float maxHeight = 0;
        float x = XPadding;
        if (NodeTemplateCache.Instance.ShowBlockUI)
        {
            foreach (var plugin in plugins)
            {
                var pluginRect = plugin.RectTransform;
                if (pluginRect.gameObject.activeSelf)
                {
                    plugin.Layout();
                    pluginRect.anchoredPosition = new Vector2(x + plugin.PosOffset.x, plugin.PosOffset.y != 0 ? plugin.PosOffset.y : defaultYOffset);
                    plugin.LayoutChild();

                    x = pluginRect.localPosition.x + pluginRect.rect.width + Spacing;
                    maxHeight = Mathf.Max(maxHeight, pluginRect.rect.height);
                }
            }
            x += XPadding - Spacing;
        }
        return new Vector2(x, maxHeight);
    }
}
