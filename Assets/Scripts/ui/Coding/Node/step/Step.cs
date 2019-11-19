using UnityEngine;
using UnityEngine.Assertions;
using System.Collections.Generic;
using System;
using System.Collections;
using System.Linq;

public delegate void OnStepActionFinish();

public class Step : MonoBehaviour
{
    public GameObject m_Root;
    public GameObject m_Line;
    public float m_DefaultYOffset;
    public StepNode m_ParentNode;
    public LogicTransform m_LogicTransform;
    public float m_HeightAdjustment;

    RectTransform m_Rect;
    float m_OriginPosY;
    float m_OriginalHeight;

    [SerializeField]
    protected List<NodePluginsBase> m_Plugins = new List<NodePluginsBase>();

    [SerializeField] // for cloning
    [ReadOnly]
    FunctionNode m_HeadNode;

    int m_SubNodeIndexInSave;

    void Awake()
    {
        m_Rect = GetComponent<RectTransform>();
        m_OriginPosY = m_Rect.localPosition.y;
        m_OriginalHeight = m_Rect.rect.height;
    }

    public float OriginalPosY
    {
        get { return m_OriginPosY; }
    }

    public float OriginalHeight
    {
        get { return m_OriginalHeight; }
    }

    public void Init()
    {
        if (NodeTemplateCache.Instance.ShowBlockUI)
        {
            HeadConn = m_ParentNode.Connections.First(x => x.line.GetComponentInParent<Step>() == this);
        }

    }

    public Connection HeadConn
    {
        get;
        private set;
    }

    /// <summary>
    /// Insert the newNode after the current node
    /// </summary>
    /// <param name="newNode"></param>
    public void InsertHead(FunctionNode newNode)
    {
        if (newNode == null)
        {
            throw new ArgumentNullException("newNode");
        }

        if (m_HeadNode)
        {
            var curEnd = newNode.GetLastNode();
            m_HeadNode.PrevNode = curEnd;
            curEnd.NextNode = m_HeadNode;
        }

        Head = newNode;

        while (newNode)
        {
            newNode.ParentNode = m_ParentNode;
            newNode.LogicTransform.SetParent(m_LogicTransform);
            newNode = newNode.NextNode;
        }
    }

    public float GetChildrenHeight()
    {
        float totalHeight = 0.0f;
        if (m_HeadNode)
        {
            for (var curNode = m_HeadNode; curNode; curNode = curNode.NextNode)
            {
                totalHeight += curNode.RectTransform.rect.height + curNode.m_HeightAdjustment;
            }
        }
        return totalHeight;
    }

    public int GetChildrenNum()
    {
        int childCount = 0;
        if (m_HeadNode)
        {
            for (var curNode = m_HeadNode; curNode; curNode = curNode.NextNode)
            {
                ++childCount;
            }
        }
        return childCount;
    }

    public void AddPlugins(NodePluginsBase plugin)
    {
        if (plugin == null)
        {
            throw new ArgumentNullException();
        }

        m_Plugins.Add(plugin);
        plugin.transform.SetParent(m_Root.transform);
    }

    public Vector2 Layout()
    {
        var size = NodeLayoutUtil.Layout(m_Rect, m_Plugins, m_DefaultYOffset);
        if (NodeTemplateCache.Instance.ShowBlockUI)
        {
            size.y = Mathf.Max(m_OriginalHeight, size.y - m_HeightAdjustment);
            m_Rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);

            if (m_HeadNode)
            {
                var offsetFromAnchor = new Vector2(0, -m_Rect.rect.height) + NodeConstants.ControlNodeChildOffset;
                var localAnchorPos = Vector2.Scale(new Vector2(0, 1) - m_Rect.pivot, size);
                m_HeadNode.LogicTransform.localPosition = localAnchorPos + offsetFromAnchor;
                m_HeadNode.PositionSiblingsTopDown();
            }
        }
        return size;
    }

    public IEnumerator Run(ThreadContext context)
    {
        var curNode = m_HeadNode;
        while (curNode && !context.isReturned && !context.shouldBreakFromLoop && !context.isAborted)
        {
            while (Time.timeScale == 0.0f)
            {
                yield return null;
            }

            curNode.SetColor(NodeColor.Play, false);

            yield return curNode.ActionBlock(context);

            curNode.SetColor(NodeColor.Normal, false);
            curNode = curNode.NextNode;
        }
    }

    public RectTransform RectTransform
    {
        get { return m_Rect; }
    }

    public LogicTransform LogicTransform
    {
        get { return m_LogicTransform; }
    }

    public FunctionNode Head
    {
        get { return m_HeadNode; }
        internal set
        {
            m_HeadNode = value;
            if (m_HeadNode)
            {
                m_HeadNode.PrevNode = m_ParentNode;
            }
        }
    }

    public void GetStepSaveData(Save_StepData save)
    {
        save.StepSubIndex = (null == m_HeadNode ? 0 : m_HeadNode.NodeIndex);
    }

    public void UnPackStepSaveData(Save_StepData save)
    {
        m_SubNodeIndexInSave = save.StepSubIndex;
    }

    public void PostLoad()
    {
        for (var node = m_HeadNode; node; node = node.NextNode)
        {
            node.ParentNode = m_ParentNode;
            node.LogicTransform.SetParent(m_LogicTransform);
        }
    }

    public void RelinkNodes(ReadOnlyMap<int, FunctionNode> linkMap)
    {
        if (0 != m_SubNodeIndexInSave)
        {
            linkMap.TryGetValue(m_SubNodeIndexInSave, out m_HeadNode);
        }
    }

    public void PostClone(Step other)
    {
        Assert.AreEqual(m_HeadNode != null, other.m_HeadNode != null);

        m_OriginPosY = other.m_OriginPosY;
        m_OriginalHeight = other.m_OriginalHeight;

        var curNode = m_HeadNode;
        var otherNode = other.m_HeadNode;

        while (curNode && otherNode)
        {
            curNode.PostClone(otherNode);

            curNode = curNode.NextNode;
            otherNode = otherNode.NextNode;
        }

        Assert.AreEqual(curNode == null, otherNode == null);
    }
}
