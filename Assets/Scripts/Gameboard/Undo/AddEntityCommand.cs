using System;
using UnityEngine;

namespace Gameboard
{
    public class AddEntityCommand : BaseUndoCommand
    {
        private readonly GameboardSceneManager m_sceneManager;
        private readonly ObjectInfo m_objInfo;
        private readonly int m_objIndex;
        private readonly int m_prevEntityId;
        private readonly int m_oldNextObjNum;
        private readonly int m_curNextObjNum;

        public AddEntityCommand(
            GameboardSceneManager sceneManager,
            int oldNextObjNum,
            int curNextObjNum,
            ObjectInfo objInfo,
            int index = -1)
            : base(true)
        {
            if (sceneManager == null)
            {
                throw new ArgumentNullException("sceneManager");
            }

            var objects = sceneManager.gameboard.objects;
            if (index > objects.Count)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            m_sceneManager = sceneManager;
            m_oldNextObjNum = oldNextObjNum;
            m_curNextObjNum = curNextObjNum;
            m_objInfo = objInfo;
            m_objIndex = index < 0 ? objects.Count : index;

            prevEntityId = sceneManager.objectManager.prevEntityId;
        }

        public int prevEntityId { get; set; }

        protected override void UndoImpl()
        {
            SetNextObjectNum(m_oldNextObjNum);

            var entity = m_sceneManager.objectManager.Get(m_objInfo.name);
            if (entity)
            {
                entity.Destroy();
                m_sceneManager.gameboard.RemoveObject(m_objInfo);
                m_sceneManager.objectManager.prevEntityId = prevEntityId;
            }
            else
            {
                Debug.LogErrorFormat("entity {0} not found", m_objInfo.name);
            }
        }

        public override void Redo()
        {
            var request = m_sceneManager.objectFactory.Create(new ObjectCreateInfo(m_objInfo));
            request.onCompleted += sender => {
                if (sender.result != 0)
                {
                    SetNextObjectNum(m_curNextObjNum);
                    m_sceneManager.gameboard.InsertObject(m_objIndex, m_objInfo);
                }
                else
                {
                    Debug.LogFormat("failed to create object");
                }
                FireCompleted();
            };
        }

        void SetNextObjectNum(int num)
        {
            var assetInfo = m_sceneManager.gameboard.GetAssetInfo(m_objInfo.assetId);
            if (assetInfo != null)
            {
                assetInfo.nextObjectNum = num;
            }
            else
            {
                Debug.LogError("invalid asset id: " + m_objInfo.assetId);
            }
        }
    }
}
