using System;

namespace Gameboard
{
    public class UndoRobotSettings : IObjectSettingsUndo<RobotInfo>
    {
        private readonly UndoManager m_undoManager;
        private readonly ObjectManager m_objManager;
        private readonly Gameboard m_gameboard;

        public UndoRobotSettings(UndoManager undoManager, ObjectManager objManager, Gameboard gameboard)
        {
            if (undoManager == null)
            {
                throw new ArgumentNullException("undoManager");
            }

            if (objManager == null)
            {
                throw new ArgumentNullException("objManager");
            }

            if (gameboard == null)
            {
                throw new ArgumentNullException("gameboard");
            }

            m_undoManager = undoManager;
            m_objManager = objManager;
            m_gameboard = gameboard;
        }

        public void Record(RobotInfo oldInfo, RobotInfo newInfo, int entityId)
        {
            m_undoManager.AddUndo(
                new UpdateRobotSettingsCommand(m_objManager, entityId, oldInfo, newInfo, m_gameboard));
        }
    }
}
