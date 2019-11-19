using System;

public interface IUndoCommand
{
    /// <summary>
    /// triggered when Undo or Redo has completed running
    /// </summary>
    event Action<IUndoCommand> onCompleted;

    void Undo();

    void Redo();

    /// <summary>
    /// true if the undo command affect the clean state of the system
    /// </summary>
    /// <remarks>
    /// The return value should be constant across calls.
    /// </remarks>
    bool hasSideEffect { get; }

    object userData { get; set; }
}
