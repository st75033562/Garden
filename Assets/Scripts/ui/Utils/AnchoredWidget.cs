using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// qt-like simple anchoring system
/// </summary>
[ExecuteInEditMode]
public class AnchoredWidget : UIBehaviour
{
    public enum Anchor
    {
        Left,
        HorizontalCenter,
        Right,

        Top,
        VerticalCenter,
        Bottom,
    }

    [Serializable]
    public class AnchorConfig
    {
        public AnchoredWidget widget;
        public Anchor anchor;
        public float offset;
    }

    public AnchorConfig left;
    public AnchorConfig horizontalCenter;
    public AnchorConfig right;

    public AnchorConfig top;
    public AnchorConfig verticalCenter;
    public AnchorConfig bottom;

    private readonly List<AnchoredWidget> m_dependents = new List<AnchoredWidget>();
    private RectTransform m_rectTransform;
    private RectTransform m_parentTransform;

    private AnchorConfig[] m_configs;

#if UNITY_EDITOR
    private bool m_initialized;
#endif

    private enum VisitState
    {
        Unvisited,
        Visiting,
        Visited
    }

    private static readonly Dictionary<AnchoredWidget, VisitState> s_anchorStates 
        = new Dictionary<AnchoredWidget, VisitState>();
    private static readonly Queue<AnchoredWidget> s_anchors = new Queue<AnchoredWidget>();
    private static readonly Stack<AnchoredWidget> s_sortedWidgets = new Stack<AnchoredWidget>();
    private static bool s_updatingLayout = false;

    static AnchoredWidget()
    {
        // HACK: make sure Unity's willRenderCanvases is registered before our callback
        UnityEngine.UI.CanvasUpdateRegistry.IsRebuildingLayout();
        Canvas.willRenderCanvases += ForceImmediateLayout;
#if UNITY_EDITOR
        EditorApplication.update += OnEditorUpdate;
#endif
    }

    protected override void Awake()
    {
        m_rectTransform = GetComponent<RectTransform>();
        m_parentTransform = m_rectTransform.parent as RectTransform;

#if UNITY_EDITOR
        // seems these will be null when first added to the game object in EditMode
        EnsureNotNull(ref left);
        EnsureNotNull(ref right);
        EnsureNotNull(ref horizontalCenter);
        EnsureNotNull(ref top);
        EnsureNotNull(ref bottom);
        EnsureNotNull(ref verticalCenter);
#endif
    }

    void EnsureNotNull(ref AnchorConfig config)
    {
        if (config == null)
        {
            config = new AnchorConfig();
        }
    }

    protected override void Start()
    {
        Assert.IsTrue(!left.widget || !horizontalCenter.widget || !right.widget,
            "left, horiztonalCenter and right cannot be specified at the same time");

        Assert.IsTrue(!top.widget || !verticalCenter.widget || !bottom.widget,
            "top, verticalCenter and bottom cannot be specified at the same time");

        m_configs = new[] { left, horizontalCenter, right, top, verticalCenter, bottom };

        foreach (var cfg in m_configs)
        {
            // only direct parent and siblings are supported
#if UNITY_EDITOR
            Assert.IsTrue(!cfg.widget || cfg.widget.transform.parent == transform.parent || cfg.widget.transform == transform.parent);
#endif

            if (cfg.widget && !cfg.widget.m_dependents.Contains(this))
            {
                cfg.widget.m_dependents.Add(this);
            }
        }

#if UNITY_EDITOR
        m_initialized = true;
#endif

        MarkForLayout();
    }

    /// <summary>
    /// return the anchor's position in parent transform's rect
    /// </summary>
    /// <param name="cfg"></param>
    /// <returns></returns>
    private float GetAnchorPos(AnchorConfig cfg)
    {
        float pos;
        // if the anchor is our sibling
        if (cfg.widget.m_rectTransform.parent == m_rectTransform.parent)
        {
            pos = GetSiblingAnchorPos(cfg.widget.m_rectTransform, cfg.anchor);
        }
        else
        {
            // anchor is our parent
            switch (cfg.anchor)
            {
            case Anchor.Left:
            case Anchor.Bottom:
                pos = 0;
                break;

            case Anchor.Right:
                pos = m_parentTransform.rect.width;
                break;

            case Anchor.HorizontalCenter:
                pos = m_parentTransform.rect.center.x;
                break;

            case Anchor.Top:
                pos = m_parentTransform.rect.height;
                break;

            case Anchor.VerticalCenter:
                pos = m_parentTransform.rect.center.y;
                break;

            default:
                throw new InvalidOperationException();
            }
        }

        return pos + cfg.offset;
    }

