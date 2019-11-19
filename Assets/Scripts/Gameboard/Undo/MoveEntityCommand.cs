using System;
using UnityEngine;

namespace Gameboard
{
    public class MoveEntityCommand : BaseTransformCommand
    {
        private readonly Vector3 m_oldPos;
        private readonly Vector3 m_newPos;

        public MoveEntityCommand(
            ObjectManager objectManager, int entityId, IObjectInfo objInfo, Vector3 oldPos, Vector3 newPos)
            : base(objectManager, entityId, objInfo)
        {
            m_oldPos = oldPos;
            m_newPos = newPos;
        }

        protected override void UndoImpl()
        {
            SetPosition(m_oldPos);
        }

        protected override void RedoImpl()
        {
            SetPosition(m_newPos);
        }

        void SetPosition(Vector3 pos)
        {
            var entity = m_objectManager.Get(m_entityId);
            if (!entity)
            {
                Debug.LogErrorFormat("entity {0} not found", m_entityId);
                return;
            }

            m_objectInfo.position = pos;
            entity.positional.position = Coordinates.ConvertVector(pos);
            entity.positional.Synchornize();
        }
    }
}
