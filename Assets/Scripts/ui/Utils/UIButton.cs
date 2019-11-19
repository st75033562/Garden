using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A simple hack for making a button without image.
/// Transparent buttons will cause too many overdraw when number gets high.
/// </summary>
public class UIButton : Graphic
{
    public bool raycastIgnoreParent = true;

    public override bool Raycast(Vector2 sp, Camera eventCamera)
    {
        if (!raycastIgnoreParent && transform.parent)
        {
            return base.Raycast(sp, eventCamera);
        }
        return isActiveAndEnabled;
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
    }
}
