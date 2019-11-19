using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// set vertex color for gradient effect, only support simple sprite for now
/// </summary>
[RequireComponent(typeof(Image))]
public class UIGradientEffect : BaseMeshEffect
{
    public Color32 m_startColor = Color.white;
    public Color32 m_endColor = Color.white;

    public override void ModifyMesh(VertexHelper vh)
    {
        for (int i = 0; i < 4; ++i)
        {
            UIVertex vert = new UIVertex();
            vh.PopulateUIVertex(ref vert, i);
            vert.color = i < 2 ? m_startColor : m_endColor;
            vh.SetUIVertex(vert, i);
        }
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();

        var image = GetComponent<Image>();
        if (image.type != Image.Type.Simple && image.sprite != null)
        {
            Debug.LogError("only support simple image");
        }
    }
#endif
}
