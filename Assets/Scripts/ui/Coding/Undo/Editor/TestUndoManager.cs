using NUnit.Framework;
using System;

public class UndoStub : SimpleUndoCommand
{
    private readonly Action m_undo;
    private readonly Action m_redo;

    public UndoStub(Action undo = null, Action redo = null, bool hasSideEffect = true)
        : base(undo ?? delegate {}, redo ?? delegate {}, hasSideEffect)
    {
    }
}

[TestFixture]
public class TestUndoManager
{
    private UndoManager m_undoManager;

    [SetUp]
    public void Setup()
    {
        m_undoManager = new UndoManager();
    }

    [Test]
    public void InitiallyShouldBeClean()
    {
        Assert.IsTrue(m_undoManager.IsClean());
    }

    [Test]
    public void InitiallyUndoShouldBeEnabled()
    {
        Assert.IsTrue(m_undoManager.undoEnabled);
    }

    [Test]
    public void RedoShouldBeCalledOnAdd_IfRedoIsTrue()
    {
        bool redoCalled = false;
        m_undoManager.AddUndo(new UndoStub(redo: () => redoCalled = true));

        Assert.IsTrue(redoCalled);
    }

    [Test]
    public void RedoShouldNotCalledOnAdd_IfRedoIsFalse()
    {
        bool redoCalled = false;
        m_undoManager.AddUndo(new UndoStub(redo: () => redoCalled = true), false);

        Assert.IsFalse(redoCalled);
    }

    [Test]
    public void UndoStackSizeShouldBeIncreased_RegardlessOf_RedoValue()
    {
        m_undoManager.AddUndo(new UndoStub(), true);
        Assert.AreEqual(1, m_undoManager.UndoStackSize);

        m_undoManager.AddUndo(new UndoStub(), false);
        Assert.AreEqual(2, m_undoManager.UndoStackSize);
    }

    [Test]
    public void RedoStackSizeShouldBeZero_IfUndosAreAdded()
    {
        m_undoManager.AddUndo(new UndoStub(), true);
        Assert.AreEqual(0, m_undoManager.RedoStackSize);

        m_undoManager.AddUndo(new UndoStub(), false);
        Assert.AreEqual(0, m_undoManager.RedoStackSize);
    }

    [Test]
    public void UndoShouldBeCalledOnUndo_And_StackSizeIsCorrect()
    {
        bool undoCalled = false;
        m_undoManager.AddUndo(new UndoStub(undo: () => undoCalled = true));

        Assert.IsTrue(m_undoManager.Undo());
        Assert.IsTrue(undoCalled);
        Assert.AreEqual(1, m_undoManager.RedoStackSize);
        Assert.AreEqual(0, m_undoManager.UndoStackSize);
    }

    [Test]
    public void RedoShouldBeCalledOnRedo_And_StackSizeIsCorrect()
    {
        var redoCalled = false;
        m_undoManager.AddUndo(new UndoStub(redo: () => redoCalled = true), false);
        m_undoManager.Undo();

        Assert.IsTrue(m_undoManager.Redo());
        Assert.IsTrue(redoCalled);
        Assert.AreEqual(0, m_undoManager.RedoStackSize);
        Assert.AreEqual(1, m_undoManager.UndoStackSize);
    }

    [Test]
    public void UndoRedoChangesClean()
    {
        m_undoManager.AddUndo(new UndoStub());
        Assert.IsFalse(m_undoManager.IsClean());

        m_undoManager.Undo();
        Assert.IsTrue(m_undoManager.IsClean());

        m_undoManager.Redo();
        Assert.IsFalse(m_undoManager.IsClean());
    }


    [Test]
    public void CleanShouldBeFalse_AfterAddUndo_WhenDisabled()
    {
        m_undoManager.stackEnabled = false;

        m_undoManager.AddUndo(new UndoStub());

        Assert.IsFalse(m_undoManager.IsClean());
    }

    [Test]
    public void UndoRedoDoesNotChangeClean_WhenDisabled()
    {
        m_undoManager.stackEnabled = false;
        m_undoManager.AddUndo(new UndoStub());

        m_undoManager.Undo();
        Assert.IsFalse(m_undoManager.IsClean());

        m_undoManager.Redo();
        Assert.IsFalse(m_undoManager.IsClean());
    }

