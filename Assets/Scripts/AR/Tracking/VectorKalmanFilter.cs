using System;
using UnityEngine;

namespace AR
{
    class Vector2KalmanFilter : LinearKalmanFilter, IVector2Filter
    {
        public Vector2KalmanFilter()
            : base(2)
        {
        }

        public Vector2 Filter(Vector2 v, float dt)
        {
            SetDeltaTime(dt);
            Measurement.Set_21(v);
            Predict();
            return Correct().ToVector_21();
        }
    }

    class Vector3KalmanFilter : LinearKalmanFilter, IVector3Filter
    {
        public Vector3KalmanFilter()
            : base(3)
        {
        }

        public Vector3 Filter(Vector3 v, float dt)
        {
            SetDeltaTime(dt);
            Measurement.Set_31(v);
            Predict();
            return Correct().ToVector_31();
        }
    }
}