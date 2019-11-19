using System;
using UnityEngine;
using OpenCVForUnitySample;

namespace AR
{
    class PoseFilter : IDisposable
    {
        private readonly IQuaternionFilter m_rotationFilter;
        private readonly IVector3Filter m_posFilter;

        public PoseFilter(IVector3Filter posFilter, IQuaternionFilter rotationFilter)
        {
            if (posFilter == null)
            {
                throw new ArgumentNullException("posFilter");
            }
            if (rotationFilter == null)
            {
                throw new ArgumentNullException("rotationFilter");
            }
            m_posFilter = posFilter;
            m_rotationFilter = rotationFilter;
        }

        public virtual Matrix4x4 Filter(ref Matrix4x4 m, float dt)
        {
            Vector3 position = ARUtils.ExtractTranslationFromMatrix(ref m);
            Quaternion rotation = ARUtils.ExtractRotationFromMatrix(ref m);

            position = m_posFilter.Filter(position, dt);
            rotation = m_rotationFilter.Filter(rotation);

            return Matrix4x4.TRS(position, rotation, Vector3.one);
        }

        public void Dispose()
        {
            m_posFilter.Dispose();
        }
    }

}