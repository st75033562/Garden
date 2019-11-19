using LitJson;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RobotSimulation
{
    [Serializable]
    public struct ProximityDataPoint : IComparable<ProximityDataPoint>
    {
        public float distanceCm;
        public int proximity;

        public int CompareTo(ProximityDataPoint other)
        {
            return distanceCm.CompareTo(other.distanceCm);
        }
    }

    public class ProximityLookupTable : IProximityModel
    {
        private static ProximityLookupTable s_default;

        private readonly List<ProximityDataPoint> m_data = new List<ProximityDataPoint>();

        public float Evaluate(float cm)
        {
            var point = new ProximityDataPoint { distanceCm = cm };
            var hiIndex = m_data.BinarySearch(point);
            if (hiIndex < 0)
            {
                hiIndex = ~hiIndex;
            }
            if (hiIndex == 0 || hiIndex >= m_data.Count)
            {
                return 0.0f;
            }

            // linear interpolate
            var hi = m_data[hiIndex];
            var low = m_data[hiIndex - 1];
            return Mathf.Lerp(low.proximity, hi.proximity, Mathf.InverseLerp(low.distanceCm, hi.distanceCm, cm));
        }

        public float maxDistance
        {
            get;
            private set;
        }

        /// <summary>
        /// set the data points for the lookup table
        /// <para>points must be ordered by distanceCm in ascending order</para>
        /// </summary>
        public void SetDataPoints(IEnumerable<ProximityDataPoint> data)
        {
            m_data.Clear();
            m_data.AddRange(data);
            maxDistance = m_data.LastOrDefault().distanceCm;
        }

        /// <summary>
        /// return the default lookup table
        /// </summary>
        public static ProximityLookupTable defaultInstance
        {
            get
            {
                if (s_default == null)
                {
                    var proximityData = Resources.Load<TextAsset>("Data/sim_proximity");
                    var dataPoints = JsonMapper.ToObject<List<ProximityDataPoint>>(proximityData.text);
                    s_default = new ProximityLookupTable();
                    s_default.SetDataPoints(dataPoints);
                }
                return s_default;
            }
        }
    }
}
