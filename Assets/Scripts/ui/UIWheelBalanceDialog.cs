using UnityEngine;
using UnityEngine.UI;
using Robomation;
using System.Collections;

public class UIWheelBalanceDialog : MonoBehaviour
{
    public Text robotName;
    public Text testButtonText;
    public Text balanceText;
    public Slider balanceSlider;
    public Button addBalanceButton;
    public Button decreaseBalanceButton;
    public int defaultTestSpeed = 40;

    private HamsterRobot m_robot;
    private bool m_testing;

    private const int MaxBalance = sbyte.MaxValue;
    private const int MinBalance = sbyte.MinValue;

    public void Init(HamsterRobot robot)
    {
        m_robot = robot;
        robotName.text = robot.getName();

        balanceSlider.minValue = MinBalance;
        balanceSlider.maxValue = MaxBalance;
        curBalance = 0;

        OnBalanceSliderValueChanged();

        m_testing = false;
        UpdateTestButton();
    }

    public void Close()
    {
        m_testing = false;
        m_robot.resetDevices();
        m_robot = null;
        gameObject.SetActive(false);
    }

    public int curBalance
    {
        get { return (int)balanceSlider.value; }
        set { balanceSlider.value = value; }
    }

    public void OnClickSave()
    {
        m_robot.write(Hamster.WHEEL_BALANCE, curBalance);
        m_robot.saveWheelBalance();
        Close();
    }

    public void OnClickTest()
    {
        if (!m_testing)
        {
            m_robot.write(Hamster.LEFT_WHEEL, defaultTestSpeed);
            m_robot.write(Hamster.RIGHT_WHEEL, defaultTestSpeed);
        }
        else
        {
            m_robot.write(Hamster.LEFT_WHEEL, 0);
            m_robot.write(Hamster.RIGHT_WHEEL, 0);
        }

        m_testing = !m_testing;
        UpdateTestButton();
    }

    private void UpdateTestButton()
    {
        testButtonText.text =  m_testing ? "ui_wheel_balance_stop".Localize() : "ui_wheel_balance_test".Localize();
    }

    public void OnChangeBalance(int step)
    {
        curBalance = curBalance + step;
    }

    public void OnBalanceSliderValueChanged()
    {
        m_robot.write(Hamster.WHEEL_BALANCE, curBalance);

        addBalanceButton.interactable = curBalance < MaxBalance;
        decreaseBalanceButton.interactable = curBalance > MinBalance;
        balanceText.text = curBalance.ToString("+#;-#;0");
    }
}
