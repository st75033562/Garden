using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// A modified version of ContentSizeFitter with added capability for specifying minimum size
/// </summary>
[ExecuteInEditMode]
[RequireComponent(typeof(RectTransform))]
public class ContentSizeFitter : UIBehaviour, ILayoutSelfController
{
    public enum FitMode
    {
        Unconstrained,
        MinSize,
        PreferredSize
    }

    [SerializeField]
    protected Vector2 m_MinSize = Vector2.zero;
    public Vector2 minSize { get { return m_MinSize; } set { if (SetPropertyUtility.SetStruct(ref m_MinSize, value)) SetDirty(); } }

    [SerializeField]
    protected Vector2 m_MaxSize = Vector2.zero;
    public Vector2 maxSize { get { return m_MaxSize; } set { if (SetPropertyUtility.SetStruct(ref m_MaxSize, value)) SetDirty(); } }

    [SerializeField]
    protected Vector2 m_Padding = Vector2.zero;
    public Vector2 padding { get { return m_Padding; } set { if (SetPropertyUtility.SetStruct(ref m_Padding, value)) SetDirty(); } }

    [SerializeField]
    protected FitMode m_HorizontalFit = FitMode.Unconstrained;
    public FitMode horizontalFit { get { return m_HorizontalFit; } set { if (SetPropertyUtility.SetStruct(ref m_HorizontalFit, value)) SetDirty(); } }

    [SerializeField]
    protected FitMode m_VerticalFit = FitMode.Unconstrained;
    public FitMode verticalFit { get { return m_VerticalFit; } set { if (SetPropertyUtility.SetStruct(ref m_VerticalFit, value)) SetDirty(); } }

    [System.NonSerialized]
    private RectTransform m_Rect;
    private RectTransform rectTransform
    {
        get
        {
            if (m_Rect == null)
                m_Rect = GetComponent<RectTransform>();
            return m_Rect;
        }
    }

    protected DrivenRectTransformTracker m_Tracker;

    protected ContentSizeFitter()
    { }

    #region Unity Lifetime calls

    protected override void OnEnable()
    {
        base.OnEnable();
        SetDirty();
    }

    protected override void OnDisable()
    {
        m_Tracker.Clear();
        LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
        base.OnDisable();
    }

    #endregion

    protected override void OnRectTransformDimensionsChange()
    {
        SetDirty();
    }

    private void HandleSelfFittingAlongAxis(int axis)
    {
        FitMode fitting = (axis == 0 ? horizontalFit : verticalFit);
        if (fitting == FitMode.Unconstrained)
        {
            // Keep a reference to the tracked transform, but don't control its properties:
            m_Tracker.Add(this, rectTransform, DrivenTransformProperties.None);
            return;
        }

        m_Tracker.Add(this, rectTransform, (axis == 0 ? DrivenTransformProperties.SizeDeltaX : DrivenTransformProperties.SizeDeltaY));

        float minSize = m_MinSize[axis];
        float maxSize = m_MaxSize[axis];
        float size;
        // Set size to min or preferred size
        if (fitting == FitMode.MinSize)
        {
            size = LayoutUtility.GetMinSize(m_Rect, axis);
        }
        else
        {
            size = LayoutUtility.GetPreferredSize(m_Rect, axis);
        }
        size = Mathf.Clamp(size, minSize, maxSize > 0 ? maxSize : float.MaxValue) + padding[axis];
        rectTransform.SetSizeWithCurrentAnchors((RectTransform.Axis)axis, size);
    }

    public virtual void SetLayoutHorizontal()
    {
        m_Tracker.Clear();
        HandleSelfFittingAlongAxis(0);
    }

    public virtual void SetLayoutVertical()
    {
        HandleSelfFittingAlongAxis(1);
    }

    protected void SetDirty()
    {
        if (!IsActive())
            return;

        LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        SetDirty();
    }

#endif
}