    [Test]
    public void CleanShouldBeTrue_AfterSetClean()
    {
        m_undoManager.AddUndo(new UndoStub());

        m_undoManager.SetClean(true);
        Assert.IsTrue(m_undoManager.IsClean());
    }

    [Test]
    public void CleanShouldAlternate_WhenUndoRedo_IfCleanWasFalseAndThenSetClean()
    {
        m_undoManager.AddUndo(new UndoStub());
        m_undoManager.SetClean(true);

        m_undoManager.Undo();
        Assert.IsFalse(m_undoManager.IsClean());

        m_undoManager.Redo();
        Assert.IsTrue(m_undoManager.IsClean());
    }

    [Test]
    public void CleanShouldAlternate_WhenRedoUndo_IfCleanWasFalseAndThenSetClean()
    {
        m_undoManager.AddUndo(new UndoStub());
        m_undoManager.Undo();
        m_undoManager.SetClean(true);

        m_undoManager.Redo();
        Assert.IsFalse(m_undoManager.IsClean());

        m_undoManager.Undo();
        Assert.IsTrue(m_undoManager.IsClean());
    }

    [Test]
    public void SetCleanShouldThrow_InvalidOperationException_IfMacroStackIsNotEmpty()
    {
        m_undoManager.BeginMacro();
        Assert.Throws<InvalidOperationException>(() => m_undoManager.SetClean(true));
    }

    [Test]
    public void CleanShouldRemainFalse_AfterSetToFalse_UntilSetToTrue()
    {
        m_undoManager.AddUndo(new UndoStub());
        m_undoManager.SetClean(true);

        m_undoManager.SetClean(false);

        m_undoManager.Undo();
        Assert.IsFalse(m_undoManager.IsClean());

        m_undoManager.Redo();
        Assert.IsFalse(m_undoManager.IsClean());

        m_undoManager.SetClean(true);
        Assert.IsTrue(m_undoManager.IsClean());
    }

    [Test]
    public void NestedMacroShouldBeCalledInOrder()
    {
        var counter = 0;
        var parentId = 0;
        var childId = 0;

        Action parentAction = () => parentId = ++counter;
        Action childAction = () => childId = ++counter;;

        m_undoManager.BeginMacro();
        m_undoManager.AddUndo(new UndoStub(parentAction, parentAction), false);

        m_undoManager.BeginMacro();
        m_undoManager.AddUndo(new UndoStub(childAction, childAction), false);
        m_undoManager.EndMacro();

        m_undoManager.EndMacro();

        m_undoManager.Undo();
        Assert.IsTrue(0 < childId && childId < parentId);

        parentId = childId = 0;

        m_undoManager.Redo();
        Assert.IsTrue(0 < parentId && parentId < childId);
    }

    [Test]
    public void UndoRedoShouldFireStackSizeChanged_IfUndoEnabled()
    {
        m_undoManager.AddUndo(new UndoStub());

        TestStackSizeChanged(() => m_undoManager.Undo());
        TestStackSizeChanged(() => m_undoManager.Redo());
    }

    [Test]
    public void UndoRedoShouldNotFireStackSizeChanged_IfUndoDisabled()
    {
        m_undoManager.AddUndo(new UndoStub());
        m_undoManager.undoEnabled = false;

        TestStackSizeChanged(() => m_undoManager.Undo(), false);
        TestStackSizeChanged(() => m_undoManager.Redo(), false);
    }

    [Test]
    public void ResetStackShouldFireStackSizeChanged()
    {
        m_undoManager.AddUndo(new UndoStub());

        TestStackSizeChanged(() => m_undoManager.Reset());
    }

    [Test]
    public void AddUndoShouldFireStackSizeChanged()
    {
        TestStackSizeChanged(() => m_undoManager.AddUndo(new UndoStub()));
    }

    void TestStackSizeChanged(Action action, bool expected = true)
    {
        var changed = false;
        Action onChanged = () => changed = true;
        m_undoManager.onStackSizeChanged += onChanged;

        action();
        m_undoManager.onStackSizeChanged -= onChanged;

        Assert.AreEqual(expected, changed);
    }

