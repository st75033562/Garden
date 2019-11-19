using System;

namespace Gameboard
{
    /// <summary>
    /// helper class to set the active ui context for the current running undo command
    /// </summary>
    class UndoContext
    {
        public static readonly object Any = new object();
        public static readonly object Gameboard = new object();

        private static readonly string GBUndoNamespace = typeof(AddEntityCommand).Namespace;

        private readonly UIGameboard m_uiGameboard;

        public UndoContext(UIGameboard uiGameboard)
        {
            if (uiGameboard == null)
            {
                throw new ArgumentNullException("uiGameboard");
            }

            m_uiGameboard = uiGameboard;
            uiGameboard.undoManager.onRunCommand += OnRunCommand;
        }

        private void OnRunCommand(IUndoCommand cmd)
        {
            if (m_uiGameboard.isCodingSpaceVisible)
            {
                if (cmd.userData == Gameboard || cmd.GetType().Namespace == GBUndoNamespace)
                {
                    m_uiGameboard.CloseCodingSpace();
                }
            }
            else
            {
                if (cmd.userData == null && cmd.GetType().Namespace != GBUndoNamespace)
                {
                    m_uiGameboard.OpenCodingSpace();
                }
            }
        }
    }
}