    private float GetSiblingAnchorPos(RectTransform anchorTrans, Anchor anchor)
    {
        switch (anchor)
        {
        case Anchor.Left:
            return anchorTrans.offsetMin.x + anchorTrans.anchorMin.x * m_parentTransform.rect.width;

        case Anchor.Right:
            return anchorTrans.offsetMax.x + anchorTrans.anchorMax.x * m_parentTransform.rect.width;

        case Anchor.HorizontalCenter:
            return (GetSiblingAnchorPos(anchorTrans, Anchor.Left) + GetSiblingAnchorPos(anchorTrans, Anchor.Right)) * 0.5f;

        case Anchor.Top:
            return anchorTrans.offsetMax.y + anchorTrans.anchorMax.y * m_parentTransform.rect.height;

        case Anchor.Bottom:
            return anchorTrans.offsetMin.y + anchorTrans.anchorMin.y * m_parentTransform.rect.height;

        case Anchor.VerticalCenter:
            return (GetSiblingAnchorPos(anchorTrans, Anchor.Top) + GetSiblingAnchorPos(anchorTrans, Anchor.Bottom)) * 0.5f;

        default:
            throw new InvalidOperationException();
        }
    }

    void UpdateLayout()
    {
        if (!m_parentTransform) { return; }

        Vector2 offsetMin = m_rectTransform.offsetMin;
        Vector2 offsetMax = m_rectTransform.offsetMax;

        if (left.widget)
        {
            float leftX = GetAnchorPos(left);
            offsetMin.x = leftX - m_rectTransform.anchorMin.x * m_parentTransform.rect.width;

            if (horizontalCenter.widget)
            {
                float centerX = GetAnchorPos(horizontalCenter);
                offsetMax.x = centerX + centerX - leftX - m_rectTransform.anchorMax.x * m_parentTransform.rect.width;
            }
            else if (right.widget)
            {
                float rightX = GetAnchorPos(right);
                offsetMax.x = rightX - m_rectTransform.anchorMax.x * m_parentTransform.rect.width;
            }
            else
            {
                // if fixed size, need adjusting the offsetMax, otherwise keep the right border
                if (m_rectTransform.anchorMax.x == m_rectTransform.anchorMin.x)
                {
                    offsetMax.x = leftX + m_rectTransform.sizeDelta.x - m_rectTransform.anchorMax.x * m_parentTransform.rect.width;
                }
            }
        }
        else if (horizontalCenter.widget)
        {
            float centerX = GetAnchorPos(horizontalCenter);

            if (right.widget)
            {
                float rightX = GetAnchorPos(right);
                offsetMin.x = centerX + (centerX - rightX) + m_rectTransform.anchorMin.x * m_parentTransform.rect.width;
                offsetMax.x = centerX + (rightX - centerX) + m_rectTransform.anchorMax.x * m_parentTransform.rect.width;
            }
            else
            {
                // need adjusting both offsets while keep sizeDelta unchanged
                float halfWidth = m_rectTransform.sizeDelta.x + m_parentTransform.rect.width * (m_rectTransform.anchorMax.x - m_rectTransform.anchorMin.x);
                halfWidth *= 0.5f;
                offsetMin.x = centerX - halfWidth - m_rectTransform.anchorMin.x * m_parentTransform.rect.width;
                offsetMax.x = centerX + halfWidth - m_rectTransform.anchorMax.x * m_parentTransform.rect.width;
            }
        }
        else if (right.widget)
        {
            float rightX = GetAnchorPos(right);
            offsetMax.x = rightX - m_rectTransform.anchorMax.x * m_parentTransform.rect.width;

            // if fixed size, need adjusting the offsetMin, otherwise keep the left border
            if (m_rectTransform.anchorMax.x == m_rectTransform.anchorMin.x)
            {
                offsetMin.x = rightX - m_rectTransform.sizeDelta.x - m_rectTransform.anchorMin.x * m_parentTransform.rect.width;
            }
        }

        if (bottom.widget)
        {
            float bottomY = GetAnchorPos(bottom);
            offsetMin.y = bottomY - m_rectTransform.anchorMin.y * m_parentTransform.rect.height;

            if (verticalCenter.widget)
            {
                float centerY = GetAnchorPos(verticalCenter);
                offsetMax.y = centerY + centerY - bottomY - m_rectTransform.anchorMax.y * m_parentTransform.rect.height;
            }
            else if (top.widget)
            {
                float topY = GetAnchorPos(top);
                offsetMax.y = topY - m_rectTransform.anchorMax.y * m_parentTransform.rect.height;
            }
            else
            {
                // if fixed size, need adjusting the offsetMax, otherwise keep the top border
                if (m_rectTransform.anchorMax.x == m_rectTransform.anchorMin.x)
                {
                    offsetMax.y = bottomY + m_rectTransform.sizeDelta.y - m_rectTransform.anchorMax.y * m_parentTransform.rect.height;
                }
            }
        }
        else if (verticalCenter.widget)
        {
            float centerY = GetAnchorPos(verticalCenter);

            if (top.widget)
            {
                float topY = GetAnchorPos(top);
                offsetMin.y = centerY + (centerY - topY) + m_rectTransform.anchorMin.y * m_parentTransform.rect.height;
                offsetMax.y = centerY + (topY - centerY) + m_rectTransform.anchorMax.y * m_parentTransform.rect.height;
            }
            else
            {
                // need adjusting both offsets while keep sizeDelta unchanged
                float halfHeight = m_rectTransform.sizeDelta.y + m_parentTransform.rect.height * (m_rectTransform.anchorMax.y - m_rectTransform.anchorMin.y);
                halfHeight *= 0.5f;
                offsetMin.x = centerY - halfHeight - m_rectTransform.anchorMin.y * m_parentTransform.rect.height;
                offsetMax.x = centerY + halfHeight - m_rectTransform.anchorMax.y * m_parentTransform.rect.height;
            }
        }
        else if (top.widget)
        {
            float topY = GetAnchorPos(top);
            offsetMax.y = topY - m_rectTransform.anchorMax.y * m_parentTransform.rect.height;

            // if fixed size, need adjusting the offsetMin, otherwise keep the bottom border
            if (m_rectTransform.anchorMax.y == m_rectTransform.anchorMin.y)
            {
                offsetMin.y = topY - m_rectTransform.sizeDelta.y - m_rectTransform.anchorMin.y * m_parentTransform.rect.height;
            }
        }

        m_rectTransform.offsetMin = offsetMin;
        m_rectTransform.offsetMax = offsetMax;
    }

