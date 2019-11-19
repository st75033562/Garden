using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using Debug = UnityEngine.Debug;

public static class DebugUtils
{
    public static Color color = Color.red;

    [Conditional("UNITY_EDITOR")]
    public static void DrawLineLoop(IList<Vector3> lines, int start, int end, Color color)
    {
        if (end - start < 2)
        {
            return;
        }

        for (int i = start; i < end - 1; ++i)
        {
            Debug.DrawLine(lines[i], lines[i + 1], color);
        }
        Debug.DrawLine(lines[end - 1], lines[start], color);
    }

    [Conditional("UNITY_EDITOR")]
    public static void DrawLineLoop(IList<Vector2> lines, int start, int end, float y, Color color)
    {
        if (end - start < 2)
        {
            return;
        }

        for (int i = start; i < end - 1; ++i)
        {
            Debug.DrawLine(lines[i].xzAtY(y), lines[i + 1].xzAtY(y), color);
        }
        Debug.DrawLine(lines[end - 1].xzAtY(y), lines[start].xzAtY(y), color);
    }

    [Conditional("UNITY_EDITOR")]
    public static void DrawLineLoop(IList<Vector3> lines, Color color)
    {
        DrawLineLoop(lines, 0, lines.Count, color);
    }

    [Conditional("UNITY_EDITOR")]
    public static void DrawLineLoop(IList<Vector2> lines, float y, Color color)
    {
        DrawLineLoop(lines, 0, lines.Count, y, color);
    }

    [Conditional("UNITY_EDITOR")]
    public static void DrawLineLoop(IList<Vector3> lines)
    {
        DrawLineLoop(lines, color);
    }

    [Conditional("UNITY_EDITOR")]
    public static void DrawLineLoop(IList<Vector2> lines, float y)
    {
        DrawLineLoop(lines, y, color);
    }

    [Conditional("UNITY_EDITOR")]
    public static void DrawTriangles(IList<Vector3> lines, Color color)
    {
        for (int i = 0; i + 3 <= lines.Count; i += 3)
        {
            DrawLineLoop(lines, i, i + 3, color);
        }
    }

    [Conditional("UNITY_EDITOR")]
    public static void DrawTriangles(IList<Vector2> lines, float y, Color color)
    {
        for (int i = 0; i + 3 <= lines.Count; i += 3)
        {
            DrawLineLoop(lines, i, i + 3, y, color);
        }
    }

    [Conditional("UNITY_EDITOR")]
    public static void DrawTriangles(IList<Vector3> lines)
    {
        DrawTriangles(lines, color);
    }

    [Conditional("UNITY_EDITOR")]
    public static void DrawTriangles(IList<Vector2> lines, float y)
    {
        DrawTriangles(lines, y, color);
    }

    [Conditional("UNITY_EDITOR")]
    public static void DrawText(Vector3 pos, string text, Color color)
    {
#if UNITY_EDITOR
        var oldColor = GUI.skin.label.normal.textColor;
        GUI.skin.label.normal.textColor = color;
        Handles.Label(pos, text);
        GUI.skin.label.normal.textColor = oldColor;
#endif
    }

    [Conditional("UNITY_EDITOR")]
    public static void DrawText(Vector3 pos, string text)
    {
        DrawText(pos, text, color);
    }
}
