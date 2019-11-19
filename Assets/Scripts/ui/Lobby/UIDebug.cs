using System;
using UnityEngine;
using UnityEngine.UI;


public class UIDebug : MonoBehaviour
{
    public GameObject m_Content;
	public Toggle m_ServerToggle;
    public Toggle m_PythonToggle;

    void Start()
    {
		m_ServerToggle.isOn = AppConfig.TestServer;
		// add manually to avoid triggering Logout
		m_ServerToggle.onValueChanged.AddListener(OnTestServerToggleChanged);
        m_PythonToggle.onValueChanged.AddListener(OnPythonToggleChanged);

        EventBus.Default.AddListener(EventId.UserLoggedIn, OnLoggedIn);
        gameObject.SetActive(AppConfig.DebugOn);
        m_Content.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.LeftControl) &&
            Input.GetKey(KeyCode.LeftAlt) &&
            Input.GetKey(KeyCode.LeftShift) &&
            Input.GetKeyDown(KeyCode.K) ||
            Input.touchCount >= 4 && Input.touches[3].phase == TouchPhase.Began)
        {
            m_Content.SetActive(!m_Content.activeSelf);
        }
    }

    void OnDestroy()
    {
        EventBus.Default.RemoveListener(EventId.UserLoggedIn, OnLoggedIn);
    }

    private void OnLoggedIn(object obj)
    {
        m_PythonToggle.isOn = UserManager.Instance.IsPythonUser;
    }

    private void OnPythonToggleChanged(bool selected)
    {
        UserManager.Instance.IsPythonUser = selected;
    }

	void OnTestServerToggleChanged(bool selected)
    {
        AppConfig.TestServer = selected;
		SocketManager.instance.serverPort = AppConfig.SocketServerPort;
		WebRequestManager.Default.UrlHost = AppConfig.WebServerUrl;
	}
}