    protected override void OnRectTransformDimensionsChange()
    {
        // ignore callback when doing layout
        if (s_updatingLayout) { return; }

        MarkForLayout();
    }

    public void MarkForLayout()
    {
        if (m_rectTransform && !s_anchors.Contains(this))
        {
            s_anchors.Enqueue(this);
        }
    }

    public static void ForceImmediateLayout()
    {
        if (s_anchors.Count == 0) { return; }

        while (s_anchors.Count > 0)
        {
            var candidate = s_anchors.Dequeue();
            if (!s_anchorStates.ContainsKey(candidate))
            {
                Layout(candidate);
            }
        }
        s_anchors.Clear();
        s_anchorStates.Clear();

        s_updatingLayout = true;
        foreach (var widget in s_sortedWidgets)
        {
            widget.UpdateLayout();
        }
        s_updatingLayout = false;
        s_sortedWidgets.Clear();
    }

    private static void Layout(AnchoredWidget widget)
    {
        VisitState state;
        s_anchorStates.TryGetValue(widget, out state);
        if (state == VisitState.Visited)
        {
            return;
        }
        if (state == VisitState.Visiting)
        {
            throw new InvalidOperationException("cyclic dependency");
        }

        s_anchorStates[widget] = VisitState.Visiting;
        if (widget.m_configs != null)
        {
            s_sortedWidgets.Push(widget);
        }
        
        foreach (var dependent in widget.m_dependents)
        {
            Layout(dependent);
        }

        s_anchorStates[widget] = VisitState.Visited;
    }

#if UNITY_EDITOR
    private static void OnEditorUpdate()
    {
        if (Selection.activeTransform)
        {
            var widget = Selection.activeTransform.GetComponent<AnchoredWidget>();
            if (widget && widget.m_initialized)
            {
                widget.MarkForLayout();
                ForceImmediateLayout();
            }
        }
    }
#endif
}
