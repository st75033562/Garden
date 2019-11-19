using System;
using UnityEngine;

namespace Gameboard
{
    public class RotateEntityCommand : BaseTransformCommand
    {
        private readonly Vector3 m_oldRotation;
        private readonly Vector3 m_newRotation;

        public RotateEntityCommand(
            ObjectManager objectManager, int entityId, IObjectInfo objInfo, Vector3 oldRotation, Vector3 newRotation)
            : base(objectManager, entityId, objInfo)
        {
            m_oldRotation = oldRotation;
            m_newRotation = newRotation;
        }

        protected override void UndoImpl()
        {
            SetRotation(m_oldRotation);
        }

        protected override void RedoImpl()
        {
            SetRotation(m_newRotation);
        }

        private void SetRotation(Vector3 rotation)
        {
            var entity = m_objectManager.Get(m_entityId);
            if (!entity)
            {
                Debug.LogErrorFormat("entity {0} not found", m_entityId);
                return;
            }

            m_objectInfo.rotation = rotation;
            entity.positional.rotation = Coordinates.ConvertVector(rotation);
            entity.positional.Synchornize();
        }
    }
}
