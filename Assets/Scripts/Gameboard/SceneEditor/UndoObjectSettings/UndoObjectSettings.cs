using System;

namespace Gameboard
{
    public class UndoObjectSettings : IObjectSettingsUndo<ObjectInfo>
    {
        private readonly UndoManager m_undoManager;
        private readonly ObjectManager m_objManager;
        private readonly Gameboard m_gameboard;
        private readonly VariableManager m_varManager;

        public UndoObjectSettings(
            UndoManager undoManager, ObjectManager objManager, Gameboard gameboard, VariableManager varManager)
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

            if (varManager == null)
            {
                throw new ArgumentNullException("varManager");
            }

            m_undoManager = undoManager;
            m_objManager = objManager;
            m_gameboard = gameboard;
            m_varManager = varManager;
        }

        public void Record(ObjectInfo oldInfo, ObjectInfo newInfo, int entityId)
        {
            m_undoManager.BeginMacro(UndoContext.Gameboard);

            m_undoManager.AddUndo(
                new UpdateEntitySettingsCommand(m_objManager, entityId, oldInfo, newInfo, m_gameboard));

            if (oldInfo.name != newInfo.name)
            {
                var oldVar = m_varManager.get(oldInfo.name);
                if (oldVar != null)
                {
                    var cmd = new RenameVariableCommand(m_varManager, oldInfo.name, newInfo.name);
                    m_undoManager.AddUndo(cmd);
                }
            }

            m_undoManager.EndMacro();
        }
    }
}
