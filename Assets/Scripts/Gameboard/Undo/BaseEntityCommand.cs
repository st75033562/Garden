using System;

namespace Gameboard
{
    public abstract class BaseEntityCommand : BaseUndoCommand
    {
        protected readonly ObjectManager m_objectManager;

        protected BaseEntityCommand(ObjectManager objectManager, bool hasSideEffect)
            : base(hasSideEffect)
        {
            if (objectManager == null)
            {
                throw new ArgumentNullException("objectManager");
            }

            m_objectManager = objectManager;
        }
    }
}
