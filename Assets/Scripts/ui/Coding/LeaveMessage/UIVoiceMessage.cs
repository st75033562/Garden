using UnityEngine;
using UnityEngine.UI;
using System.Collections;

using g_WebRequestManager = Singleton<WebRequestManager>;

public class UIVoiceMessage : MessageElementBase
{
	public GameObject[] m_WaveImages;
	public UIMaskBase m_Mask;
	public VoicePlayer m_VoicePlayer;

	bool m_bPlaying;

	const float m_AnimSpaceTime = 0.5f;
	int m_CurWaveIndex;
	float m_AnimTime;

	// Use this for initialization
	void Start()
	{
        ShowAllWaves();
	}

	// Update is called once per frame
	void Update()
	{
		if (m_bPlaying)
		{
			m_CurWaveIndex = (int)(m_AnimTime / 0.5f) % m_WaveImages.Length;
			UpdateWaves();
			m_AnimTime += Time.deltaTime;
		}
	}

    private void ShowAllWaves()
    {
        m_CurWaveIndex = m_WaveImages.Length - 1;
        UpdateWaves();
    }

    private void UpdateWaves()
    {
        for (int i = 0; i < m_WaveImages.Length; ++i)
        {
            m_WaveImages[i].SetActive(i <= m_CurWaveIndex);
        }
    }

	public void ClickPlay()
	{
		if (m_bPlaying)
		{
			return;
		}

		byte[] tVoiceData = m_MessageTag.m_MessagePanel.VoiceRepo.load(Message.TextLeaveMessage);
		if (null == tVoiceData)
		{
            var request = Downloads.DownloadVoice(Message.m_UserID, Message.TextLeaveMessage);
            request.blocking = true;
            request.userData = Message.TextLeaveMessage;
            request.Success(x => GetVoiceSuccessCallBack((string)request.userData, x))
                   .Execute();
		}
		else
		{
			PlayVoice(tVoiceData);
		}
	}

	private void GetVoiceSuccessCallBack(string name, byte[] data)
	{
        m_MessageTag.m_MessagePanel.VoiceRepo.save(name, data);
		PlayVoice(data);
    }

	void PlayVoice(byte[] data)
	{
		m_VoicePlayer.onBeginPlay += BeginPlayEvent;
		if (!m_VoicePlayer.Play(data))
		{
			m_VoicePlayer.onBeginPlay -= BeginPlayEvent;
		}
	}

	public void BeginPlayEvent()
	{
		m_VoicePlayer.onBeginPlay -= BeginPlayEvent;
		m_VoicePlayer.onStop += StopPlayEvent;

		m_bPlaying = true;
	}

	public void StopPlayEvent()
	{
		m_VoicePlayer.onStop -= StopPlayEvent;
		Reset();
    }

	void Reset()
	{
		m_bPlaying = false;
        ShowAllWaves();
	}

	void OnDestroy()
	{
		if(m_bPlaying)
		{
			m_VoicePlayer.onStop -= StopPlayEvent;
			m_VoicePlayer.Stop();
        }
	}
}
