using System;

namespace Gameboard
{
    public abstract class BaseTransformCommand : BaseEntityCommand
    {
        protected readonly int m_entityId;
        protected readonly IObjectInfo m_objectInfo;

        protected BaseTransformCommand(ObjectManager objectManager, int entityId, IObjectInfo objInfo)
            : base(objectManager, true)
        {
            if (objInfo == null)
            {
                throw new ArgumentNullException("objInfo");
            }

            m_entityId = entityId;
            m_objectInfo = objInfo;
        }
    }
}
