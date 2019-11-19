using System.Linq;
using NUnit.Framework;
using System.Collections.Generic;

public class IntrusiveLinkedListTest
{
    private class Node : IntrusiveListNode<Node>
    {
        public int value;

        public Node(int value)
        {
            this.value = value;
        }
    }

    private IntrusiveList<Node> m_list;

    [SetUp]
    public void Setup()
    {
        m_list = new IntrusiveList<Node>();
    }

    [Test]
    public void AddLastToEmptyList()
    {
        var node = new Node(1);
        m_list.AddLast(node);

        Assert.AreSame(node, m_list.last);
    }

    [Test]
    public void AddLastToNonEmptyList()
    {
        var node0 = new Node(0);
        m_list.AddLast(node0);
        var node1 = new Node(1);
        m_list.AddLast(node1);

        Assert.AreSame(node0, m_list.first);
        Assert.AreSame(node1, m_list.last);
    }

    [Test]
    public void AddFirstToEmptyList()
    {
        var node = new Node(1);
        m_list.AddFirst(node);

        Assert.AreSame(node, m_list.first);
    }

    [Test]
    public void AddFirstToNonEmptyList()
    {
        var node0 = new Node(0);
        m_list.AddFirst(node0);
        var node1 = new Node(1);
        m_list.AddFirst(node1);

        Assert.AreSame(node1, m_list.first);
        Assert.AreSame(node0, m_list.last);
    }

    [Test]
    public void AddAfter()
    {
        var node0 = new Node(0);
        m_list.AddLast(node0);

        var node1 = new Node(1);
        m_list.AddAfter(node0, node1);

        Assert.AreSame(node0.next, node1);
        Assert.AreSame(node1.prev, node0);
    }

    [Test]
    public void AddBefore()
    {
        var node0 = new Node(0);
        m_list.AddLast(node0);

        var node1 = new Node(1);
        m_list.AddBefore(node0, node1);

        Assert.AreSame(node0.prev, node1);
        Assert.AreSame(node1.next, node0);
    }

    [Test]
    public void Remove()
    {
        var node0 = new Node(0);
        var node1 = new Node(1);

        m_list.AddLast(node0);
        m_list.AddLast(node1);

        Assert.AreSame(m_list.Remove(node0), node1);
        Assert.AreSame(m_list.Remove(node1), null);
    }

    [Test]
    public void TestEnumerator()
    {
        var nodes = new List<Node>();
        foreach (var value in Enumerable.Range(0, 4))
        {
            m_list.AddLast(new Node(value));
            nodes.Add(m_list.last);
        }

        CollectionAssert.AreEqual(nodes, m_list);
    }

    [Test]
    public void Clear()
    {
        var node0 = new Node(0);
        var node1 = new Node(1);
        m_list.AddLast(node0);
        m_list.AddLast(node1);

        m_list.Clear();
        AssertNodeNotInList(node0);
        AssertNodeNotInList(node1);
    }

    private static void AssertNodeNotInList(Node node)
    {
        Assert.IsNull(node.list);
        Assert.IsNull(node.prev);
        Assert.IsNull(node.next);
    }
}
