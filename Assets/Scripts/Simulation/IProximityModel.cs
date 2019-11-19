using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RobotSimulation
{
    public interface IProximityModel
    {
        /// <summary>
        /// find the proximity value at the given distance in centimeter
        /// </summary>
        float Evaluate(float cm);

        float maxDistance { get; }
    }
}
