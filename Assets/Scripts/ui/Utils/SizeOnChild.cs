using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[ExecuteInEditMode]
public class SizeOnChild : UIBehaviour
{
    private static readonly List<SizeOnChild> s_widgets = new List<SizeOnChild>();
    private static bool s_updatingLayout;

    public bool controlWidth = true;
    public bool controlHeight = true;

    private SizeOnChild m_parentWidget;
    private int m_depth;
    private RectTransform m_rectTrans;
    private LayoutGroup m_layoutGroup;
    private bool m_isDoingLayout;

    protected override void Awake()
    {
        m_rectTrans = GetComponent<RectTransform>();
        OnTransformParentChanged();
        MarkForLayout();
    }

    protected override void OnEnable()
    {
        MarkForLayout();
    }

    protected override void OnDisable()
    {
        MarkForLayout();
    }

    protected override void OnDestroy()
    {
        s_widgets.Remove(this);
    }

    protected override void OnRectTransformDimensionsChange()
    {
        if (s_updatingLayout || !isActiveAndEnabled) { return; }

        MarkForLayout();
    }

    protected override void OnTransformParentChanged()
    {
        m_depth = 0;
        var parent = transform.parent;
        m_parentWidget = parent.GetComponent<SizeOnChild>();
        do
        {
            ++m_depth;
            parent = parent.parent;
        } while (parent);

        m_layoutGroup = GetComponent<LayoutGroup>();
    }

    public void MarkForLayout()
    {
        if (!s_widgets.Contains(this))
        {
            s_widgets.Add(this);
            m_isDoingLayout = true;
        }
    }

    private void UpdateLayout()
    {
        Vector2 size;
        if (m_layoutGroup != null)
        {
            // must be called, otherwise, CalculateLayoutInputVertical won't work
            m_layoutGroup.CalculateLayoutInputHorizontal();
            if (controlWidth)
            {
                // Manually calculating the preferred size, this is faster than calling ForceRebuildLayoutImmediate
                // which will rebuild the entire subtree. Since we do a bottom-up update, children have already been updated,
                // a single update is sufficed.
                m_rectTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, LayoutUtility.GetPreferredWidth(m_rectTrans));
            }
            if (controlHeight)
            {
                m_layoutGroup.CalculateLayoutInputVertical();
                m_rectTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, LayoutUtility.GetPreferredHeight(m_rectTrans));
            }
        }
        else if (transform.childCount > 0)
        {
            Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
            Vector2 max = new Vector2(float.MinValue, float.MinValue);
            foreach (Transform child in transform)
            {
                var rectTrans = child.GetComponent<RectTransform>();
                if (rectTrans.gameObject.activeInHierarchy)
                {
                    var scale = rectTrans.localScale.xy();
                    var offset = rectTrans.localPosition.xy();
                    // assume no rotation
                    var childMin = Vector2.Scale(rectTrans.rect.min, scale) + offset;
                    var childMax = Vector2.Scale(rectTrans.rect.max, scale) + offset;

                    min = Vector2.Min(min, childMin);
                    max = Vector2.Max(max, childMax);
                }
            }
            size = max - min;
            if (controlWidth)
            {
                m_rectTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
            }
            if (controlHeight)
            {
                m_rectTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
            }
        }
    }

    public static void Layout()
    {
        for (int i = s_widgets.Count - 1; i >= 0; --i)
        {
            var widget = s_widgets[i];
            var parent = widget.m_parentWidget;
            while (parent && !parent.m_isDoingLayout)
            {
                s_widgets.Add(parent);
                parent.m_isDoingLayout = true;
                parent = parent.m_parentWidget;
            }
        }

        s_widgets.Sort((x, y) => y.m_depth.CompareTo(x.m_depth));
        s_updatingLayout = true;
        foreach (var widget in s_widgets)
        {
            widget.UpdateLayout();
            widget.m_isDoingLayout = false;
        }
        s_updatingLayout = false;
        s_widgets.Clear();
    }

#if UNITY_EDITOR
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void DoReset()
    {
        s_widgets.Clear();
    }
#endif
}
