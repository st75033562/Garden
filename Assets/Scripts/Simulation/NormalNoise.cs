using UnityEngine;

namespace RobotSimulation
{
    public class NormalNoise
    {
        private float m_nextNextGaussian;
        private bool m_haveNextNextGaussian;

        /// <summary>
        /// construct a standard normal distribution
        /// </summary>
        public NormalNoise()
            : this(0.0f, 1.0f)
        {
        }

        public NormalNoise(float mean, float stdDeviation)
        {
            this.mean = mean;
            this.stdDeviation = stdDeviation;
        }

        public float mean
        {
            get;
            set;
        }

        public float stdDeviation
        {
            get;
            set;
        }

        /// <summary>
        /// apply noise to the input
        /// </summary>
        public float Apply(float input)
        {
            float nextGaussian;
            // this is translated from the java source
            // see http://hg.openjdk.java.net/jdk8/jdk8/jdk/file/tip/src/share/classes/java/util/Random.java#l583
            if (m_haveNextNextGaussian)
            {
                nextGaussian = m_nextNextGaussian;
                m_haveNextNextGaussian = false;
            }
            else
            {
                float v1, v2, s;
                do
                {
                    v1 = 2 * Random.Range(0.0f, 1.0f) - 1; // between -1 and 1
                    v2 = 2 * Random.Range(0.0f, 1.0f) - 1; // between -1 and 1
                    s = v1 * v1 + v2 * v2;
                } while (s >= 1 || s == 0);

                float multiplier = Mathf.Sqrt(-2 * Mathf.Log(s)/s);
                m_nextNextGaussian = v2 * multiplier;
                m_haveNextNextGaussian = true;
                nextGaussian = v1 * multiplier;
            }

            // from standard normal to normal
            return stdDeviation * nextGaussian + mean + input;
        }
    }
}
