using System;

namespace Gameboard
{
    public class UpdateEntitySettingsCommand : BaseEntitySettingsCommand<ObjectInfo>
    {
        private readonly Gameboard m_gameboard;

        public UpdateEntitySettingsCommand(
            ObjectManager objManager, 
            int entityId, 
            ObjectInfo oldInfo, 
            ObjectInfo newInfo, 
            Gameboard gameboard)
            : base(objManager, entityId, oldInfo, newInfo)
        {
            if (gameboard == null)
            {
                throw new ArgumentNullException("gameboard");
            }

            m_gameboard = gameboard;
        }

        protected override void UpdateProperties(Entity entity, ObjectInfo info)
        {
            base.UpdateProperties(entity, info);

            entity.entityName = info.name;
            m_gameboard.NotifyObjectUpdated(m_curInfo);
        }
    }
}
