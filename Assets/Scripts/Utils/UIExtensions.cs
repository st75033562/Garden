using UnityEngine;
using UnityEngine.UI;
using System;
using RobotSimulation;

public enum UIAnchor
{
    LeftCenter,
    BottomCenter,
    UpperCenter,
}

[Flags]
public enum UIRestrictAxis
{
    X = 1 << 0,
    Y = 1 << 1,
    All = X | Y
}

public static class UIExtensions
{
    public static void SetSize(this RectTransform transform, Vector2 size)
    {
#if z
        var delta = size - transform.rect.size;
        var pivot = transform.pivot;
        transform.offsetMin -= new Vector2(pivot.x * delta.x, pivot.y * delta.y);
        transform.offsetMax += new Vector2((1.0f - pivot.x) * delta.x, (1 - pivot.y) * delta.y);
#else
        SetSize(transform, size.x, size.y);
#endif
    }
    
    public static void SetSize(this RectTransform transform, float width, float height)
    {
        transform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);   
        transform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);   
    }

    public static void SetLocText(this Text textControl, string id)
    {
        textControl.text = LocalizationManager.instance.getString(id);
    }

    public static Rect GetRectIn(this RectTransform source, RectTransform target)
    {
        var min = target.InverseTransformPoint(source.TransformPoint(source.rect.min));
        var size = target.InverseTransformVector(source.TransformVector(source.rect.size));
        return new Rect(min, size);
    }

    public static Vector2 GetSizeIn(this RectTransform rect, Vector2 size, RectTransform target)
    {
        return target.InverseTransformVector(rect.TransformVector(size));
    }

    public static Rect InverseTransform(this RectTransform rect, Rect worldRect)
    {
        var min = rect.InverseTransformPoint(worldRect.min);
        var size = rect.InverseTransformVector(worldRect.size);
        return new Rect(min, size);
    }

    /// <summary>
    /// return the top left position in local coordinate system
    /// </summary>
    public static Vector2 TopLeft(this RectTransform rectTrans)
    {
        var rect = rectTrans.rect;
        return new Vector2(rect.xMin, rect.yMax);
    }

    public static Vector2 BottomLeft(this RectTransform rectTrans)
    {
        var rect = rectTrans.rect;
        return new Vector2(rect.xMin, rect.yMin);
    }

    /// <summary>
    /// return the offset from top left corner to the pivot in local coordinate system
    /// </summary>
    public static Vector2 PivotOffsetFromTopLeft(this RectTransform rectTrans)
    {
        var pivot = rectTrans.pivot;
        var rect = rectTrans.rect;
        return new Vector2(rect.width * pivot.x, (pivot.y - 1.0f) * rect.height);
    }

    /// <summary>
    /// fit the raw texture within the specified frame while keeping the aspect ratio
    /// </summary>
    public static void FitWithin(this RawImage image, RectTransform frame)
    {
        if (!image.texture) { return; }

        int width = image.texture.width;
        int height = image.texture.height;

        float scale = Mathf.Min(frame.rect.width / (float)width, frame.rect.height / (float)height);
        scale = Mathf.Min(scale, 1);
        image.rectTransform.SetSize(Mathf.RoundToInt(width * scale), Mathf.RoundToInt(height * scale));
    }

    /// <summary>
    /// position the transform at the given anchor position
    /// </summary>
    /// <param name="pos">world position</param>
    public static void Position(this RectTransform tran, Vector2 pos, UIAnchor anchor)
    {
        var anchorPos = GetPosition(tran, anchor);

        Vector3 offset;
        offset.x = pos.x - anchorPos.x;
        offset.y = pos.y - anchorPos.y;
        offset.z = 0;
        tran.position += offset;
    }

    public static Vector3 GetPosition(this RectTransform tran, UIAnchor anchor)
    {
        var corners = new Vector3[4];
        tran.GetWorldCorners(corners);

        int i, j;
        switch (anchor)
        {
        case UIAnchor.LeftCenter:
            i = 0;
            j = 1;
            break;

        case UIAnchor.BottomCenter:
            i = 0;
            j = 3;
            break;

        case UIAnchor.UpperCenter:
            i = 1;
            j = 2;
            break;

        default:
            throw new ArgumentOutOfRangeException("anchor");
        }

        return (corners[i] + corners[j]) / 2;
    }

    public static void Align(this RectTransform trans, UIAnchor anchor, Vector2 localOffset, RectTransform target, UIAnchor targetAnchor)
    {
        var worldOffset = trans.localToWorldMatrix.MultiplyVector(localOffset);
        trans.position += GetPosition(target, targetAnchor) - GetPosition(trans, anchor) + worldOffset;
    }

    public static void RestrictWithinCanvas(this RectTransform trans, UIRestrictAxis axes = UIRestrictAxis.All)
    {
        var canvas = trans.GetComponentInParent<Canvas>();
        var canvasTrans = canvas.GetComponent<RectTransform>();

        var canvasCorners = new Vector3[4];
        canvasTrans.GetWorldCorners(canvasCorners);

        var transCorners = new Vector3[4];
        trans.GetWorldCorners(transCorners);

        Vector3 delta = Vector3.zero;
        for (int i = 0; i < 2; ++i)
        {
            if (((int)axes & (1 << i)) == 0)
            {
                continue;
            }

            var end = i == 0 ? canvasCorners[3] : canvasCorners[1];
            float minT = float.MaxValue;
            float maxT = float.MinValue;
            for (int j = 0; j < 4; ++j)
            {
                var t = GeometryUtils.ComputeSegmentT(canvasCorners[0], end, transCorners[j]);
                minT = Mathf.Min(minT, t);
                maxT = Mathf.Max(maxT, t);
            }

            if (minT < 0)
            {
                delta += -minT * (end - canvasCorners[0]);
            }
            else if (maxT > 1)
            {
                delta += (1 - maxT) * (end - canvasCorners[0]);
            }
        }

        if (delta != Vector3.zero)
        {
            trans.position += delta;
        }
    }

    public static void SetAnchorMin(this RectTransform trans, RectTransform.Axis axis, float size)
    {
        var min = trans.anchorMin;
        min[(int)axis] = size;
        trans.anchorMin = min;
    }

    public static void SetAnchorMax(this RectTransform trans, RectTransform.Axis axis, float size)
    {
        var max = trans.anchorMax;
        max[(int)axis] = size;
        trans.anchorMax = max;
    }
}
