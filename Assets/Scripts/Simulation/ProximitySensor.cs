//#define RADIUS_WEIGHT
//#define ANGLE_WEIGHT

using UnityEngine;

namespace RobotSimulation
{
    public class ProximitySensor : MonoBehaviour
    {
        private float m_worldToPhysicalCmRatio = 1.0f;

        void Awake()
        {
            worldToPhysicalRatio = 1.0f;
        }

        public IProximityModel proximityCurve
        {
            get;
            set;
        }

        public float worldToPhysicalRatio
        {
            get { return m_worldToPhysicalCmRatio * 100; }
            set { m_worldToPhysicalCmRatio = value / 100; }
        }

        public int value
        {
            get;
            private set;
        }

        public NormalNoise noise
        {
            get;
            set;
        }

        void FixedUpdate()
        {
            if (proximityCurve != null)
            {
                //float totalWeights = 0.0f;
                //float totalValue = 0.0f;
                float maxDistance = proximityCurve.maxDistance * m_worldToPhysicalCmRatio;
                float maxRadius = Mathf.Tan(Constants.ProximitySensorHalfConeRadians) * maxDistance;
                float newValue;
                Vector3 center = transform.position + transform.forward * maxDistance;
                float maxValue = 0;

                for (int i = 0; i < Constants.ProximitySensorNumConcentricSlices; ++i)
                {
                    float radius = (float)i / (Constants.ProximitySensorNumConcentricSlices - 1) * maxRadius;
#if RADIUS_WEIGHT
                    float radiusWeight = Mathf.Pow(Mathf.Lerp(1.0f, Constants.ProximitySensorMinRadiusWeight, radius / maxRadius), 
                                                   Constants.ProximitySensorRadiusWeightPower);
#else
                    //float radiusWeight = 1.0f;
#endif

                    for (int j = 0; j < Constants.ProximitySensorConeSegments[i]; ++j)
                    {
                        float angle = (float)j / Constants.ProximitySensorConeSegments[i] * Mathf.PI * 2;

                        float yScale = j <= Constants.ProximitySensorConeSegments[i] / 2 ? Constants.ProximitySensorUpMinorAxisScale : 1.0f;
                        // seems the cone is elliptic
                        Vector3 dir = Mathf.Cos(angle) * radius * transform.right + Mathf.Sin(angle) * radius * yScale * transform.up;
                        Vector3 end = center + dir;

                        RaycastHit hitInfo;
                        if (Physics.Linecast(transform.position, end, out hitInfo, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore) 
                            && !hitInfo.collider.CompareTag("Floor"))
                        {
                            float f = Mathf.Max(0, Vector3.Dot((transform.position - end).normalized, hitInfo.normal));
                            newValue = proximityCurve.Evaluate(hitInfo.distance / m_worldToPhysicalCmRatio) * f;
                            maxValue = Mathf.Max(maxValue, newValue);
#if ANGLE_WEIGHT
                            float angleWeight = Mathf.Max(0, Vector3.Dot((transform.position - end).normalized, hitInfo.normal));
                            angleWeight = Mathf.Pow(angleWeight, Constants.ProximitySensorAnglePower);
#else
                            //float angleWeight = 1.0f;
#endif
                            //totalValue += newValue * angleWeight * radiusWeight;

                            Debug.DrawLine(transform.position, hitInfo.point, Color.green);
                        }
                        else
                        {
                            Debug.DrawLine(transform.position, end, Color.red);
                        }
                    }
                    //totalWeights += (Constants.ProximitySensorConeSegments[i]) * radiusWeight;
                }

                value = Mathf.Max(0, (int)(noise != null ? noise.Apply(maxValue) : maxValue));
            }
        }

        void OnDrawGizmos()
        {
            DebugUtils.DrawText(transform.position, value.ToString(), Color.magenta);
        }
    }
}
