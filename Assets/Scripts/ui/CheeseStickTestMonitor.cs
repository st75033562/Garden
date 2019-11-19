using Robomation;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CheeseStickTestMonitor : MonoBehaviour
{
    public Dropdown m_dropDownSaMode;
    public Dropdown m_dropDownSbMode;
    public Dropdown m_dropDownScMode;

    public Slider m_sliderOutSa;
    public Slider m_sliderOutSb;
    public Slider m_sliderOutSc;

    public Toggle m_togglePullSa;
    public Toggle m_toggleADCSa;

    public Toggle m_togglePullSb;
    public Toggle m_toggleADCSb;

    public Toggle m_togglePullSc;
    public Toggle m_toggleADCSc;

    public Text m_freeFallIdText;
    public Text m_tapIdText;

    public Text m_infoText;
    public InputField m_inputSoundClip;

    private IRobot m_robot;

    IEnumerator Start()
    {
        InitRobot();

        OnSaModeChanged(m_dropDownSaMode.value);
        OnSbModeChanged(m_dropDownSbMode.value);
        OnScModeChanged(m_dropDownScMode.value);

        CallbackQueue.EnsureInstance();
        yield return LocalizationManager.instance.loadData();

        RobotManager.instance.onInitialized += OnInitialized;
    }

    void InitRobot()
    {
        m_robot = RobotManager.instance.robots
                    .OfType<CheeseStickRobot>()
                    .FirstOrDefault() ?? (IRobot)NullRobot.instance;

        if (m_robot == NullRobot.instance)
        {
            m_infoText.text = "Cheese Not Found";
        }
        else
        {
            m_infoText.text = "Cheese Found";
        }
    }

    void OnDestroy()
    {
        RobotManager.instance.onInitialized -= OnInitialized;
    }

    private void OnInitialized(bool ok)
    {
        if (ok)
        {
            StartCoroutine(ScanAndStop());
        }
        else
        {
            m_infoText.text = "Initialization Failed";
        }
    }

    IEnumerator ScanAndStop()
    {
        RobotManager.instance.stopScan();
        m_infoText.text = "Scanning";
        RobotManager.instance.startScan();

        yield return new WaitForSeconds(5);

        RobotManager.instance.stopScan();
        InitRobot();
    }

    public void RefreshRobot()
    {
        if (RobotManager.instance.state == RobotManager.State.Invalid)
        {
            m_infoText.text = "Initializing";
            RobotManager.instance.initialize();
        }
        else
        {
            m_infoText.text = "Refreshing";
            StartCoroutine(ScanAndStop());
        }
    }

    public void OnBuzzeChanged(float value)
    {
        m_robot.write(CheeseStick.BUZZ, (int)value);
    }

    public void OnSoundOutChanged(int option)
    {
        m_robot.write(CheeseStick.SOUND_OUT, option);
    }

    public void OnConfigLModeChanged(int option)
    {
        m_robot.write(CheeseStick.CONFIG_L_MODE, option);
    }

    public void OnConfigLaChanged(int option)
    {
        m_robot.write(CheeseStick.CONFIG_LA, option);
    }

    public void OnConfigLbChanged(int option)
    {
        m_robot.write(CheeseStick.CONFIG_LB, option);
    }

    public void OnConfigLcChanged(int option)
    {
        m_robot.write(CheeseStick.CONFIG_LC, option);
    }

    public void OnOutLaChanged(float value)
    {
        m_robot.write(CheeseStick.OUT_LA, (int)value);
    }

    public void OnOutLbChanged(float value)
    {
        m_robot.write(CheeseStick.OUT_LB, (int)value);
    }

    public void OnOutLcChanged(float value)
    {
        m_robot.write(CheeseStick.OUT_LC, (int)value);
    }

    public void OnConfigMModeChanged(int option)
    {
        m_robot.write(CheeseStick.CONFIG_M_MODE, option);
    }

    public void OnConfigMStepChanged(int option)
    {
        m_robot.write(CheeseStick.CONFIG_M_STEP, option);
    }

    public void OnConfigMCycleChanged(int option)
    {
        m_robot.write(CheeseStick.CONFIG_M_CYCLE, option);
    }

    public void OnBandwidthChanged(int option)
    {
        m_robot.write(CheeseStick.BANDWIDTH, option);
    }

    public void OnGRangeChanged(int option)
    {
        m_robot.write(CheeseStick.G_RANGE, option);
    }

    public void OpenMonitor()
    {
        var dialog = UIDialogManager.g_Instance.GetDialog<UIMonitorDialog>();
        dialog.Configure(RobotManager.instance, new VariableManager(), false);
    }

    public void OnSaModeChanged(int option)
    {
        m_robot.write(CheeseStick.CONFIG_SA, option);
        m_sliderOutSa.interactable = option > CheeseStick.S_MODE_ANALOG;
        m_toggleADCSa.interactable = m_togglePullSa.interactable = !m_sliderOutSa.interactable;
    }

    public void OnSaValueChanged(float value)
    {
        m_robot.write(CheeseStick.OUT_SA, (int)value);
    }

    public void OnPullSaChanged(bool isOn)
    {
        m_robot.write(CheeseStick.PULL_SA, isOn ? CheeseStick.PULL_UP : CheeseStick.PULL_DOWN);
    }

    public void OnADCSaChanged(bool isOn)
    {
        m_robot.write(CheeseStick.ADC_SA, isOn ? CheeseStick.ADC_VOLTAGE_POWER_REF : CheeseStick.ADC_VOLTAGE_REF);
    }

    public void OnSbModeChanged(int option)
    {
        m_robot.write(CheeseStick.CONFIG_SB, option);
        m_sliderOutSb.interactable = option > CheeseStick.S_MODE_ANALOG;
        m_toggleADCSb.interactable = m_togglePullSb.interactable = !m_sliderOutSb.interactable;
    }

    public void OnSbValueChanged(float value)
    {
        m_robot.write(CheeseStick.OUT_SB, (int)value);
    }

    public void OnPullSbChanged(bool isOn)
    {
        m_robot.write(CheeseStick.PULL_SB, isOn ? CheeseStick.PULL_UP : CheeseStick.PULL_DOWN);
    }

    public void OnADCSbChanged(bool isOn)
    {
        m_robot.write(CheeseStick.ADC_SB, isOn ? CheeseStick.ADC_VOLTAGE_POWER_REF : CheeseStick.ADC_VOLTAGE_REF);
    }

    public void OnScModeChanged(int option)
    {
        m_robot.write(CheeseStick.CONFIG_SC, option);
        m_sliderOutSc.interactable = option > CheeseStick.S_MODE_ANALOG;
        m_toggleADCSc.interactable = m_togglePullSc.interactable = !m_sliderOutSc.interactable;
    }

    public void OnScValueChanged(float value)
    {
        m_robot.write(CheeseStick.OUT_SC, (int)value);
    }

    public void OnPullScChanged(bool isOn)
    {
        m_robot.write(CheeseStick.PULL_SC, isOn ? CheeseStick.PULL_UP : CheeseStick.PULL_DOWN);
    }

    public void OnADCScChanged(bool isOn)
    {
        m_robot.write(CheeseStick.ADC_SC, isOn ? CheeseStick.ADC_VOLTAGE_POWER_REF : CheeseStick.ADC_VOLTAGE_REF);
    }

    public void OnPPSValueChanged(float value)
    {
        m_robot.write(CheeseStick.PPS, (int)value);
    }

    public void PlaySoundClip()
    {
        m_robot.write(CheeseStick.SOUND_CLIP, int.Parse(m_inputSoundClip.text));
    }

    public void OnNoteChanged(float note)
    {
        m_robot.write(CheeseStick.PIANO_NOTE, (int)note);
    }

    void Update()
    {
        m_freeFallIdText.text = m_robot.read(CheeseStick.FREE_FALL_ID).ToString();
        m_tapIdText.text = m_robot.read(CheeseStick.TAP_ID).ToString();
    }
}
