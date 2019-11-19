using System.Linq;
using UnityEditor;
using UnityEngine;

namespace RobotSimulation
{
    public class PathGridGizmo
    {
        [DrawGizmo(GizmoType.InSelectionHierarchy)]
        public static void Draw(FloorGrid grid, GizmoType type)
        {
            if (grid.paths == null || grid.paths.Length == 0 || !grid.partition)
            {
                return;
            }

            // draw the grid
            Gizmos.color = Color.red;

            var y = grid.meshObjects.Max(x => x ? x.transform.position.y : 0);

            // draw x lines
            var p = grid.partition.bound.min.xzAtY(y);
            var cellSize = grid.partition.cellSize;
            var xSize = grid.partition.bound.size.x * Vector3.right;

            for (int i = 0; i <= grid.partition.numCellY; ++i)
            {
                Gizmos.DrawLine(p, p + xSize);
                p += cellSize.y * Vector3.forward;
            }

            // draw z lines
            p = grid.partition.bound.min.xzAtY(y);
            var ySize = grid.partition.bound.size.y * Vector3.forward;

            for (int i = 0; i <= grid.partition.numCellX; ++i)
            {
                Gizmos.DrawLine(p, p + ySize);
                p += cellSize.x * Vector3.right;
            }
        }
    }
}
