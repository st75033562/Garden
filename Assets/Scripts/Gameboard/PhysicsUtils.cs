using UnityEngine;

namespace Gameboard
{
    public static class PhysicsUtils
    {
        public static readonly int PlacementLayer = LayerMask.NameToLayer("Placement");
        public static readonly int IgnoreEditorPicking = LayerMask.NameToLayer("IgnoreEditorPicking");

        public static bool GetPlacementPosition(Vector2 pos, out Vector3 hitPoint)
        {
            RaycastHit hitInfo;
            if (Physics.Raycast(pos.xzAtY(10), Vector3.down, out hitInfo, 20, 1 << PlacementLayer))
            {
                hitPoint = hitInfo.point;
                hitPoint.y += 0.01f;
                return true;
            }
            else
            {
                hitPoint.x = pos.x;
                hitPoint.y = 0;
                hitPoint.z = pos.y;
            }
            return false;
        }

        public static bool IsFloor(GameObject go)
        {
            return go.layer == PlacementLayer && go.CompareTag("Floor");
        }
    }
}
