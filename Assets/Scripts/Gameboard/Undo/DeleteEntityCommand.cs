using System;
using UnityEngine;

namespace Gameboard
{
    public class DeleteEntityCommand : BaseUndoCommand
    {
        private readonly GameboardSceneManager m_sceneManager;
        private readonly int m_entityId;
        private readonly ObjectInfo m_objInfo;
        private readonly int m_objIndex;
        
        public DeleteEntityCommand(GameboardSceneManager sceneManager, int entityId, ObjectInfo objInfo)
            : base(true)
        {
            if (sceneManager == null)
            {
                throw new ArgumentNullException("sceneManager");
            }
            if (objInfo == null)
            {
                throw new ArgumentNullException("objInfo");
            }

            m_sceneManager = sceneManager;
            m_entityId = entityId;
            m_objInfo = objInfo;
            m_objIndex = sceneManager.gameboard.objects.IndexOf(objInfo);

            if (m_objIndex == -1)
            {
                throw new ArgumentException("objInfo");
            }
        }

        protected override void UndoImpl()
        {
            var request = m_sceneManager.objectFactory.Create(
                new ObjectCreateInfo(m_objInfo) {
                    objectId = m_entityId,
                });

            request.onCompleted += sender => {
                if (sender.result != 0)
                {
                    m_sceneManager.gameboard.InsertObject(m_objIndex, m_objInfo);
                }
                else
                {
                    Debug.LogError("failed to create object");
                }
                FireCompleted();
            };
        }

        protected override void RedoImpl()
        {
            m_sceneManager.objectManager.Remove(m_entityId);
            m_sceneManager.gameboard.RemoveObject(m_objInfo);
        }
    }
}
