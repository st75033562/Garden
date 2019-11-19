using System;
using UnityEngine;

namespace Gameboard
{
    public class Editor : MonoBehaviour
    {
        public event Action onBeforeChangingSelection;
        public event Action onSelectionChanged;

        public Material m_invalidPlacementMat;
        public GizmoManager m_gizmoManager;

        private Entity m_selectedEntity;

        public Entity lastSelectedEntity { get; set; }

        public Entity selectedEntity
        {
            get { return m_selectedEntity; }
            set
            {
                if (m_selectedEntity != value)
                {
                    if (onBeforeChangingSelection != null)
                    {
                        onBeforeChangingSelection();
                    }

                    lastSelectedEntity = m_selectedEntity;
                    m_selectedEntity = value;

                    if (onSelectionChanged != null)
                    {
                        onSelectionChanged();
                    }
                }
            }
        }

        public void SetupEntity(Entity entity, bool enablePlacementErrorDetection)
        {
            var rigidBody = entity.GetComponent<Rigidbody>();
            if (rigidBody)
            {
                rigidBody.isKinematic = true;
            }

            var errorDetector = entity.GetComponent<PlacementErrorDetector>();
            if (!errorDetector)
            {
                errorDetector = entity.gameObject.AddComponent<PlacementErrorDetector>();
                errorDetector.errorMaterial = m_invalidPlacementMat;
            }

            errorDetector.enabled = enablePlacementErrorDetection;

            if (entity.asset != null && entity.asset.gizmo != GizmoType.None && !m_gizmoManager.HasGizmo(entity.transform))
            {
                m_gizmoManager.AddGizmo(entity.transform, entity.asset.gizmo);
            }
        }

        public static void EnablePlacementErrorDetection(Entity entity, bool enabled)
        {
            var detector = entity.GetComponent<PlacementErrorDetector>();
            if (detector)
            {
                detector.enabled = enabled;
            }
        }
    }
}