    [Test]
    public void ChangeUndoEnabledStateShouldFireUndoEnableChanged()
    {
        TestUndoEnabledChanged(() => m_undoManager.undoEnabled = false);
        TestUndoEnabledChanged(() => m_undoManager.undoEnabled = true);
    }

    void TestUndoEnabledChanged(Action action)
    {
        var changed = false;
        Action onChanged = () => changed = true;
        m_undoManager.onUndoEnabledChanged += onChanged;

        action();
        m_undoManager.onUndoEnabledChanged -= onChanged;

        Assert.IsTrue(changed);
    }

    [Test]
    public void CleanShouldBeTrue_AfterAddingCommand_Without_SideEffect()
    {
        m_undoManager.AddUndo(new UndoStub(hasSideEffect: false));

        Assert.IsTrue(m_undoManager.IsClean());
    }

    [Test]
    public void CleanShouldChange_OnlyWhenUndoRedo_Until_SideEffectCommand()
    {
        var cmdWithSideEffect = new UndoStub();
        var cmdWithoutSideEffect = new UndoStub(hasSideEffect: false);

        m_undoManager.AddUndo(cmdWithSideEffect);
        m_undoManager.AddUndo(cmdWithoutSideEffect);
        m_undoManager.SetClean(true);
        m_undoManager.AddUndo(cmdWithoutSideEffect);
        m_undoManager.AddUndo(cmdWithSideEffect);

        // undo until clean
        m_undoManager.Undo();
        m_undoManager.Undo();
        Assert.IsTrue(m_undoManager.IsClean());

        //  undo a command w/o side effect 
        m_undoManager.Undo();
        Assert.IsTrue(m_undoManager.IsClean());
        // undo a command w/ side effect
        m_undoManager.Undo();
        Assert.IsFalse(m_undoManager.IsClean());

        // redo until clean
        m_undoManager.Redo();
        m_undoManager.Redo();
        Assert.IsTrue(m_undoManager.IsClean());

        // redo a command w/o side effect
        m_undoManager.Redo();
        Assert.IsTrue(m_undoManager.IsClean());
        // redo a command w/ side effect
        m_undoManager.Redo();
        Assert.IsFalse(m_undoManager.IsClean());
    }

    [Test]
    public void BeginEndEventShouldBeFiredOnUndo()
    {
        m_undoManager.AddUndo(new UndoStub());

        TestRunningEvent(() => {
            m_undoManager.Undo();
        });
    }

    [Test]
    public void BeginEndEventShouldBeFiredOnRedo()
    {
        m_undoManager.AddUndo(new UndoStub());
        m_undoManager.Undo();

        TestRunningEvent(() => {
            m_undoManager.Redo();
        });
    }

    void TestRunningEvent(Action action)
    {
        int changedCount = 0;

        m_undoManager.onRunningChanged += () => {
            if (changedCount == 0)
            {
                Assert.IsTrue(m_undoManager.isRunning);
            }
            else if (changedCount == 1)
            {
                Assert.IsFalse(m_undoManager.isRunning);
            }
            ++changedCount;
        };

        action();
    }

    [Test]
    public void CleanShouldRemainTrue_WhenTheStackIsEmpty_ThenDisabled()
    {
        m_undoManager.stackEnabled = false;
        Assert.IsTrue(m_undoManager.IsClean());
    }

    [Test]
    public void CleanShouldRemainTrue_WhenTheStackIsNotEmptyAndClean_ThenDisabled()
    {
        m_undoManager.AddUndo(new UndoStub());
        m_undoManager.SetClean(true);
        m_undoManager.stackEnabled = false;

        Assert.IsTrue(m_undoManager.IsClean());
    }

    [Test]
    public void CleanShouldRemainFalse_WhenNewUndoIsAdded_And_CleanUndoStackSize_IsLargerThan_UndoStackSize()
    {
        var cmd = new UndoStub();
        m_undoManager.AddUndo(cmd);
        m_undoManager.AddUndo(cmd);
        m_undoManager.SetClean(true);
        m_undoManager.Undo();
        m_undoManager.AddUndo(cmd);

        Assert.IsFalse(m_undoManager.IsClean());
        m_undoManager.Redo();
        Assert.IsFalse(m_undoManager.IsClean());
    }
}
