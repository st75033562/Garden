using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Sprites;

public class UIMultiplicativeTextureEffect : BaseMeshEffect, IMaterialModifier
{
    [SerializeField]
    private Sprite m_multiplicativeSprite;

    private Material m_material;

    public Sprite multiplicativeSprite
    {
        get { return m_multiplicativeSprite; }
        set
        {
            if (SetPropertyUtility.SetClass(ref m_multiplicativeSprite, value))
            {
                graphic.SetMaterialDirty();
            }
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (m_material)
        {
            Utils.Destroy(m_material);
        }
    }

    public override void ModifyMesh(VertexHelper vh)
    {
        if (vh.currentVertCount != 4 || !m_multiplicativeSprite)
        {
            return;
        }

        var uv = DataUtility.GetOuterUV(m_multiplicativeSprite);
        SetUV1(vh, 0, new Vector2(uv.x, uv.y));
        SetUV1(vh, 1, new Vector2(uv.x, uv.w));
        SetUV1(vh, 2, new Vector2(uv.z, uv.w));
        SetUV1(vh, 3, new Vector2(uv.z, uv.y));
    }

    private void SetUV1(VertexHelper vh, int i, Vector2 uv)
    {
        var vert = new UIVertex();
        vh.PopulateUIVertex(ref vert, i);
        vert.uv1 = uv;
        vh.SetUIVertex(vert, i);
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        if (graphic && (graphic.canvas.additionalShaderChannels & AdditionalCanvasShaderChannels.TexCoord1) == 0)
        {
            Debug.Log("additional channel TexCoord1 must be enabled for the effect to work");
            graphic.canvas.additionalShaderChannels |= AdditionalCanvasShaderChannels.TexCoord1;
        }
    }
#endif

    public Material GetModifiedMaterial(Material baseMaterial)
    {
        if (!m_multiplicativeSprite)
        {
            return baseMaterial;
        }

        if (!m_material)
        {
            m_material = new Material(baseMaterial);
        }
        m_material.SetTexture("_MultiplicativeTex", m_multiplicativeSprite.texture);
        return m_material;
    }
}
