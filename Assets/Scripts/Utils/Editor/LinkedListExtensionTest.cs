using NUnit.Framework;
using System;
using System.Collections.Generic;

public class LinkedListExtensionTest
{
    private LinkedList<int> m_list;

    [SetUp]
    public void Setup()
    {
        m_list = new LinkedList<int>();
        m_list.AddLast(1);
        m_list.AddLast(2);
        m_list.AddLast(3);
        m_list.AddLast(1);
        m_list.AddLast(2);
    }

    [Test]
    public void TestRemoveAll()
    {
        m_list.RemoveAll(x => x == 1);
        Assert.AreEqual(3, m_list.Count);
        CollectionAssert.AreEqual(new[] { 2, 3, 2 }, m_list);
    }

    [Test]
    public void TestRemoveAllFromSecond()
    {
        m_list.RemoveAll(m_list.First.Next, x => x == 1);
        Assert.AreEqual(4, m_list.Count);
        CollectionAssert.AreEqual(new[] { 1, 2, 3, 2 }, m_list);
    }

    [Test]
    public void TestRemoveInvalidNode()
    {
        Assert.Throws<ArgumentException>( delegate {
            m_list.RemoveAll(new LinkedListNode<int>(1), delegate { return false; });
        });
    }

    [Test]
    public void TestContains()
    {
        Assert.IsTrue(m_list.Contains(x => x == 1));
        Assert.IsTrue(m_list.Contains(x => x == 2));
        Assert.IsFalse(m_list.Contains(x => x == 4));
    }
}
