using OpenCVForUnity;
using System;

namespace AR
{
    // used to filter corners of marker
    class CornerFilter : IDisposable
    {
        private readonly IVector2Filter[] m_filters = new IVector2Filter[4];

        public CornerFilter(Func<IVector2Filter> factory)
        {
            for (int i = 0; i < m_filters.Length; ++i)
            {
                m_filters[i] = factory();
            }
        }

        public void Filter(Mat corners, float dt)
        {
            for (int i = 0; i < 4; ++i)
            {
                var res = m_filters[i].Filter(corners.ToVector2_11(0, i), dt);
                corners.Set_11(res, 0, i);
            }
        }

        public void Dispose()
        {
            for (int i = 0; i < m_filters.Length; ++i)
            {
                m_filters[i].Dispose();
            }
        }
    }
}
