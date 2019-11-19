using Google.Protobuf;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Profiling;

public class StepNode : FunctionNode
{
    [SerializeField]
    private List<Step> m_StepData = new List<Step>();

    protected override void Awake()
    {
        base.Awake();

        foreach (var step in m_StepData)
        {
            step.Init();
        }
    }

    protected override IEnumerable<FunctionNode> InnerChildren
    {
        get
        {
            foreach (var step in m_StepData)
            {
                for (var node = step.Head; node; node = node.NextNode)
                {
                    yield return node;
                }
            }
        }
    }

    protected override void DoRemoveChildren(FunctionNode firstNode, FunctionNode lastNode)
    {
        foreach (var step in m_StepData)
        {
            if (step.Head == firstNode)
            {
                // clear PrevNode so that Disconnect can work correctly
                firstNode.PrevNode = null;
                step.Head = lastNode.NextNode;
                break;
            }
        }

        base.DoRemoveChildren(firstNode, lastNode);
    }

    public override UnPluggedNodeInfo Connect(FunctionNode newNode, Connection target)
    {
        if (target.type == ConnectionTypes.SubBottom)
        {
            var step = GetStep(target);
            Assert.IsNotNull(step, "invalid connection");

            step.InsertHead(newNode);
            LayoutBottomUp();

            return null;
        }
        else
        {
            return base.Connect(newNode, target);
        }
    }

    public override bool CanConnect(FunctionNode node, Connection target)
    {
        if (target.type == ConnectionTypes.SubBottom)
        {
            if (!target.IsMatched(node.TopConn))
            {
                return false;
            }

            var step = GetStep(target);
            Assert.IsNotNull(step, "invalid connection");
            
            if (step.Head && !step.Head.TopConn.IsMatched(node.GetLastNode().BottomConn))
            {
                return false;
            }

            return true;
        }
        else
        {
            return base.CanConnect(node, target);
        }
    }

    protected override bool CanConnectChildTop(FunctionNode node, Connection target)
    {
        // check if the node of the target connection is the head node
        var step = m_StepData.Find(x => x.Head == target.node);
        if (step)
        {
            // check if node is conectable to the step
            return CanConnect(node, step.HeadConn);
        }
        else
        {
            // check if the previous node is connectable to node
            return target.node.PrevNode.BottomConn.IsMatched(node.TopConn);
        }
    }

    Step GetStep(Connection conn)
    {
        return m_StepData.Find(x => x.m_Line == conn.line);
    }

    public void AddPluginsToStep(NodePluginsBase plugin, int index)
    {
        if (plugin == null)
        {
            throw new ArgumentNullException();
        }

        if (index < 0 || index >= m_StepData.Count)
        {
            return;
        }

        Step mStep = m_StepData[index];
        mStep.AddPlugins(plugin);
    }

    protected override Connection GetPrevConnection(FunctionNode childNode)
    {
        foreach (var step in m_StepData)
        {
            if (step.Head == childNode)
            {
                return GetConnection(step.m_Line);
            }
        }

        return base.GetPrevConnection(childNode);
    }

    public override void Layout()
    {
        Profiler.BeginSample("StepNode.Layout");

        Vector2 nodeSize = new Vector2(0, m_OriginalHeight);
        float deltaHeight = 0;
        for (int i = 0; i < m_StepData.Count; ++i)
        {
            var step = m_StepData[i];
            var size = step.Layout();
            nodeSize.x = Mathf.Max(size.x, nodeSize.x);

            float stepDeltaHeight = size.y - m_StepData[i].OriginalHeight;

            step.LogicTransform.localPosition = new Vector3(0, step.OriginalPosY - deltaHeight);
            deltaHeight += stepDeltaHeight;
            if (step.Head)
            {
                deltaHeight += step.GetChildrenHeight() - NodeConstants.ControlNodeInnerHeight;
            }
        }
        nodeSize.y += deltaHeight;
        nodeSize.x = Mathf.Max(nodeSize.x, m_MinWidth);

        m_RectTransform.sizeDelta = nodeSize;

        Profiler.EndSample();
    }

    public IEnumerator ActionStep(ThreadContext context, int index)
    {
        yield return m_StepData[index].Run(context);
    }

    protected override IMessage PackNodeSaveData()
    {
        Save_StepNode tSave = new Save_StepNode();
        tSave.BaseData = PackBaseNodeSaveData();
        for (int i = 0; i < m_StepData.Count; ++i)
        {
            Save_StepData tStepData = new Save_StepData();
            m_StepData[i].GetStepSaveData(tStepData);
            tSave.StepSubData.Add(tStepData);
        }
        return tSave;
    }

    protected override void UnPackNodeSaveData(byte[] nodeData)
    {
        Save_StepNode tSave = Save_StepNode.Parser.ParseFrom(nodeData);
        UnPackBaseNodeSaveData(tSave.BaseData);

        for (int i = 0; i < m_StepData.Count; ++i)
        {
            Step tCurStep = m_StepData[i];
            tCurStep.UnPackStepSaveData(tSave.StepSubData[i]);
        }
    }

    public override void RelinkNodes(ReadOnlyMap<int, FunctionNode> linkMap)
    {
        base.RelinkNodes(linkMap);

        for (int i = 0; i < m_StepData.Count; ++i)
        {
            m_StepData[i].RelinkNodes(linkMap);
        }
    }

    public override void PostLoad()
    {
        foreach (var step in m_StepData)
        {
            step.PostLoad();
        }
    }

    internal override void PostClone(FunctionNode other)
    {
        base.PostClone(other);

        var otherSteps = ((StepNode)other).m_StepData;
        Assert.AreEqual(m_StepData.Count, otherSteps.Count);

        foreach (var pair in m_StepData.Zip(otherSteps))
        {
            pair.first.PostClone(pair.second);
        }
    }
}
