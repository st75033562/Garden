using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A logic transform in a logic tree
/// </summary>
/// <remarks>
/// A hierarchy of Unity transforms form a visual tree.
/// Each of the unity transforms can have a corresponding logic transform.
/// The unity transforms should have uniform scales and identity rotations.
/// </remarks>
public class LogicTransform : MonoBehaviour, IEnumerable<LogicTransform>
{
    [SerializeField] private LogicTransform m_firstChild;
    [SerializeField] private LogicTransform m_lastChild;
    [SerializeField] private LogicTransform m_parent;
    [SerializeField] private LogicTransform m_prevSibling;
    [SerializeField] private LogicTransform m_nextSibling;

    private Transform m_visualTarget;
    private Vector3 m_localPosition;
    // invariant: the logic world position equals to the local position of 
    // the visual transform in space of the parent of the root visual transform
    private Vector3 m_worldPosition;

    private float m_globalScale; // assume uniform scale

    // invariant: if dirty, all the children are dirty
    private bool m_dirty;

    private static readonly HashSet<LogicTransform> s_dirtyRoots = new HashSet<LogicTransform>();

    private void Awake()
    {
        m_visualTarget = transform;
        localPosition = transform.localPosition;
    }

    private void OnDestroy()
    {
        s_dirtyRoots.Remove(this);
    }

    public Transform visualTarget
    {
        get { return m_visualTarget; }
    }

    /// <summary>
    /// change the parent transform
    /// </summary>
    /// <param name="parent">the new parent transform, can be null</param>
    /// <param name="worldPositionStays">true if the world position should remain unchanged</param>
    public void SetParent(LogicTransform parent, bool worldPositionStays = true)
    {
        if (m_parent == parent)
        {
            return;
        }

        Vector3 oldPosition = Vector2.zero;
        if (worldPositionStays)
        {
            oldPosition = worldPosition;
        }

        if (parent != null)
        {
            parent.AddChild(this);
        }
        else
        {
            m_parent.RemoveChild(this);
        }

        if (worldPositionStays)
        {
            worldPosition = oldPosition;
        }
    }

    /// <summary>
    /// Add the transform as the child, localPosition is not changed
    /// </summary>
    public void AddChild(LogicTransform child)
    {
        if (child == null)
        {
            throw new ArgumentNullException("child");
        }

        if (child.parent == this)
        {
            return;
        }

        if (child.parent != null)
        {
            child.parent.RemoveChild(child);
        }

        if (m_firstChild == null)
        {
            m_firstChild = m_lastChild = child;
        }
        else
        {
            m_lastChild.m_nextSibling = child;
            child.m_prevSibling = m_lastChild;
            m_lastChild = child;
        }
        child.m_parent = this;
        child.MarkDirty();
        s_dirtyRoots.Remove(child);
    }

    /// <summary>
    /// remove the child transform, localPosition is not changed
    /// </summary>
    public void RemoveChild(LogicTransform child)
    {
        if (child == null)
        {
            throw new ArgumentNullException("child");
        }

        if (child.parent != this)
        {
            throw new ArgumentException("not a child", "child");
        }

        if (child.nextSibling != null)
        {
            child.nextSibling.m_prevSibling = child.prevSibling;
        }
        else
        {
            m_lastChild = child.prevSibling;
        }

        if (child.prevSibling != null)
        {
            child.prevSibling.m_nextSibling = child.nextSibling;
        }
        else
        {
            m_firstChild = child.nextSibling;
        }

        child.m_prevSibling = child.m_nextSibling = child.m_parent = null;
        child.MarkDirty();
    }

    public LogicTransform firstChild { get { return m_firstChild; } }

    public LogicTransform lastChild { get { return m_lastChild; } }

    /// <summary>
    /// set the parent of the transform, worldPosition is not changed
    /// </summary>
    public LogicTransform parent
    {
        get { return m_parent; }
        set { SetParent(value, true); }
    }

    /// <summary>
    /// the logic root
    /// </summary>
    public LogicTransform root
    {
        get
        {
            var cur = this;
            while (cur.parent)
            {
                cur = cur.parent;
            }
            return cur;
        }
    }

    public LogicTransform prevSibling { get { return m_prevSibling; } }
            
    public LogicTransform nextSibling { get { return m_nextSibling; } }

