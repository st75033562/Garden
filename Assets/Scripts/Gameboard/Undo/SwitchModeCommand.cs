using System;
using System.Collections;

namespace Gameboard
{
    public class SwitchModeCommand : BaseUndoCommand
    {
        private readonly UIGameboard m_uiGameboard;
        private readonly UIGameboard.Mode m_newMode;
        private readonly UIGameboard.Mode m_oldMode;

        public SwitchModeCommand(UIGameboard uiGameboard, UIGameboard.Mode newMode)
            : base(false)
        {
            if (uiGameboard == null)
            {
                throw new ArgumentNullException("uiGameboard");
            }

            m_uiGameboard = uiGameboard;
            m_newMode = newMode;
            m_oldMode = uiGameboard.mode;
        }

        protected override void UndoImpl()
        {
            m_uiGameboard.StartCoroutine(ChangeMode(m_oldMode));
        }

        protected override void RedoImpl()
        {
            m_uiGameboard.StartCoroutine(ChangeMode(m_newMode));
        }

        private IEnumerator ChangeMode(UIGameboard.Mode mode)
        {
            yield return m_uiGameboard.ChangeMode(mode);
            FireCompleted();
        }
    }
}
