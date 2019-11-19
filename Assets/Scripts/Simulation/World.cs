using UnityEngine;

namespace RobotSimulation
{
    public class World : MonoBehaviour
    {
        public RobotManager robotManager;
        public CameraManager cameraManager;
        public FloorGrid floor;

        public IProximityModel proximityModel { get; set; }

        void Start()
        {
            if (proximityModel == null)
            {
                proximityModel = ProximityLookupTable.defaultInstance;
                Debug.Log("using the default proximity table");
            }

            foreach (var robot in robotManager)
            {
                robot.floor = floor;
                robot.proximityModel = proximityModel;
            }
        }
    }
}
