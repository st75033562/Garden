using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The helper to optimize the handling of RectTransform.ReapplyDrivenProperties for LayoutRebuilder
/// </summary>
/// <remarks>
/// The original handler will detect if a RectTransform needs layout which is time wasting for transforms
/// we know for sure, that don't need auto layout. So we replace the default handler with a custom optimized
/// version which will by pass the default handling by checking the layer of a transform.
/// </remarks>
public static class UIPerformanceHelper
{
    private static int s_ignoreLayoutLayerMask;
    private static RectTransform.ReapplyDrivenProperties s_originalHandler;

    static UIPerformanceHelper()
    {
        var method = typeof(LayoutRebuilder).GetMethod("ReapplyDrivenProperties", BindingFlags.Static | BindingFlags.NonPublic);
        if (method != null)
        {
            var eventInfo = typeof(RectTransform).GetEvent("reapplyDrivenProperties");
            s_originalHandler = (RectTransform.ReapplyDrivenProperties)Delegate.CreateDelegate(eventInfo.EventHandlerType, method);
            // remove the original handler
            eventInfo.RemoveEventHandler(null, s_originalHandler);
            // add the custom handler
            eventInfo.AddEventHandler(null, (RectTransform.ReapplyDrivenProperties)ReapplyDrivenProperties);
        }
        else
        {
            Debug.LogWarning("LayoutRebuilder.ReapplyDrivenProperties not found");
        }
    }

    public static int ignoredLayoutLayerMask
    {
        get { return s_ignoreLayoutLayerMask; }
        set { s_ignoreLayoutLayerMask = value; }
    }

    static void ReapplyDrivenProperties(RectTransform driven)
    {
        if (driven != null && ((1 << driven.gameObject.layer) & s_ignoreLayoutLayerMask) != 0)
        {
            return;
        }
        else
        {
            s_originalHandler(driven);
        }
    }
}
