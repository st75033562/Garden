using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

public class VariableManagerTest
{
    private VariableManager m_mgr;

    [SetUp]
    public void Setup()
    {
        m_mgr = new VariableManager();
    }

    [Test]
    public void TestGetByType()
    {
        m_mgr.add(new VariableData("", NameScope.Local));
        Assert.IsNull(m_mgr.get<ListData>(""));
        Assert.NotNull(m_mgr.get<VariableData>(""));
    }

    [Test]
    public void TestGetInvalidName()
    {
        m_mgr.add(new VariableData("a", NameScope.Local));

        Assert.IsNull(m_mgr.get(""));
        Assert.NotNull(m_mgr.get("a"));
    }

    [Test]
    public void TestAddDuplicateNameThrowException()
    {
        m_mgr.add(new VariableData("a", NameScope.Local));

        Assert.Throws(typeof(ArgumentException), () => {
            m_mgr.add(new VariableData("a", NameScope.Local));
        });

        Assert.Throws(typeof(ArgumentException), () => {
            m_mgr.add(new ListData("a", NameScope.Local));
        });
    }

    [Test]
    public void TestResetVars()
    {
        m_mgr.add(new VariableData("a", NameScope.Local));
        m_mgr.add(new VariableData("b", NameScope.Global));

        m_mgr.setVar("a", 1.0f);
        m_mgr.setVar("b", 1.0f);

        m_mgr.reset();
        Assert.AreEqual(0.0f, m_mgr.get<VariableData>("a").getValue());
        Assert.AreEqual(0.0f, m_mgr.get<VariableData>("b").getValue());
    }

    [Test]
    public void TestResetLocalVars()
    {
        m_mgr.add(new VariableData("a", NameScope.Local));
        m_mgr.add(new VariableData("b", NameScope.Global));

        m_mgr.setVar("a", 1.0f);
        m_mgr.setVar("b", 1.0f);

        m_mgr.reset(false);
        Assert.AreEqual(0.0f, m_mgr.get<VariableData>("a").getValue());
        Assert.AreEqual(1.0f, m_mgr.get<VariableData>("b").getValue());
    }
}
