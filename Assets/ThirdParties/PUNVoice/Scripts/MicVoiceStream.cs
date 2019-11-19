using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class MicVoiceStream
{
    private class SharedMicrophone
    {
        public int useCount;
        public string deviceName;
        public AudioClip clip;
    }

    private static Dictionary<string, SharedMicrophone> s_openedMic = new Dictionary<string, SharedMicrophone>();

    private SharedMicrophone m_sharedMic;
    private int m_lastReadPos;

    // frequency will be clamped to the valid range
    public MicVoiceStream(string device, int frequency)
    {
        if (device == null)
        {
            // null key is not allowed
            device = string.Empty;
        }

        // TODO: sampling at highest frequency to support different frequencies for same device ?
        if (!s_openedMic.TryGetValue(device, out m_sharedMic))
        {
            m_sharedMic = new SharedMicrophone();
            m_sharedMic.deviceName = device;

            int minFreq, maxFreq;
            Microphone.GetDeviceCaps(device, out minFreq, out maxFreq);
            if (minFreq != 0 || maxFreq != 0)
            {
                frequency = Mathf.Clamp(frequency, minFreq, maxFreq);
            }
            // 1 sec circular buffer
            m_sharedMic.clip = Microphone.Start(device, true, 1, frequency);
            if (!m_sharedMic.clip)
            {
                //throw new ApplicationException("Failed to open microphone, frequency " + frequency);
                Debug.LogError("Failed to open microphone, frequency " + frequency);
                // create a dummy clip
                const int DummyFrequencey = 44000;
                m_sharedMic.clip = AudioClip.Create("Dummy", DummyFrequencey, 1, DummyFrequencey, false);
            }

            s_openedMic.Add(device, m_sharedMic);
        }
        ++m_sharedMic.useCount;
    }

    private void EnsureValid()
    {
        if (m_sharedMic == null)
        {
            throw new ObjectDisposedException("MicVoiceStream");
        }
    }

    public int samplingRate
    {
        get
        {
            EnsureValid();
            return m_sharedMic.clip.frequency;
        }
    }

    public int channels
    {
        get
        {
            EnsureValid();
            return m_sharedMic.clip.channels;
        }
    }

    public int position
    {
        get
        {
            EnsureValid();
            return Microphone.GetPosition(m_sharedMic.deviceName);
        }
    }

    // how many samples available to read
    // NOTE: different calls will return different values even in same frame
    public int availableSamples
    {
        get
        {
            int curPosition = position;
            // assume not wrapped
            if (curPosition >= m_lastReadPos)
            {
                return curPosition - m_lastReadPos;
            }
            else
            {
                return m_sharedMic.clip.samples + (curPosition - m_lastReadPos);
            }
        }
    }

    // read number of samples determined by the buffer length
    // if not enough samples, return false, and the buffer is modified
    public bool Read(float[] buffer)
    {
        EnsureValid();

        int samples = availableSamples;
        int samplesToRead = buffer.Length / m_sharedMic.clip.channels;
        if (samples >= samplesToRead)
        {
            m_sharedMic.clip.GetData(buffer, m_lastReadPos);
            m_lastReadPos = (m_lastReadPos + samplesToRead) % m_sharedMic.clip.samples;
            return true;
        }
        return false;
    }

    // read some samples from the stream
    // return the number of read samples
    public int ReadSome(float[] buffer)
    {
        EnsureValid();

        int samples = availableSamples;
        int maxSamplesToRead = buffer.Length / m_sharedMic.clip.channels;
        m_sharedMic.clip.GetData(buffer, m_lastReadPos);
        int samplesRead = Mathf.Min(samples, maxSamplesToRead);
        m_lastReadPos = (m_lastReadPos + samplesRead) % m_sharedMic.clip.samples;
        return samplesRead;
    }

    public void Close()
    {
        if (m_sharedMic == null)
        {
            return;
        }

        if (--m_sharedMic.useCount == 0)
        {
            Microphone.End(m_sharedMic.deviceName);
#if UNITY_EDITOR
            GameObject.DestroyImmediate(m_sharedMic.clip);
#else
            GameObject.Destroy(m_sharedMic.clip);
#endif
            s_openedMic.Remove(m_sharedMic.deviceName);
            m_sharedMic = null;
        }
    }
}
