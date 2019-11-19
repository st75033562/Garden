using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

public class VariableDataTest
{
    private VariableData m_data;

    [SetUp]
    public void Setup()
    {
        m_data = new VariableData("a", NameScope.Local);
    }

    [Test]
    public void TestTypeIsVariable()
    {
        Assert.AreEqual(BlockVarType.Variable, m_data.type);
    }

    [Test]
    public void TestDefaultValue()
    {
        Assert.AreEqual(0, m_data.getValue());
        Assert.True(m_data.isNumber());
        Assert.AreEqual("0", m_data.getString());
    }

    [Test]
    public void TestSetStringValue()
    {
        const string Value = "string";

        m_data.setValue(Value);
        Assert.IsFalse(m_data.isNumber());
        Assert.AreEqual(m_data.getString(), Value);
        Assert.AreEqual(0, m_data.getValue());
    }

    [Test]
    public void TestSetNumericValueAfterSettingStringValue()
    {
        m_data.setValue("");
        m_data.setValue(1.0f);
        Assert.IsTrue(m_data.isNumber());
        Assert.AreEqual(1.0f, m_data.getValue());
    }

    [Test]
    public void TestSetStringValueAfterSettingNumericValue()
    {
        m_data.setValue(1.0f);
        m_data.setValue("");

        Assert.IsFalse(m_data.isNumber());
        Assert.AreEqual("", m_data.getString());
        Assert.AreEqual(0, m_data.getValue());
    }

    [Test]
    public void TestChangeStringValueShouldTriggerChangedEvent()
    {
        m_data.setValue("a");

        bool triggered = false;
        m_data.onChanged += x => {
            triggered = true;
        };

        m_data.setValue("");
        Assert.IsTrue(triggered);
    }

    [Test]
    public void TestChangeNumericValueShouldTriggerChangedEvent()
    {
        Assert.IsTrue(m_data.isNumber());

        bool triggered = false;
        m_data.onChanged += x => {
            triggered = true;
        };

        m_data.setValue(1.0f);
        Assert.IsTrue(triggered);
    }

    [Test]
    public void TestChangeStringValueForANumericDataShouldTriggerChangedEvent()
    {
        Assert.IsTrue(m_data.isNumber());

        bool triggered = false;
        m_data.onChanged += x => {
            triggered = true;
        };

        m_data.setValue("hello");
        Assert.IsTrue(triggered);
    }

    [Test]
    public void TestAddValueToNumericValue()
    {
        m_data.addValue(1.0f);
        Assert.AreEqual(1.0f, m_data.getValue());
    }

    [Test]
    public void TestAddValueToStringValueShouldNotChangeValue()
    {
        m_data.setValue("");
        m_data.addValue(1.0f);

        Assert.IsFalse(m_data.isNumber());
        Assert.AreEqual("", m_data.getString());
    }

    [Test]
    public void TestAddValueToNumericValueShouldTriggerChangedEvent()
    {
        bool triggered = false;
        m_data.onChanged += x => {
            triggered = true;
        };

        m_data.addValue(1.0f);
        Assert.IsTrue(triggered);
    }

    [Test]
    public void TestReset()
    {
        m_data.setValue("");
        m_data.reset();
        TestDefaultValue();
    }
}
