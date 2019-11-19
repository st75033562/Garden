using System;
using UnityEngine;

namespace Gameboard
{
    public class SelectEntityCommand : BaseEntityCommand
    {
        private readonly Editor m_editor;
        private readonly int m_oldEntityId;
        private readonly int m_newEntityId;

        public SelectEntityCommand(
            Editor editor, ObjectManager objectManager, int oldEntityId, int newEntityId)
            : base(objectManager, false)
        {
            if (editor == null)
            {
                throw new ArgumentNullException("editor");
            }

            m_editor = editor;
            m_oldEntityId = oldEntityId;
            m_newEntityId = newEntityId;
        }
            
        protected override void UndoImpl()
        {
            SelectEntity(m_oldEntityId);
        }

        protected override void RedoImpl()
        {
            SelectEntity(m_newEntityId);
        }

        private void SelectEntity(int entityId)
        {
            Entity entity = null;
            if (entityId != 0)
            {
                entity = m_objectManager.Get(entityId);
                if (!entity)
                {
                    Debug.LogErrorFormat("entity {0} not found", entityId);
                    return;
                }
            }

            m_editor.selectedEntity = entity;
        }
    }
}
