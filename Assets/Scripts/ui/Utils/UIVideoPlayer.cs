using UnityEngine;
using UnityEngine.UI;

public class UIVideoPlayer : MonoBehaviour
{
    public MediaPlayerCtrl m_mediaPlayer;
    public RawImage m_playerSurface;
    public Button m_playButton;
    public Button m_stopButton;
    public GameObject m_pauseButton;
    public Slider m_progressBar;
    public Text m_playbackTimeText;
    public Text m_playbackSpeedText;

    private enum State
    {
        Stopped,
        Playing,
        Paused
    }

    private State m_state = State.Stopped;
    private bool m_userSeeking;

    private const int SpeedCount = 3;
    private int m_speedIndex;

    void Start()
    {
        // black out the image until the video is ready to play
        m_playerSurface.color = Color.black;

        m_mediaPlayer.OnReady += OnVideoReady;
        m_mediaPlayer.OnVideoFirstFrameReady += OnFirstFrameReady;
        m_mediaPlayer.OnVideoError += OnVideoError;
        m_mediaPlayer.OnEnd += OnVideoEnd;
        m_progressBar.onValueChanged.AddListener(OnProgressChanged);
        m_progressBar.value = 0.0f;
        UpdatePlaybackSpeed();
    }

    void Update()
    {
        if (m_state == State.Playing && !m_userSeeking)
        {
            m_progressBar.value = m_mediaPlayer.GetSeekBarValue();
        }
    }

    private void SetCurrentState(State state)
    {
        m_state = state;
        UpdateUI();
        OnStateChanged();
    }

    protected virtual void OnStateChanged() { }

    private void OnVideoReady()
    {
        m_mediaPlayer.Play();
    }

    private void OnFirstFrameReady()
    {
        m_playerSurface.color = Color.white;
    }

    private void OnVideoError(MediaPlayerCtrl.MEDIAPLAYER_ERROR errorCode, MediaPlayerCtrl.MEDIAPLAYER_ERROR errorCodeExtra)
    {
        Stop();
    }

    private void OnVideoEnd()
    {
        Stop();
        m_progressBar.value = 1.0f;
    }

    protected void UpdateUI()
    {
        m_playButton.gameObject.SetActive(m_state != State.Playing);
        m_pauseButton.SetActive(m_state == State.Playing);
        m_stopButton.interactable = m_state != State.Stopped;
        m_progressBar.interactable = m_state != State.Stopped;
    }
    
    public void SetUrl(string url)
    {
        m_mediaPlayer.m_strFileName = url;
    }

    public void Play()
    {
        if (m_state == State.Playing) { return; }

        m_mediaPlayer.Play();
        m_playbackTimeText.text = TimeUtils.GetHHmmssString(0);
        SetCurrentState(State.Playing);
    }

    public void Stop()
    {
        if (m_state == State.Stopped) { return; }

        m_mediaPlayer.Stop();
        SetCurrentState(State.Stopped);
    }

    public void Pause()
    {
        if (m_state != State.Playing) { return; }

        m_mediaPlayer.Pause();
        SetCurrentState(State.Paused);
    }

    public void Abort()
    {
        Stop();
        m_mediaPlayer.UnLoad();
    }

    public void OnProgressChanged(float value)
    {
        float seconds = m_mediaPlayer.GetSeekPosition() / 1000.0f;
        m_playbackTimeText.text = TimeUtils.GetHHmmssString(Mathf.RoundToInt(seconds));
    }

    public void OnProgressBarDown()
    {
        if (m_state != State.Stopped)
        {
            m_userSeeking = true;
            m_mediaPlayer.Pause();
        }
    }

    public void OnProgressBarUp()
    {
        if (m_state != State.Stopped)
        {
            m_userSeeking = false;
            m_mediaPlayer.SetSeekBarValue(m_progressBar.value);
            if (m_state != State.Paused)
            {
                m_mediaPlayer.Play();
            }
        }
    }

    public void OnCyclePlaybackSpeed()
    {
        m_speedIndex = (m_speedIndex + 1) % SpeedCount;
        UpdatePlaybackSpeed();
    }

    private void UpdatePlaybackSpeed()
    {
        switch (m_speedIndex)
        {
        case 0:
            m_playbackSpeedText.text = "1x";
            m_mediaPlayer.SetSpeed(1f);
            break;
        case 1:
            m_playbackSpeedText.text = "1.5x";
            m_mediaPlayer.SetSpeed(1.5f);
            break;
        case 2:
            m_playbackSpeedText.text = "2x";
            m_mediaPlayer.SetSpeed(2);
            break;
        }
    }
}
