using System;

namespace Gameboard
{
    public class ObjectNameValidator
    {
        private readonly string m_curName;
        private readonly Gameboard m_gameboard;
        private readonly VariableManager m_varManager;

        public ObjectNameValidator(string curName, Gameboard gameboard, VariableManager varManager)
        {
            if (gameboard == null)
            {
                throw new ArgumentNullException("gameboard");
            }
            if (varManager == null)
            {
                throw new ArgumentNullException("varManager");
            }

            m_curName = curName;
            m_gameboard = gameboard;
            m_varManager = varManager;
        }

        public bool IsDuplicate(string newName)
        {
            if (newName == m_curName)
            {
                return false;
            }

            if (m_gameboard.GetObject(newName) != null)
            {
                return true;
            }

            return m_varManager.has(newName);
        }
    }
}
