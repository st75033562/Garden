using System;
using UnityEngine;

namespace Gameboard
{
    public class UpdateRobotSettingsCommand : BaseEntitySettingsCommand<RobotInfo>
    {
        private readonly Gameboard m_gameboard;

        public UpdateRobotSettingsCommand(
            ObjectManager objManager, 
            int entityId, 
            RobotInfo oldInfo, 
            RobotInfo newInfo, 
            Gameboard gameboard)
            : base(objManager, entityId, oldInfo, newInfo)
        {
            if (gameboard == null)
            {
                throw new ArgumentNullException("gameboard");
            }

            m_gameboard = gameboard;
        }

        protected override void UpdateProperties(Entity entity, RobotInfo info)
        {
            base.UpdateProperties(entity, info);

            entity.GetComponent<RobotColor>().colorId = info.colorId;

            var index = m_gameboard.robots.IndexOf(m_curInfo);
            if (index == -1)
            {
                Debug.LogError("robot info not found");
                return;
            }

            m_gameboard.NotifyRobotUpdated(index);
        }
    }
}
