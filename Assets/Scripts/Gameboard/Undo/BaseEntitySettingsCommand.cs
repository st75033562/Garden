using System;
using UnityEngine;

namespace Gameboard
{
    public class BaseEntitySettingsCommand<T> : BaseEntityCommand where T : IObjectInfo<T>
    {
        private readonly int m_entityId;
        protected readonly T m_oldInfo;
        protected readonly T m_newInfo;
        protected readonly T m_curInfo;

        protected BaseEntitySettingsCommand(
            ObjectManager objManager, int entityId, T oldInfo, T newInfo)
            : base(objManager, true)
        {
            if (oldInfo == null)
            {
                throw new ArgumentNullException("oldInfo");
            }

            if (newInfo == null)
            {
                throw new ArgumentNullException("newInfo");
            }

            m_entityId = entityId;
            m_oldInfo = oldInfo;
            m_newInfo = newInfo.Clone();
            m_curInfo = newInfo;
        }

        protected override void UndoImpl()
        {
            UpdateProperties(m_oldInfo);
        }

        protected override void RedoImpl()
        {
            UpdateProperties(m_newInfo);
        }

        private void UpdateProperties(T info)
        {
            var entity = m_objectManager.Get(m_entityId);
            if (!entity)
            {
                Debug.LogErrorFormat("entity {0} not found", m_entityId);
                return;
            }

            UpdateProperties(entity, info);
        }

        protected virtual void UpdateProperties(Entity entity, T info)
        {
            entity.positional.position = Coordinates.ConvertVector(info.position);
            entity.positional.rotation = Coordinates.ConvertRotation(info.rotation);
            entity.positional.localScale = Coordinates.ConvertVector(info.scale);
            entity.positional.Synchornize();

            m_curInfo.CopyFrom(info);
        }
    }
}
