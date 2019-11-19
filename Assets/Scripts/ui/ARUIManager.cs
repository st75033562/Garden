using AR;
using cn.sharerec;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ARUIManager : MonoBehaviour {
    public UnityEvent m_OnHide;

    public MarkerTracker m_MarkerTracker;
    public ArWebCamTextureToMatHelper m_WebCamTextureToMatHelper;
    public Camera m_VideoCamera;
    public ARSceneManager m_SceneManager;

	public Button m_StartRecord;
	public Button m_StopRecord;

    public UIRecordTimer m_RecordingTimer;
	public GameObject m_RecorderTimeGo;

	public GameObject[] m_HideObj;

    bool m_trackingStarted;

    void Awake()
	{
		gameObject.SetActive(false);
		m_RecorderTimeGo.SetActive(false);
    }

	void Start ()
	{
		m_StopRecord.gameObject.SetActive(false);
        m_StartRecord.gameObject.SetActive(Application.isMobilePlatform || Application.isEditor);
        m_RecordingTimer.gameObject.SetActive(false);
        VideoRecorder.onStopRecording += OnStopRecording;

        if (m_WebCamTextureToMatHelper.isInited())
        {
            OnWebCamTextureInit();
        }
        else
        {
            m_WebCamTextureToMatHelper.OnInitedEvent.AddListener(OnWebCamTextureInit);
        }
	}

    void OnDestroy()
    {
        VideoRecorder.onStopRecording -= OnStopRecording;
    }

    void OnDisable()
    {
        if (m_MarkerTracker)
        {
            m_MarkerTracker.StopTracking();
        }

        if (m_WebCamTextureToMatHelper)
        {
            m_WebCamTextureToMatHelper.Stop();
            m_WebCamTextureToMatHelper.GetComponent<Renderer>().enabled = false;
        }

        if (m_VideoCamera)
        {
            m_VideoCamera.enabled = false;
        }

        if (m_SceneManager)
        {
            m_SceneManager.RenderingOn = false;
        }
    }

    void OnEnable()
    {
        if (m_trackingStarted)
        {
            m_MarkerTracker.StartTracking();
        }
        m_WebCamTextureToMatHelper.GetComponent<Renderer>().enabled = true;
        m_WebCamTextureToMatHelper.Play();
        m_VideoCamera.enabled = true;
        m_SceneManager.RenderingOn = true;
        m_SceneManager.ActivateSceneObjects(true);
    }

	public void StartRecorder()
	{
        IsRecording = true;
        VideoRecorder.StartRecording();
        // record for minimum required time
        m_StopRecord.interactable = false;
        StartCoroutine(EnableStopRecordButton());

		m_StartRecord.gameObject.SetActive(false);
		m_StopRecord.gameObject.SetActive(true);
        m_RecordingTimer.gameObject.SetActive(true);
        m_RecordingTimer.Begin();
		HideUIOnStartRecorder();
    }

    IEnumerator EnableStopRecordButton()
    {
        yield return new WaitForSecondsRealtime(VideoRecorder.minimumRecordingSeconds);
        m_StopRecord.interactable = true;
    }

	void HideUIOnStartRecorder()
	{
		for (int i = 0; i < m_HideObj.Length; ++i)
		{
			m_HideObj[i].SetActive(false);
		}
		m_RecorderTimeGo.SetActive(true);
    }

	void OnStopRecording()
	{
        IsRecording = false;
		m_StartRecord.gameObject.SetActive(true);
        m_RecordingTimer.gameObject.SetActive(false);
        m_RecordingTimer.End();
        ShowUIOnStopRecorder();

        if (gameObject.activeInHierarchy && VideoRecorder.lastVideoPath != null)
        {
            PopupManager.VideoPreview(SharedVideo.LocalFile(VideoRecorder.lastVideoPath));
        }
	}

    public void StopRecorder()
	{
		m_StopRecord.gameObject.SetActive(false);
        VideoRecorder.StopRecording();

        m_RecorderTimeGo.SetActive(false);
    }

    void ShowUIOnStopRecorder()
	{
		for (int i = 0; i < m_HideObj.Length; ++i)
		{
			m_HideObj[i].SetActive(true);
		}
		m_RecorderTimeGo.SetActive(false);
	}

    public void Show(bool visible)
    {
        gameObject.SetActive(visible);
        if (!visible)
        {
            if (m_OnHide != null)
            {
                m_OnHide.Invoke();
            }
        }
    }

    public void OnToggleAR(bool trackingStarted)
    {
        if(!UserManager.Instance.IsArUser) {
            PopupManager.ActivationCode(PopupActivation.Type.AR);
            return;
        }

        m_trackingStarted = trackingStarted;
        if (m_trackingStarted)
        {
            m_MarkerTracker.StartTracking();
        }
        else
        {
            m_MarkerTracker.StopTracking();
        }

        m_SceneManager.RenderingOn = trackingStarted;
    }

    public void OnWebCamTextureInit()
    {
        if (gameObject.activeInHierarchy)
        {
            m_WebCamTextureToMatHelper.Play();
        }
    }

    public bool IsRecording
    {
        get;
        private set;
    }

    public bool IsVisible
    {
        get { return gameObject.activeInHierarchy; }
    }

    public bool OnBackPressed()
    {
        if (!IsRecording)
        {
            Show(false);
            return true;
        }
        return false;
    }
}
