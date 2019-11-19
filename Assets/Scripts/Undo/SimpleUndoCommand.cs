using System;

public class SimpleUndoCommand : BaseUndoCommand
{
    private readonly Action m_undo;
    private readonly Action m_redo;

    public SimpleUndoCommand(Action undo, Action redo, bool hasSideEffect = true)
        : base(hasSideEffect)
    {
        if (undo == null)
        {
            throw new ArgumentNullException("undo");
        }

        if (redo == null)
        {
            throw new ArgumentNullException("redo");
        }

        m_undo = undo;
        m_redo = redo;
    }

    protected override void UndoImpl()
    {
        m_undo();
    }

    protected override void RedoImpl()
    {
        m_redo();
    }
}