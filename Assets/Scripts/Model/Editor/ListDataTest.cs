using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

public class ListDataTest
{
    private ListData m_data;

    [SetUp]
    public void Setup()
    {
        m_data = new ListData("a", NameScope.Local);
    }

    [Test]
    public void TestTypeIsList()
    {
        Assert.AreEqual(BlockVarType.List, m_data.type);
    }

    [Test]
    public void TestSizeAfterReset()
    {
        m_data.add("");
        m_data.reset();
        Assert.AreEqual(0, m_data.size());
    }

    [Test]
    public void TestAddItem()
    {
        m_data.add("1");
        Assert.AreEqual(m_data.size(), 1);
        Assert.AreEqual("1", m_data[1]);

        m_data.add("2");
        Assert.AreEqual(m_data.size(), 2);
        Assert.AreEqual("2", m_data[2]);
    }

    [Test]
    public void TestRemoveItem()
    {
        m_data.add("1");
        m_data.add("2");

        m_data.removeAt(1);
        Assert.AreEqual(1, m_data.size());
        Assert.AreEqual("2", m_data[1]);

        m_data.removeAt(1);
        Assert.AreEqual(0, m_data.size());
    }

    [Test]
    public void TestRemoveOutOfBoundItem()
    {
        m_data.removeAt(0);
        Assert.AreEqual(0, m_data.size());

        m_data.removeAt(1);
        Assert.AreEqual(0, m_data.size());
    }

    [Test]
    public void TestInsertItem()
    {
        m_data.insert(1, "1");
        m_data.insert(1, "0");
        m_data.insert(3, "2");

        Assert.AreEqual(3, m_data.size());

        CollectionAssert.AreEqual(new string[] { "0", "1", "2" }, m_data);
    }

    [Test]
    public void TestInsertOutOfBound()
    {
        m_data.insert(0, "");
        Assert.AreEqual(0, m_data.size());

        m_data.insert(2, "");
        Assert.AreEqual(0, m_data.size());
    }

    [Test]
    public void TestSetItem()
    {
        m_data.add("1");
        m_data.add("2");

        m_data[1] = "2";
        CollectionAssert.AreEqual(new[] { "2", "2" }, m_data);

        m_data[2] = "1";
        CollectionAssert.AreEqual(new[] { "2", "1" }, m_data);
    }

    [Test]
    public void TestSetOutOfBound()
    {
        m_data.add("1");

        m_data[0] = "0";
        CollectionAssert.AreEqual(new[] { "1" }, m_data);

        m_data[2] = "0";
        CollectionAssert.AreEqual(new[] { "1" }, m_data);
    }

    [Test]
    public void TestGetOutOfBoundReturnEmpty()
    {
        Assert.IsEmpty(m_data[1]);
    }
}
