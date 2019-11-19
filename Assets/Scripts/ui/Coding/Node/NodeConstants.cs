using UnityEngine;

public static class NodeConstants
{
    public const float ControlNodeInnerHeight = 30.0f;
    // offset from step's bottom left to the child's anchored position
    // child's anchor is at parent's top left corner, pivot is (0, 1)
    public static readonly Vector2 ControlNodeChildOffset = new Vector2(30, 14);
}
