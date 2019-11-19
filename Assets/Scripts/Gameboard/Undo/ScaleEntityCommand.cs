using UnityEngine;

namespace Gameboard
{
    public class ScaleEntityCommand : BaseTransformCommand
    {
        private readonly Vector3 m_oldScale;
        private readonly Vector3 m_newScale;

        public ScaleEntityCommand(
            ObjectManager objectManager, int entityId, IObjectInfo objInfo, Vector3 oldScale, Vector3 newScale)
            : base(objectManager, entityId, objInfo)
        {
            m_oldScale = oldScale;
            m_newScale = newScale;
        }

        protected override void UndoImpl()
        {
            SetScale(m_oldScale);
        }

        protected override void RedoImpl()
        {
            SetScale(m_newScale);
        }

        private void SetScale(Vector3 scale)
        {
            var entity = m_objectManager.Get(m_entityId);
            if (!entity)
            {
                Debug.LogErrorFormat("entity {0} not found", m_entityId);
                return;
            }

            m_objectInfo.scale = scale;
            entity.positional.localScale = Coordinates.ConvertVector(scale);
            entity.positional.Synchornize();
        }
    }
}
