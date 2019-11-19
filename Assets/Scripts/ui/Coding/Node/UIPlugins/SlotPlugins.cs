using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;

public class SlotPlugins : NodePluginsBase
{
    public GameObject m_BackGround;
    public RectTransform m_Line;
    public Connection m_Connection;

    private static readonly Vector2 m_Padding = new Vector2(0, 0);

    [SerializeField] // for cloning
    [ReadOnly]
    protected FunctionNode m_Insert;
    protected int m_InsertIndexInSave;

    Vector2 m_RT;
    Vector2 m_LB;

    protected override void Awake()
    {
        base.Awake();
        m_RT = m_Line.offsetMax;
        m_LB = m_Line.offsetMin;
        Insertable = true;
        m_Connection.targetStateChecker = () => Insertable;
    }

    protected virtual void ShowBackground(bool visible)
    {
        if (m_BackGround)
        {
            m_BackGround.SetActive(visible);
        }

        var graphic = GetComponent<Graphic>();
        if (graphic)
        {
            graphic.enabled = visible;
        }
    }

    public override void CopyDataToTarget(NodePluginsBase target)
    {
        base.CopyDataToTarget(target);

        var other = (SlotPlugins)target;
        other.m_BackGround = m_BackGround;
        other.m_Line = m_Line;
        other.m_Connection = m_Connection;
    }

    private bool m_insertable = true;
    public bool Insertable
    {
        get { return m_insertable; }
        set
        {
            m_insertable = value;
        }
    }

    public void Insert(FunctionNode node)
    {
        if (node == null)
        {
            throw new ArgumentNullException("node");
        }

        if (!node.Interactable)
        {
            throw new ArgumentException("node not insertable");
        }

        if (m_Insert)
        {
            throw new InvalidOperationException();
        }

        if (!Insertable)
        {
            return;
        }

        m_Line.offsetMax = Vector2.zero;
        m_Line.offsetMin = Vector2.zero;
        ShowBackground(false);

        m_Insert = node;
        m_Insert.ParentNode = m_MyNode;
        node.LogicTransform.SetParent(ParentNode.LogicTransform);
    }

    public void AlignInsertion()
    {
        if (m_Insert)
        {
            var leftBottom = (Vector2)m_Rect.localPosition + m_Rect.BottomLeft();
            var offset = Vector2.Scale(m_Insert.RectTransform.rect.size, m_Insert.RectTransform.pivot);
            offset += m_Padding * 0.5f;
            m_Insert.LogicTransform.localPosition = leftBottom + offset;
        }
    }

    public void RemoveInsertion()
    {
        m_Line.offsetMax = m_RT;
        m_Line.offsetMin = m_LB;
        ShowBackground(true);

        if (m_Insert)
        {
            m_Insert.LogicTransform.parent = null;
            m_Insert.ParentNode = null;
            m_Insert = null;
        }
    }

    public override FunctionNode ParentNode
    {
        set
        {
            base.ParentNode = value;
            if (m_Insert)
            {
                m_Insert.ParentNode = value;
            }
        }
    }

    public FunctionNode InsertedNode
    {
        get { return m_Insert; }
    }

    public virtual IEnumerator GetSlotValue(ThreadContext context, ValueWrapper<string> retValue)
    {
        if (m_Insert)
        {
            yield return m_Insert.GetReturnValue(context, retValue);
        }
        else
        {
            retValue.value = m_TextKey;
        }
    }

    protected override void OnInput(string str)
    {
        EventBus.Default.AddEvent(EventId.GuideInput, new GuideInputBackData(gameObject, str));
        ChangePluginsText(str);
    }

    public override void Layout()
    {
        if (m_Insert)
        {
            m_Rect.SetSize(m_Insert.RectTransform.rect.size + m_Padding);
        }
        else
        {
            base.Layout();
        }
    }

    public override void LayoutChild()
    {
        AlignInsertion();
    }

    public override Save_PluginsData GetPluginSaveData()
    {
        Save_PluginsData tSaveData = base.GetPluginSaveData();
        tSaveData.PluginTextValue = m_TextKey;
        tSaveData.PluginIntValue = (null == m_Insert ? 0 : m_Insert.NodeIndex);
        return tSaveData;
    }

    public override void LoadPluginSaveData(Save_PluginsData save)
    {
        base.LoadPluginSaveData(save);
        SetPluginsText(save.PluginTextValue);
        m_InsertIndexInSave = save.PluginIntValue;
    }

    public void RelinkNodes(ReadOnlyMap<int, FunctionNode> linkMap)
    {
        if (0 != m_InsertIndexInSave)
        {
            FunctionNode tCurNode;
            if (linkMap.TryGetValue(m_InsertIndexInSave, out tCurNode))
            {
                Insert(tCurNode);
            }
        }
    }

    void OnDrawGizmos()
    {
        var trans = GetComponent<RectTransform>();
        Gizmos.matrix = trans.localToWorldMatrix;
        Gizmos.DrawWireCube(new Vector2(trans.rect.width * 0.5f, 0), trans.rect.size - m_Padding);
    }
}
