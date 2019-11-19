using System;
using System.Collections;
using UnityEngine;
#if UNITY_5_4_OR_NEWER
using UnityEngine.Networking;
#else
using UnityEngine.Experimental.Networking;
#endif
using POpusCodec;
using POpusCodec.Enums;
using ExitGames.Client.Photon.Voice;
using Channels = POpusCodec.Enums.Channels;


[RequireComponent(typeof(AudioSource))]
public class VoicePlayer : MonoBehaviour, IVoiceLevelMeter
{
    public event Action onStop;
    //public event Action onBeginLoading;
    //public event Action onLoaded;
    public event Action onBeginPlay;
    //public event Action onVoiceLoadError;

    private AudioStreamPlayer m_player;
    //private WebCacheLoadRequest m_voiceRequest;
    private string m_currentVoiceUrl;
    private VoiceClipHeader m_voiceHeader;
    private OpusDecoder m_decoder;

    private byte[] m_voiceBuffer;
    private int m_readPos;

    private byte[] m_compressedFrameBuffer = new byte[255];
    private float m_elpasedTimeInMs;
    private LevelMeter m_levelMeter;

    public enum State
    {
        Stopped,
        Loading,
        Loaded,
        Playing,
    }

    private State m_state = State.Stopped;

    private void Awake()
    {
        autoPlay = true;
    }

    // whether auto start playing when data is loaded. True by default
    public bool autoPlay { get; set; }

    /*
    public void Play(string url)
    {
        if (!Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute))
        {
            Debug.LogErrorFormat("Invalid absolute url: {0}", url);
            return;
        }

        Stop();
        StartCoroutine(LoadVoice(url));
    }*/

    // play the loaded data
    public void Play()
    {
        if (m_state == State.Loaded)
        {
            ResetDecoder(m_voiceHeader);
            PlayVoice();
        }
        else
        {
            Debug.LogError("data is not available");
        }
    }

    // NOTE: always auto start playing
    public bool Play(byte[] data)
    {
        Stop();
        if (Prepare(data))
        {
            PlayVoice();
        }
		else
		{
			return false;
		}

		return true;
    }
    
    private void PlayVoice()
    {
        m_state = State.Playing;
        m_player.Start(m_voiceHeader.samplingRate,
                       m_voiceHeader.channels, 
                       m_voiceHeader.frameDurationMs * m_voiceHeader.samplingRate / 1000, 
                       200);
        m_elpasedTimeInMs = 0;
        // ignore the header
        m_readPos = VoiceClipHeader.Size;

        if (onBeginPlay != null)
        {
            onBeginPlay();
        }
    }

    /*
    private IEnumerator LoadVoice(string url)
    {
        if (m_currentVoiceUrl == url && m_voiceBuffer != null)
        {
            m_state = State.Loaded;
            if (autoPlay)
            {
                Play();
            }
            else
            {
                if (onLoaded != null)
                {
                    onLoaded();
                }
            }
            yield break;
        }

        m_state = State.Loading;
        if (onBeginLoading != null)
        {
            onBeginLoading();
        }

        bool error = false;
        m_voiceRequest = WebCache.instance.loadDataAsync(url);
        try
        {
            yield return m_voiceRequest;
            if (!m_voiceRequest.isError)
            {
                var data = m_voiceRequest.bytes;
                Debug.Log("voice data size: " + data.Length);
                if (Prepare(data))
                {
                    m_currentVoiceUrl = url;
                    if (autoPlay)
                    {
                        PlayVoice();
                    }
                    else
                    {
                        m_state = State.Loaded;
                        if (onLoaded != null)
                        {
                            onLoaded();
                        }
                    }
                }
                else
                {
                    error = true;
                }
            }
            else
            {
                Debug.LogErrorFormat("failed to load voice {0}, {1}", url, m_voiceRequest.error);
                error = true;
            }
        }
        finally
        {
            m_voiceRequest.Dispose();
            m_voiceRequest = null;
        }
        if (error)
        {
            m_state = State.Stopped;
            if (onVoiceLoadError != null)
            {
                onVoiceLoadError();
            }
        }
    }
*/

    private bool Prepare(byte[] data)
    {
        var newHeader = new VoiceClipHeader();
        try
        {
            newHeader.Deserialize(data);
        }
        catch (Exception e)
        {
            Debug.LogErrorFormat("Failed to parse voice clip header, {0}", e);
            return false;
        }

        // check the validity of the buffer size
        if (data.Length <= VoiceClipHeader.Size)
        {
            return false;
        }

        // create the decoder if necessary
        if (!ResetDecoder(newHeader))
        {
            return false;
        }

        if (m_player == null)
        {
            m_player = new AudioStreamPlayer(GetComponent<AudioSource>(), "Voice", true);
        }

        m_voiceBuffer = data;
        m_voiceHeader = newHeader;
        m_levelMeter = new LevelMeter(m_voiceHeader.samplingRate, m_voiceHeader.channels);

        return true;
    }

    private bool ResetDecoder(VoiceClipHeader newHeader)
    {
        try
        {
            if (m_decoder != null)
            {
                m_decoder.Dispose();
                m_decoder = null;
            }
            m_decoder = new OpusDecoder((SamplingRate)newHeader.samplingRate, (POpusCodec.Enums.Channels)newHeader.channels);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogErrorFormat("Failed to create opus decoder, {0}", e);
            return false;
        }
    }

    public State state { get { return m_state; } }

    void Update()
    {
        if (m_player != null && m_player.IsStarted)
        {
            m_player.Update();
            m_elpasedTimeInMs += Time.deltaTime * 1000.0f;
            while (m_elpasedTimeInMs >= m_voiceHeader.frameDurationMs)
            {
                m_elpasedTimeInMs -= m_voiceHeader.frameDurationMs;

                int frameSize = m_voiceBuffer[m_readPos++];
                if (frameSize > m_compressedFrameBuffer.Length)
                {
                    Debug.LogError("Invalid compressed frame size");
                    Stop();
                    break;
                }

                if (m_readPos + frameSize > m_voiceBuffer.Length)
                {
                    Debug.LogError("not enough voice data, force stopping");
                    Stop();
                    break;
                }

                Array.Copy(m_voiceBuffer, m_readPos, m_compressedFrameBuffer, 0, frameSize);
                var frameBuffer = m_decoder.DecodePacketFloat(m_compressedFrameBuffer, frameSize);
                m_levelMeter.process(frameBuffer);
                m_player.OnAudioFrame(frameBuffer);

                m_readPos += frameSize;
                if (m_readPos >= m_voiceBuffer.Length)
                {
                    Stop();
                    break;
                }
            }
        }
    }

    void OnDestroy()
    {
        Stop();
        if (m_decoder != null)
        {
            m_decoder.Dispose();
        }
    }

    public void Stop()
    {
        if (m_state != State.Stopped)
        {
            m_state = State.Stopped;

            if (m_player != null)
            {
                m_player.Stop();
            }
            /*
            if (m_voiceRequest != null)
            {
                m_voiceRequest.Cancel(true);
                m_voiceRequest = null;
            }*/
            StopAllCoroutines();

            if (onStop != null)
            {
                onStop();
            }
        }
    }

    public float currentAvgAmp
    {
        get { return m_levelMeter != null ? m_levelMeter.CurrentAvgAmp : 0; }
    }
}
