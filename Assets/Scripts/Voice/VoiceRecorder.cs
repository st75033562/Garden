using System;
using UnityEngine;
using POpusCodec;
using POpusCodec.Enums;
using PhotonVoiceUtil = ExitGames.Client.Photon.Voice.VoiceUtil;
using ExitGames.Client.Photon.Voice;

public class VoiceRecorder : MonoBehaviour, IVoiceLevelMeter
{
    private MicVoiceStream m_voiceStream;
    private LevelMeter m_levelMeter;

    private OpusEncoder m_opusEncoder;
    private float[] m_souceFrameBuffer;
    private float[] m_encoderFrameBuffer;
    private SegmentedBuffer m_voiceBuffer = new SegmentedBuffer(2048);

    private int m_souceSamplingRate;
    private int m_samplingRate;
    private int m_channels;

    // NOTE: we store the compressed clip in the memory, so don't record for too long
    public void Begin()
    {
        if (m_voiceStream != null)
        {
            throw new InvalidOperationException("already recording");
        }

        duration = 0.0f;
        clip = null;

        // TODO: separate encoding settings ?
        m_samplingRate = (int)PhotonVoiceSettings.Instance.SamplingRate;
        m_voiceStream = new MicVoiceStream(null, m_samplingRate);

        if (m_voiceStream.samplingRate != m_souceSamplingRate || m_voiceStream.channels != m_channels)
        {
            m_levelMeter = new LevelMeter(m_voiceStream.samplingRate, m_voiceStream.channels);
            m_souceSamplingRate = m_voiceStream.samplingRate;
            m_channels = m_voiceStream.channels;
        }
        else
        {
            m_levelMeter.ResetAccumAvgPeakAmp();
        }


        // write the clip header
        var header = new VoiceClipHeader {
            samplingRate = m_samplingRate,
            channels = m_channels,
            frameDurationMs = (int)PhotonVoiceSettings.Instance.FrameDuration / 1000
        };
        m_voiceBuffer.Clear();
        m_voiceBuffer.Add(header.Serialize());

        m_opusEncoder = new OpusEncoder(
            (SamplingRate)m_samplingRate,
            (Channels)m_voiceStream.channels,
            PhotonVoiceSettings.Instance.Bitrate,
            OpusApplicationType.Voip,
            (Delay)((int)PhotonVoiceSettings.Instance.FrameDuration * 2 / 1000));

        // FrameDuration is in microseconds
        int encoderFrameLength = m_voiceStream.channels *
            (int)PhotonVoiceSettings.Instance.FrameDuration * m_samplingRate / 1000000;
        int sourceFrameLength = m_voiceStream.samplingRate * encoderFrameLength / m_samplingRate;

        if (m_souceFrameBuffer == null || m_souceFrameBuffer.Length != sourceFrameLength)
        {
            m_souceFrameBuffer = new float[sourceFrameLength];
        }
        if (encoderFrameLength != sourceFrameLength)
        {
            // need resampling
            if (m_encoderFrameBuffer == null || encoderFrameLength != m_encoderFrameBuffer.Length)
            {
                m_encoderFrameBuffer = new float[encoderFrameLength];
            }
        }
        else
        {
            // same buffer, no resampling
            m_encoderFrameBuffer = m_souceFrameBuffer;
        }
    }

    public void End()
    {
        if (m_voiceStream != null)
        {
            m_voiceStream.Close();
            m_voiceStream = null;

            m_opusEncoder.Dispose();
            m_opusEncoder = null;

            clip = m_voiceBuffer.ToArray();
        }
    }

    // the sampling rate of the encoded voice
    public int samplingRate
    {
        get { return m_samplingRate; }
    }

    public int channels
    {
        get { return m_channels; }
    }

    // for debugging
    public int clipSize
    {
        get { return m_voiceBuffer.size; }
    }

    public float duration
    {
        get;
        private set;
    }

    public float currentAvgAmp
    {
        get { return m_levelMeter != null ? m_levelMeter.CurrentAvgAmp : 0; }
    }

    // return the last recorded clip, null if recording
    public byte[] clip
    {
        get;
        private set;
    }

    public bool isRecording
    {
        get { return m_voiceStream != null; }
    }

    void OnDestroy()
    {
        End();
    }

    void Update()
    {
        if (m_voiceStream != null)
        {
            duration += Time.deltaTime;

            int availableSamples = m_voiceStream.availableSamples;
            int sourceBufferSamples = m_souceFrameBuffer.Length / m_voiceStream.channels;
            //Debug.Log("sample: " + availableSamples);
            while (availableSamples >= sourceBufferSamples && m_voiceStream.Read(m_souceFrameBuffer))
            {
                availableSamples -= sourceBufferSamples;

                m_levelMeter.process(m_souceFrameBuffer);
                if (m_encoderFrameBuffer != m_souceFrameBuffer)
                {
                    PhotonVoiceUtil.Resample(m_souceFrameBuffer, m_encoderFrameBuffer, m_voiceStream.channels);
                }

                var encoded = m_opusEncoder.Encode(m_encoderFrameBuffer);
                // seems the encoded frame length won't exceed 255
                if (encoded.Count > 255)
                {
                    Debug.LogError("frame size too large");
                    continue;
                }
                // variable length encoding, save the frame size
                m_voiceBuffer.Add((byte)encoded.Count);
                m_voiceBuffer.Add(encoded);
            }
        }
    }
}
