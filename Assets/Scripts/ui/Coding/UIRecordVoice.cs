using UnityEngine;
using UnityEngine.UI;
using System.Collections;

using g_WebRequestManager = Singleton<WebRequestManager>;
using System;
using UnityEngine.Events;

public class UIRecordVoice : MonoBehaviour
{
    public GameObject m_CloseButton;
	public GameObject m_NormalBack;
	public GameObject m_PressBack;
	public GameObject[] m_LeftWave;
	public GameObject[] m_RightWave;
	public Text m_Title;
	public LeaveMessagePanel m_Panel;
	public VoiceRecorder m_Recorder;
	public UIMaskBase m_Mask;

    bool m_requestingPermission;
	bool m_bRecord = false;
	float m_CurTime;
	const float m_MaxTime = 60.0f;
	const float m_AnimSpaceTime = 0.5f;
	int m_CurAnimIndex;
	float m_AnimTime;
	byte[] m_VoiceData = null;


	// Use this for initialization
	void Start()
	{
		m_Title.text = "record_start_record".Localize();
    }

	// Update is called once per frame
	void Update()
	{
		if(m_bRecord)
		{
			m_CurAnimIndex = (int)(m_AnimTime / 0.5f);
			m_Title.text = string.Format("record_release_record".Localize(), m_CurTime.ToString("F0"));

			m_CurTime -= Time.deltaTime;
			m_AnimTime += Time.deltaTime;
            if (m_CurTime <= 0)
			{
				RecordEnd();
            }
			int tCurAnimIndex = m_CurAnimIndex % m_RightWave.Length;
			for(int i = 0; i < m_RightWave.Length; ++i)
			{
				bool tShow = i > tCurAnimIndex ? false : true;
                m_LeftWave[i].SetActive(tShow);
				m_RightWave[i].SetActive(tShow);
			}
		}
	}

	public void SetActive(bool show)
	{
        if (gameObject.activeSelf == show)
        {
            return;
        }

		gameObject.SetActive(show);
        ReleaseRecordState();
    }

	public void RecordStart()
	{
        if (m_bRecord)
        {
            return;
        }

        if (m_requestingPermission)
        {
            Debug.LogError("requesting permission");
            return;
        }

        StartCoroutine(StartRecordImpl());
    }

    private IEnumerator StartRecordImpl()
    {
        m_requestingPermission = true;
        yield return Application.RequestUserAuthorization(UserAuthorization.Microphone);
        m_requestingPermission = false;

        if (Application.HasUserAuthorization(UserAuthorization.Microphone))
        {
            m_CloseButton.SetActive(false);
            m_bRecord = true;
            m_CurTime = m_MaxTime;
            m_AnimTime = 0.0f;
            m_NormalBack.SetActive(false);
            m_PressBack.SetActive(true);
            m_Recorder.Begin();
        }
        else
        {
            PopupManager.Notice("ui_warning_microphone_disabled".Localize());
        }
    }

	public void RecordEnd()
	{
		if(!m_bRecord)
		{
			return;
		}

        ReleaseRecordState();

		if (null != m_Recorder.clip)
		{
			m_VoiceData = m_Recorder.clip;
			string tGuidFileName = Guid.NewGuid().ToString("N");
            Uploads.UploadAudio(m_VoiceData, tGuidFileName)
                .Blocking()
                .Success(() => { SendSuccessCallBack(tGuidFileName); })
                .Execute();
		}
		else
		{
			print("not record data");
		}
	}

	void ReleaseRecordState()
	{
        m_bRecord = false;
        m_Recorder.End();
        StopAllCoroutines();
        m_requestingPermission = false;
        m_CloseButton.SetActive(true);

		m_Title.text = "record_start_record".Localize();
		m_NormalBack.SetActive(true);
		m_PressBack.SetActive(false);
		for (int i = 0; i < m_RightWave.Length; ++i)
		{
			m_LeftWave[i].SetActive(true);
			m_RightWave[i].SetActive(true);
		}
	}

	private void SendSuccessCallBack(string tVoiceName)
	{
        m_Panel.VoiceRepo.save(tVoiceName, m_VoiceData);
		m_VoiceData = null;

		m_Panel.SaveVoice(tVoiceName);
		SetActive(false);
	}
}
