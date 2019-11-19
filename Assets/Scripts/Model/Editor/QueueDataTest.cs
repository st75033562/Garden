using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

public class QueueDataTest
{
    private QueueData m_data;

    [SetUp]
    public void Setup()
    {
        m_data = new QueueData();
    }

    [Test]
    public void TestType()
    {
        Assert.AreEqual(BlockVarType.Queue, m_data.type);
    }

    [Test]
    public void TestEnqueueAndDequeue()
    {
        m_data.enqueue("1");
        m_data.enqueue("2");

        CollectionAssert.AreEqual(new[] { "1", "2" }, m_data);

        Assert.AreEqual("1", m_data.dequeue());
        Assert.AreEqual("2", m_data.dequeue());
        Assert.AreEqual(0, m_data.size());
    }

    [Test]
    public void TestDequeueEmptyReturnEmpty()
    {
        Assert.IsEmpty(m_data.dequeue());
    }
}
