using System;
using UnityEngine;

namespace RobotSimulation
{
    [RequireComponent(typeof(AudioSource))]
    public class Buzzer : MonoBehaviour
    {
        private const int SampleRate = 44100;
        private const float MaxFrequency = 167772.15f;

        [SerializeField]
        private AudioSource m_source;

        private AudioClip m_clip;

        private float m_frequency;
        private int m_position;
        private int m_note;
        private float m_audioFrequency;

        void Start()
        {
            m_clip = AudioClip.Create("buzzer", SampleRate, 1, SampleRate, true, OnAudioRead, OnAudioPosition);
            m_source.clip = m_clip;
            m_source.loop = true;
        }

        void OnDestroy()
        {
            if (m_clip)
            {
                Destroy(m_clip);
            }
        }

        /// <summary>
        /// the frequency of the buzzer
        /// </summary>
        public float frequency
        {
            get { return m_frequency; }
            set
            {
                m_frequency = Mathf.Clamp(value, 0, MaxFrequency);
                TryPlay();
            }
        }

        /// <summary>
        /// musical note, only effective when frequency is 0
        /// </summary>
        public int note
        {
            get { return m_note; }
            set
            {
                m_note = Mathf.Max(0, value);
                TryPlay();
            }
        }

        void TryPlay()
        {
            if (!m_source)
            {
                return;
            }

            if (m_frequency == 0 && note == 0)
            {
                m_source.Stop();
            }
            else
            {
                if (m_frequency != 0)
                {
                    m_audioFrequency = m_frequency;
                }
                else
                {
                    m_audioFrequency = (int)(Mathf.Pow(2.0f, (note - 49) / 12.0f) * 440);
                }
                if (!m_source.isPlaying)
                {
                    m_source.Play();
                }
            }
        }

        void OnAudioRead(float[] data)
        {
            int position = m_position;
            float s = 2.0f * m_audioFrequency / SampleRate;
            for (int i = 0; i < data.Length; ++i)
            {
                // square wave
                // calculate the half period index
                int x = (int)(position * s) & 0x1;
                data[i] = x == 0 ? 1.0f : -1.0f;
                ++position;
            }
            OnAudioPosition(position);
        }

        void OnAudioPosition(int position)
        {
            if (position >= SampleRate)
            {
                position %= SampleRate;
            }
            m_position = position;
        }

        void Reset()
        {
            m_source = GetComponent<AudioSource>();
        }
    }
}
