using System;
using UnityEngine;

namespace AR
{
    // a simple weighted moving average filter for quaternion
    // this is not correct for quaternion which is not linear but works
    class SimpleQuaternionFilter : IQuaternionFilter
    {
        private float[] m_weights = new float[0];
        private float m_invWeights;
        private CircularBuffer<Quaternion> m_samples;
        
        public SimpleQuaternionFilter()
        {
        }

        public SimpleQuaternionFilter(float[] weights)
        {
            SetWeights(weights);
        }

        public void SetWeights(float[] weights)
        {
            if (weights == null)
            {
                throw new ArgumentNullException();
            }

            // cache the total weights
            float totalWeights = 0.0f;
            for (int i = 0; i < weights.Length; ++i)
            {
                if (weights[i] <= 0)
                {
                    throw new ArgumentException("weight must be positive");
                }
                totalWeights += weights[i];
            }
            m_invWeights = 1.0f / totalWeights;

            if (m_samples == null)
            {
                m_samples = new CircularBuffer<Quaternion>(weights.Length);
            }
            while (weights.Length < m_samples.Count)
            {
                m_samples.Pop();
            }
            m_weights = weights;
        }

        public Quaternion Filter(Quaternion q)
        {
            if (m_weights.Length <= 1)
            {
                return q;
            }

            m_samples.Add(q);
            int count = m_samples.Count;
            if (count == 1)
            {
                return q;
            }

            float invWeights = m_invWeights;
            // most of the time we don't need to recalculate the inverse of weights
            if (count < m_weights.Length)
            {
                float totalWeights = 0;
                for (int i = 0; i < count; ++i)
                {
                    totalWeights += m_weights[i];
                }
                invWeights = 1.0f / totalWeights;
            }

            Quaternion res = Scale(m_samples[0], m_weights[0]);
            for (int i = 1; i < count; ++i)
            {
                res.x += m_weights[i] * m_samples[i].x;
                res.y += m_weights[i] * m_samples[i].y;
                res.z += m_weights[i] * m_samples[i].z;
                res.w += m_weights[i] * m_samples[i].w;
                res = Utils.Normalize(Scale(res, invWeights));
            }
            return res;
        }

        private static Quaternion Scale(Quaternion q, float s)
        {
            q.x *= s;
            q.y *= s;
            q.z *= s;
            q.w *= s;
            return q;
        }
    }
}
