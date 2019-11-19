using UnityEngine;

namespace RobotSimulation
{
    public interface ILighting
    {
        /// <summary>
        /// calculate the illuminance at the given point
        /// </summary>
        float ComputeIlluminance(Vector3 position, Vector3 normal);
    }
}
