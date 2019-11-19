using System;

public abstract class BaseUndoCommand : IUndoCommand
{
    public event Action<IUndoCommand> onCompleted;

    protected bool m_hasSideEffect;

    protected BaseUndoCommand()
    {
    }

    protected BaseUndoCommand(bool hasSideEffect)
    {
        m_hasSideEffect = hasSideEffect;
    }

    public virtual void Undo()
    {
        UndoImpl();
        if (!isUndoAsync)
        {
            FireCompleted();
        }
    }

    protected virtual void UndoImpl() { }

    public virtual void Redo()
    {
        RedoImpl();
        if (!isRedoAsync)
        {
            FireCompleted();
        }
    }

    protected virtual void RedoImpl() { }

    protected void FireCompleted()
    {
        if (onCompleted != null)
        {
            onCompleted(this);
        }
    }

    // TODO: make this virtual
    public bool hasSideEffect
    {
        get { return m_hasSideEffect; }
    }

    protected virtual bool isUndoAsync { get { return false; } }

    protected virtual bool isRedoAsync { get { return false; } }

    public object userData
    {
        get;
        set;
    }
}
