using System;
using System.Text;
using UnityEngine;

public static class LogicTransformUtils
{
    /// <summary>
    /// Instantiate the visual tree with the given logic tree
    /// </summary>
    /// <returns></returns>
    public static GameObject Instantitate(LogicTransform root, Transform parent)
    {
        if (root == null)
        {
            throw new ArgumentNullException("root");
        }

        return Instantiate(root, root.visualTarget.parent, parent);
    }

    private static GameObject Instantiate(LogicTransform transform, Transform oldVisualRootParent, Transform newVisualRootParent)
    {
        var go = UnityEngine.Object.Instantiate(transform.visualTarget.gameObject, newVisualRootParent);
        if (NodeTemplateCache.Instance.ShowBlockUI)
        {
            foreach (var child in transform)
            {
                if (child.visualTarget.parent == oldVisualRootParent)
                {
                    Instantiate(transform, oldVisualRootParent, newVisualRootParent);
                }
            }
        }
        return go;
    }

    public static void PrintHierarchy(LogicTransform transform, int indent = 4)
    {
        if (transform == null)
        {
            throw new ArgumentNullException("transform");
        }

        var sb = new StringBuilder();
        PrintHierarchy(transform, 0, Mathf.Max(1, indent), sb);
        Debug.Log(sb);
    }

    private static void PrintHierarchy(LogicTransform transform, int depth, int indent, StringBuilder sb)
    {
        sb.Append(new string(' ', depth * indent));
        sb.AppendFormat("{0}, local: {1}", transform.visualTarget.name, transform.localPosition);
        sb.AppendLine();

        foreach (var child in transform)
        {
            PrintHierarchy(child, depth + 1, indent, sb);
        }
    }
}