    public Vector3 localPosition
    {
        get { return m_localPosition; }
        set
        {
            if (m_localPosition != value)
            {
                m_localPosition = value;
                MarkDirty();
            }
        }
    }

    /// <summary>
    /// Mark the hierarchy dirty.
    /// In case you change the local scale of the visual transform, you should mark the hierarchy dirty explicitly.
    /// </summary>
    public void MarkDirty()
    {
        if (m_dirty)
        {
            return;
        }

        s_dirtyRoots.Add(root);
        InternalMarkDirty();
    }

    private void InternalMarkDirty()
    {
        if (m_dirty)
        {
            return;
        }

        m_dirty = true;
        for (var child = firstChild; child != null; child = child.nextSibling)
        {
            child.InternalMarkDirty();
        }
    }

    public Vector3 worldPosition
    {
        get
        {
            UpdateTransform();
            return m_worldPosition;
        }
        set
        {
            // no check for the change as m_worldPosition may be invalid
            m_worldPosition = value;
            if (parent != null)
            {
                localPosition = (value - parent.worldPosition) / parent.m_globalScale;
            }
            else
            {
                localPosition = value;
            }
        }
    }

    public Vector3 visualWorldPosition
    {
        get
        {
            UpdateTransform();
            return visualTarget.position;
        }
    }

    /// <summary>
    /// only update the current transform, children are not updated
    /// </summary>
    public void UpdateTransform()
    {
        UpdateUpwards();
        DoUpdateTransform();
    }

    /// <summary>
    /// update the current transform and all the children. All the dirty ancestors are updated as well.
    /// </summary>
    public void UpdateHierarchy()
    {
        s_dirtyRoots.Remove(this);
        UpdateUpwards();
        UpdateDownwards();
    }

    // update self and all the dirty parent transforms
    private void UpdateUpwards()
    {
        if (parent != null && parent.m_dirty)
        {
            parent.UpdateUpwards();
        }
        DoUpdateTransform();
    }

    // update self and all the child transforms
    private void UpdateDownwards()
    {
        DoUpdateTransform();
        for (var child = firstChild; child != null; child = child.nextSibling)
        {
            child.UpdateDownwards();
        }
    }

    private void DoUpdateTransform()
    {
        if (!NodeTemplateCache.Instance.ShowBlockUI) {
            return;
        }
        if (!m_dirty)
        {
            return;
        }

        Debug.Assert(parent == null || !parent.m_dirty, "Parent must be clean");

        m_dirty = false;
        if (parent != null)
        {
            // calculate the global scale
            m_globalScale = parent.m_globalScale * m_visualTarget.localScale.x;

            // calculate the world position in the logic tree
            m_worldPosition = parent.m_worldPosition + parent.m_globalScale * m_localPosition;

            // update the local position of the visual transform
            if (m_visualTarget.parent)
            {
                // check if the visual parent is in the logic tree
                var realParent = m_visualTarget.parent.GetComponent<LogicTransform>();
                if (realParent)
                {
                    m_visualTarget.localPosition = (m_worldPosition - realParent.m_worldPosition) / realParent.m_globalScale;
                }
                else
                {
                    m_visualTarget.localPosition = m_worldPosition;
                }
            }
            else
            {
                m_visualTarget.localPosition = m_worldPosition;
            }
        }
        else
        {
            m_worldPosition = m_localPosition;
            m_visualTarget.localPosition = m_localPosition;
            m_globalScale = m_visualTarget.localScale.x;
        }
    }

    /// <summary>
    /// update the position from the current visual position
    /// </summary>
    public void SyncPosition()
    {
        var visualRootParent = root.visualTarget.parent;
        if (visualRootParent)
        {
            worldPosition = visualRootParent.InverseTransformPoint(visualTarget.position);
        }
        else
        {
            worldPosition = visualTarget.position;
        }
    }

    /// <summary>
    /// get an enumerator to return all the children
    /// </summary>
    /// <returns></returns>
    public IEnumerator<LogicTransform> GetEnumerator()
    {
        for (var child = firstChild; child != null; child = child.nextSibling)
        {
            yield return child;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    /// Update all dirty transforms
    /// </summary>
    public static void FlushPendingUpdates()
    {
        if (s_dirtyRoots.Count > 0)
        {
            foreach (var root in s_dirtyRoots)
            {
                root.UpdateDownwards();
            }
            s_dirtyRoots.Clear();
        }
    }
}
