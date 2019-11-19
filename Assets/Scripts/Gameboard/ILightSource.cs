using RobotSimulation;
using UnityEngine;

namespace Gameboard
{
    public interface ILightSource
    {
        float flux { get; set; }

        float ComputeIlluminance(Vector3 position, Vector3 normal);
    }
}
