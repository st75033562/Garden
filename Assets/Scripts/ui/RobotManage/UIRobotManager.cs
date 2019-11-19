using Robomation;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class UIRobotManager : SceneController
{
	public Text m_Status;
	public GameObject m_btnScan;
	public GameObject m_btnStopScan;
	public RectTransform m_Container;
	public GameObject m_RobotNodeTemplate;
    public UIWheelBalanceDialog m_WheelBalanceDialog;

    public GameObject m_SignalStrengthButton;
    public UISignalStrengthDialog m_SignalStrengthDialog;

    public Animation m_scanningAnim;
    public GameObject m_programButton;

	List<UIRobotNode> m_RobotItems = new List<UIRobotNode>();
	Coroutine m_ScanCoroutine;
	const float m_AutoScanTime = 10.0f;

    private UIConnectedRobots m_robots;

	protected override void Awake()
	{
        base.Awake();

		m_RobotNodeTemplate.SetActive(false);
        UpdateScanBtn(false);

#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        m_SignalStrengthButton.SetActive(false);
#endif
    }

	protected override void Start()
	{
        base.Start();

        RobotManager.instance.onInitialized += OnInitialized;
		RobotManager.instance.onConnectionEnabled += OnConnectionEnabled;
        RobotManager.instance.onReset += OnReset;

		Input.gyro.enabled = true;
#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
        // TODO: whether this is necessary
		Screen.sleepTimeout = SleepTimeout.NeverSleep;
#endif
        m_robots = new UIConnectedRobots(RobotManager.instance);
        m_robots.onRobotAdded += OnRobotDiscovered;
        m_robots.onRobotRemoved += OnRobotRemoved;

        RemoveUnconnectedRobots();

        switch (RobotManager.instance.state)
        {
        case RobotManager.State.Invalid:
			m_Status.text = "connection_initializing".Localize();
			RobotManager.instance.initialize();
            break;

        case RobotManager.State.Initializing:
            m_Status.text = "connection_initializing".Localize();
            break;

        case RobotManager.State.Initialized:
			m_ScanCoroutine = StartCoroutine(AutoScanRobot());
            foreach(var robot in m_robots)
            {
                OnRobotDiscovered(robot);
            }
            break;

        case RobotManager.State.Resetting:
            m_Status.text = "connection_resetting".Localize();
            break;

        default:
            Debug.LogError("unhandled state: " + RobotManager.instance.state);
            break;
        }

        m_programButton.SetActive(UserManager.Instance.appRunModel != AppRunModel.Guide);
	}

    public override void Init(object userData, bool isRestored)
    {
        base.Init(userData, isRestored);

        if (userData != null)
        {
            ShowVisualPrograms((IRepositoryPath)userData);
        }
    }

	protected override void OnDestroy()
	{
        base.OnDestroy();

        RobotManager.instance.onInitialized -= OnInitialized;
        RobotManager.instance.onConnectionEnabled -= OnConnectionEnabled;
        RobotManager.instance.onReset -= OnReset;

		RobotManager.instance.stopScan();

        RemoveUnconnectedRobots();
        m_robots.Dispose();
	}

    void OnInitialized(bool success)
    {
		if (success)
		{
            if (RobotManager.instance.isConnectionEnabled)
            {
                m_ScanCoroutine = StartCoroutine(AutoScanRobot());
            }
            else
            {
                EnableConnection();
            }
		}
		else
		{
            if (Application.platform == RuntimePlatform.Android)
            {
				m_Status.text = "connection_ble_not_supported".Localize();
            }
            else
            {
                m_Status.text = "connection_failed_to_init".Localize();
            }
		}
    }

    private void EnableConnection()
    {
        if (RobotManager.instance.enableConnection())
        {
            if (!RobotManager.instance.isConnectionEnabled)
            {
                if (Application.platform == RuntimePlatform.Android)
                {
                    m_Status.text = "connection_enabling_ble".Localize();
                }
                else if (Application.platform == RuntimePlatform.IPhonePlayer ||
                         Application.platform == RuntimePlatform.OSXEditor ||
                         Application.platform == RuntimePlatform.OSXPlayer)
                {
                    m_Status.text = "connection_ble_off".Localize();
                }
                else
                {
                    m_Status.text = "";
                    Debug.LogError("unhandled platform: " + Application.platform);
                }
            }
            else
            {
                m_ScanCoroutine = StartCoroutine(AutoScanRobot());
            }
        }
        else
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                m_Status.text = "connection_enabling_ble_error".Localize();
            }
            else
            {
                m_Status.text = "connection_failed_to_enable_connection".Localize();
            }
        }
    }

	void OnConnectionEnabled(bool enabled)
	{
        if (RobotManager.instance.state == RobotManager.State.Initialized)
        {
            if (enabled)
            {
                m_ScanCoroutine = StartCoroutine(AutoScanRobot());
            }
            else
            {
                m_Status.text = "connection_ble_off".Localize();
                StopScanRobot(false);
            }
        }
        else
        {
            Debug.Log("connection state changed: " + enabled);
        }
	}

	IEnumerator AutoScanRobot()
	{
        var scanResult = RobotManager.instance.startScan();
        Debug.Log("scan robot: " + scanResult);

        switch (scanResult)
        {
        case RobotManager.Error.None:
            UpdateScanBtn(true);
            m_Status.text = "connection_scan_for_secs".Localize();
            yield return new WaitForSeconds(m_AutoScanTime);
            StopScanRobot();
            break;

        case RobotManager.Error.NoConnection:
            EnableConnection();
            break;

        case RobotManager.Error.Failed:
            ResetBLE();
            break;

        case RobotManager.Error.NotReady:
            Debug.Log("scanning not ready");
            break;
        }

        m_ScanCoroutine = null;
    }

    void RemoveUnconnectedRobots()
    {
        // avoid unconnected robots blocking connected ones
        foreach (var robot in RobotManager.instance.robots.Except(m_robots).ToArray())
        {
            RobotManager.instance.remove(robot);
        }
    }

	public void OnRobotDiscovered(Robot robot)
	{
        if (string.IsNullOrEmpty(robot.getName()))
        {
            robot.setName("default_robot_name".Localize());
        }

		UIRobotNode robotNode = GetRobotUI();
		robotNode.SetRobotData(robot);
        if (UserManager.Instance.appRunModel == AppRunModel.Guide)
        {
            SceneDirector.Push("SmartClassScene");
        }
    }

	public void OnRobotRemoved(Robot robot)
	{
		for(int i = 0; i < m_RobotItems.Count; ++i)
		{
			if(m_RobotItems[i].GetRobotData() == robot)
			{
                Destroy(m_RobotItems[i].gameObject);
                m_RobotItems.RemoveAt(i);
                UpdateRobotIndex();
				break;
            }
		}
	}

	void UpdateScanBtn(bool scan)
	{
		m_btnScan.SetActive(!scan);
		m_btnStopScan.SetActive(scan);
        if (scan)
        {
            m_scanningAnim.Play();
        }
        else
        {
            m_scanningAnim.Stop();
        }
	}

    public void OnClickScan()
    {
        if (RobotManager.instance.state == RobotManager.State.Initialized)
        {
            m_ScanCoroutine = StartCoroutine(AutoScanRobot());
        }
    }

    public void OnClickStopScan()
    {
        StopScanRobot();
    }

    private void ResetBLE()
    {
        UpdateScanBtn(false);

        m_Status.text = "connection_resetting_ble".Localize();
        RobotManager.instance.reset();
        m_robots.Clear();

        foreach (var robot in m_RobotItems)
        {
            Destroy(robot.gameObject);
        }
        m_RobotItems.Clear();
    }

    private void OnReset(bool success)
    {
        Debug.Log("reset finished: " + success);
        if (success)
        {
            m_ScanCoroutine = StartCoroutine(AutoScanRobot());
        }
        else
        {
            m_Status.text = "connection_ble_reset_failed".Localize();
        }
    }

	private void StopScanRobot(bool updateStateText = true)
	{
		if(null != m_ScanCoroutine)
		{
			StopCoroutine(m_ScanCoroutine);
			m_ScanCoroutine = null;
		}
		RobotManager.instance.stopScan();
		UpdateScanBtn(false);

        if (updateStateText)
        {
            m_Status.text = "connection_connected_robots".Localize();
        }
	}

    public void OnClickProgram()
    {
        if (UserManager.Instance.appRunModel == AppRunModel.Normal &&
            Preference.scriptLanguage == ScriptLanguage.Python)
        {
            PopupManager.PythonProjectView();
        }
        else
        {
            ShowVisualPrograms(null);
        }
    }

	void ShowVisualPrograms(IRepositoryPath initialDir)
	{
        PopupManager.ProjectView(path => {
            SceneDirector.Push("Main", 
                               CodeSceneArgs.FromPath(path.ToString()),
                               path.parent);
        }, initialDir: initialDir);
    }

	UIRobotNode GetRobotUI()
	{
		GameObject robotGo = Instantiate(m_RobotNodeTemplate, m_Container) as GameObject;
		robotGo.SetActive(true);
		var robot = robotGo.GetComponent<UIRobotNode>();
		m_RobotItems.Add(robot);

		robot.SetRobotIndex(m_RobotItems.Count - 1);

		return robot;
	}

	void UpdateRobotIndex()
	{
		for(int i = 0; i < m_RobotItems.Count; ++i)
		{
			m_RobotItems[i].SetRobotIndex(i);
        }
	}

	public void OnClickDelete(UIRobotNode obj)
	{
        m_robots.Remove(obj.GetRobotData());
    }

	public void OnClickEdit(UIRobotNode obj)
	{
        PopupManager.InputDialog("input_robot_name".Localize(), obj.RotbotName(), "", (str) => {
            EditComplete(obj.GetRobotData(), str);
        }, null);
    }

	public void EditComplete(Robot robot, string name)
	{
        robot.setName(name);
		for(int i = 0; i < m_RobotItems.Count; ++i)
		{
			if(m_RobotItems[i].GetRobotData() == robot)
			{
				m_RobotItems[i].UpdateRobot();
				break;
            }
		}
	}

	public void BackLastSence()
	{
		SceneDirector.Pop();
	}

    public void OnClickSetting(UIRobotNode node)
    {
        m_WheelBalanceDialog.gameObject.SetActive(true);
        m_WheelBalanceDialog.Init((HamsterRobot)node.GetRobotData());
    }

    public void OnClickSignal()
    {
        m_SignalStrengthDialog.gameObject.SetActive(true);
    }
}
