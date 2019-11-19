using System;
using UnityEngine;

public static class InputUtils
{
    private const int DepthLimit = 1000;
    private const int SiblingIndexLimit = 1000;

    // ui priority starts from 0
    public static int GetUIPriority(Transform trans, Canvas canvas, int relativePrio = -1)
    {
        if (trans == null)
        {
            throw new ArgumentNullException("trans");
        }

        if (!canvas)
        {
            throw new ArgumentException("canvas");
        }

        int subPrio = relativePrio != -1 ? relativePrio : trans.Depth() * SiblingIndexLimit + trans.GetSiblingIndex();
        return GetUIPriority(canvas.sortingOrder) + subPrio;
    }

    public static int GetUIPriority(int sortingOrder)
    {
        return sortingOrder * DepthLimit * SiblingIndexLimit;
    }
}
