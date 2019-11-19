using UnityEngine;

namespace AR
{
    class ExponentialVector2Filter : BaseExponentialFilter, IVector2Filter
    {
        public Vector2 Filter(Vector2 v, float dt)
        {
            value += (v - value) * smoothFactor;
            return value;
        }

        public Vector2 value
        {
            get;
            set;
        }

        public void Dispose() { }
    }

    class ExponentialVector3Filter : BaseExponentialFilter, IVector3Filter
    {
        public Vector3 Filter(Vector3 v, float dt)
        {
            value += (v - value) * smoothFactor;
            return value;
        }

        public Vector3 value
        {
            get;
            set;
        }

        public void Dispose() { }
    }
}
