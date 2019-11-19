using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;
using System;

public class MessageManagerTest
{
    private MessageManager m_manager;

    [SetUp]
    public void Setup()
    {
        m_manager = new MessageManager();
    }

    [Test]
    public void AddLocalMessage()
    {
        m_manager.add(new Message("1", NameScope.Local));
        Assert.AreEqual(1, m_manager.count);
    }

    [Test]
    public void AddDuplicateThrowsException()
    {
        m_manager.add(new Message("1", NameScope.Local));
        Assert.Throws<ArgumentException>(delegate {
            m_manager.add(new Message("1", NameScope.Local));
        });
    }

    [Test]
    public void DeleteExistingMessage()
    {
        m_manager.add(new Message("1", NameScope.Local));
        Assert.AreEqual(1, m_manager.count);
        m_manager.delete("1");
        Assert.AreEqual(0, m_manager.count);
    }

    [Test]
    public void DeleteNonExistingMessage()
    {
        m_manager.add(new Message("1", NameScope.Local));
        Assert.AreEqual(1, m_manager.count);
        m_manager.delete("0");
        Assert.AreEqual(1, m_manager.count);
    }

    [Test]
    public void ClearMessages()
    {
        m_manager.add(new Message("1", NameScope.Local));
        m_manager.reset();
        Assert.AreEqual(0, m_manager.count);
    }
}
