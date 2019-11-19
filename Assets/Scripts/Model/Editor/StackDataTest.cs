using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

public class StackDataTest
{
    private StackData m_data;

    [Test]
    public void Setup()
    {
        m_data = new StackData("a", NameScope.Local);
    }

    [Test]
    public void TestType()
    {
        Assert.AreEqual(BlockVarType.Stack, m_data.type);
    }

    [Test]
    public void TestPushAndPop()
    {
        m_data.push("1");
        m_data.push("2");

        CollectionAssert.AreEqual(new[] { "2", "1" }, m_data);

        Assert.AreEqual("2", m_data.pop());
        Assert.AreEqual("1", m_data.pop());
        Assert.AreEqual(0, m_data.size());
    }

    [Test]
    public void TestPopEmptyReturnEmpty()
    {
        Assert.IsEmpty(m_data.pop());
    }

    [Test]
    public void TestReset()
    {
        m_data.push("1");
        m_data.reset();

        Assert.AreEqual(0, m_data.size());
    }
}
