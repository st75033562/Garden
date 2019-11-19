using LitJson;
using UnityEngine;

namespace RobotSimulation
{
    public static class Constants
    {
        static Constants()
        {
            var data = Resources.Load<TextAsset>("Data/sim_constants");
            var jsonObj = JsonMapper.ToObject(data.text);
            ProximitySensorHalfConeRadians = jsonObj["proximity_sensor_half_cone_radians"].GetFloat();
            ProximitySensorNumConcentricSlices = jsonObj["proximity_sensor_num_concentric_slices"].GetInt();
            ProximitySensorConeSegments = jsonObj["proximity_sensor_cone_segments"].GetIntArray();
            ProximitySensorUpMinorAxisScale = jsonObj["proximity_sensor_up_minor_axis_scale"].GetFloat();
            ProximitySensorAnglePower = jsonObj["proximity_sensor_angle_power"].GetFloat();
            ProximitySensorMinRadiusWeight = jsonObj["proximity_sensor_min_radius_weight"].GetFloat();
            ProximitySensorRadiusWeightPower = jsonObj["proximity_sensor_radius_weight_power"].GetFloat();
        }

        public static float ProximitySensorHalfConeRadians
        {
            get;
            private set;
        }

        public static float ProximitySensorNumConcentricSlices
        {
            get;
            private set;
        }

        public static int[] ProximitySensorConeSegments
        {
            get;
            private set;
        }

        public static float ProximitySensorUpMinorAxisScale
        {
            get;
            private set;
        }

        public static float ProximitySensorAnglePower
        {
            get;
            private set;
        }

        public static float ProximitySensorMinRadiusWeight
        {
            get;
            private set;
        }

        public static float ProximitySensorRadiusWeightPower
        {
            get;
            private set;
        }
    }
}
