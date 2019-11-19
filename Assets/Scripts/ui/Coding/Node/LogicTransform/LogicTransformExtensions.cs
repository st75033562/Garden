using System;
using UnityEngine;

public static class LogicTransformExtensions
{
    /// <summary>
    /// calculate the local position of the visual world position in the given parent
    /// </summary>
    /// <param name="parent">the parent transform, can be null</param>
    public static Vector3 GetLocalPositionIn(this LogicTransform transform, Transform parent)
    {
        if (transform == null)
        {
            throw new ArgumentNullException("transform");
        }

        return parent ? parent.InverseTransformPoint(transform.visualWorldPosition) : transform.visualWorldPosition;
    }
}
