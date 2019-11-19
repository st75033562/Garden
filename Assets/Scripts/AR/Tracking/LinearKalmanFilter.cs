using OpenCVForUnity;
using System;
using UnityEngine;

namespace AR
{
    // just a simple utility class for wrapping opencv's kalman filter
    public class LinearKalmanFilter : IDisposable
    {
        private float m_processNoise;
        private float m_measurementNoise;
        private float m_errorCovPost;
        private bool m_initialized;
        private KalmanFilter m_filter;

        public LinearKalmanFilter(int MP)
        {
            if (MP <= 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            // measurement, velocity, acceleration
            m_filter = new KalmanFilter(MP * 3, MP, 0, CvType.CV_32F);

            var measurementMatrix = m_filter.get_measurementMatrix();
            for (int i = 0; i < MP; ++i)
            {
                measurementMatrix.put(i, i, 1);
            }

            Measurement = new Mat(MP, 1, CvType.CV_32F);
        }

        public float ProcessNoise
        {
            get { return m_processNoise; }
            set
            {
                m_processNoise = value;
                Core.setIdentity(m_filter.get_processNoiseCov(), Scalar.all(value));
            }
        }

        public float MeasurementNoise
        {
            get { return m_measurementNoise; }
            set
            {
                m_measurementNoise = value;
                Core.setIdentity(m_filter.get_measurementNoiseCov(), Scalar.all(value));
            }
        }

        public float ErrorCovPost
        {
            get { return m_errorCovPost; }
            set
            {
                m_errorCovPost = value;
                Core.setIdentity(m_filter.get_errorCovPost(), Scalar.all(value));
            }
        }

        // update the delta time used in transition matrix
        public void SetDeltaTime(float dt)
        {
            UpdateTransitionMatrix(dt);
        }

        private void UpdateTransitionMatrix(float dt)
        {
            var transitionMatrix = m_filter.get_transitionMatrix();
            int measurementCount = transitionMatrix.cols() / 3;
            // setup transitions for 0 and 1st order states
            for (int i = 0, j = measurementCount; j < transitionMatrix.cols(); ++i, ++j)
            {
                transitionMatrix.put(i, j, dt);
            }

            float dtSqrOver2 = 0.5f * dt * dt;
            // setup transitions for 2nd order states
            for (int i = 0, j = measurementCount * 2; j < transitionMatrix.cols(); ++i, ++j)
            {
                transitionMatrix.put(i, j, dtSqrOver2);
            }
        }

        public Mat Predict()
        {
            if (!m_initialized)
            {
                m_initialized = true;
                Init();
            }
            return m_filter.predict();
        }

        // make sure Measurement is filled with updates before calling
        public Mat Correct()
        {
            return m_filter.correct(Measurement);
        }

        public Mat Measurement { get; private set; }

        // Initialize the filter with the given measurement
        // if null, Measurement member is used
        void Init()
        {
            var statePost = m_filter.get_statePost();
            for (int i = 0; i < statePost.rows(); ++i)
            {
                if (i < Measurement.rows())
                {
                    statePost.put(i, 0, Measurement.get(i, 0));
                }
                else
                {
                    statePost.put(i, 0, 0);
                }
            }
        }

        public void Dispose()
        {
            m_filter.Dispose();
            Measurement.Dispose();
        }

    }
}