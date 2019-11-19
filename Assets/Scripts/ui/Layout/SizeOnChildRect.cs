using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// NOTE: we implement ILayoutGroup so that we can rebuild the layout when any child has changed.
//       we don't change child's RectTransform, so it's safe to ignore the warning of ContentSizeFitter
public class SizeOnChildRect
    : UIBehaviour
    , ILayoutSelfController
    , ILayoutElement // for use with other layout groups
    , ILayoutGroup
{
    private Vector2 m_size;

    [SerializeField]
    private Vector2 m_Padding;
    public Vector2 padding { get { return m_Padding; } set { if (SetPropertyUtility.SetStruct(ref m_Padding, value)) SetDirty(); } }

    [SerializeField]
    private Vector2 m_MinSize;
    public Vector2 minSize { get { return m_MinSize; } set { if (SetPropertyUtility.SetStruct(ref m_MinSize, value)) SetDirty(); } }

    [SerializeField]
    private Vector2 m_MaxSize;
    public Vector2 maxSize { get { return m_MaxSize; } set { if (SetPropertyUtility.SetStruct(ref m_MaxSize, value)) SetDirty(); } }

    [SerializeField]
    protected bool m_HorizontalFit = false;
    public bool horizontalFit { get { return m_HorizontalFit; } set { if (SetPropertyUtility.SetStruct(ref m_HorizontalFit, value)) SetDirty(); } }

    [SerializeField]
    protected bool m_VerticalFit = false;
    public bool verticalFit { get { return m_VerticalFit; } set { if (SetPropertyUtility.SetStruct(ref m_VerticalFit, value)) SetDirty(); } }

    [SerializeField]
    private bool m_UsePreferredSize = true;
    public bool usePreferredSize { get { return m_UsePreferredSize; } set { if (SetPropertyUtility.SetStruct(ref m_UsePreferredSize, value)) SetDirty(); } }

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

    protected SizeOnChildRect()
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
        m_size[axis] = 0;
        for (int i = 0; i < rectTransform.childCount; ++i)
        {
            var child = rectTransform.GetChild(i) as RectTransform;
            float size;
            if (m_UsePreferredSize)
            {
                size = LayoutUtility.GetPreferredSize(child, axis);
            }
            else
            {
                size = child.rect.size[axis];
            }
            m_size[axis] = Mathf.Max(size, m_size[axis]);
        }
        m_size[axis] += m_Padding[axis];

        if (m_MinSize[axis] > 0 && m_size[axis] < m_MinSize[axis])
        {
            m_size[axis] = m_MinSize[axis];
        }

        if (m_MaxSize[axis] > 0 && m_size[axis] > m_MaxSize[axis])
        {
            m_size[axis] = m_MaxSize[axis];
        }
    }

    public virtual void SetLayoutHorizontal()
    {
        m_Tracker.Clear();
        if (m_HorizontalFit)
        {
            m_Tracker.Add(this, rectTransform, DrivenTransformProperties.SizeDeltaX);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, m_size[0]);
        }
    }

    public virtual void SetLayoutVertical()
    {
        if (m_VerticalFit)
        {
            m_Tracker.Add(this, rectTransform, DrivenTransformProperties.SizeDeltaY);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, m_size[1]);
        }
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

    void OnTransformChildrenChanged()
    {
        SetDirty();
    }

    void OnChildRectTransformDimensionsChange(GameObject child)
    {
        SetDirty();
    }

    #region ILayoutElement
    public void CalculateLayoutInputHorizontal()
    {
        m_size.x = 0;
        if (m_HorizontalFit)
        {
            HandleSelfFittingAlongAxis(0);
        }
    }

    public void CalculateLayoutInputVertical()
    {
        m_size.y = 0;
        if (m_VerticalFit)
        {
            HandleSelfFittingAlongAxis(1);
        }
    }

    public float flexibleHeight
    {
        get { return -1; }
    }

    public float flexibleWidth
    {
        get { return -1; }
    }

    public int layoutPriority
    {
        get { return 0; }
    }

    public float minHeight
    {
        get { return 0; }
    }

    public float minWidth
    {
        get { return 0; }
    }

    public float preferredHeight
    {
        get { return m_size.y; }
    }

    public float preferredWidth
    {
        get { return m_size.x; }
    }
    #endregion ILayoutElement
}