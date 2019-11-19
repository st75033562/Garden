using Robomation;
using UnityEngine.UI;

public class UIMonitorCheeseStick : UIMonitorRobot
{
	public Text m_X_Acceleration_Value;
	public Text m_Y_Acceleration_Value;
	public Text m_Z_Acceleration_Value;
	public Text m_Input_A_Value;
	public Text m_Input_B_Value;
	public Text m_Input_C_Value;
	public Text m_Input_La_Value;
	public Text m_Input_Lb_Value;
	public Text m_Input_Lc_Value;
	public Text m_Temperature_Value;
	public Text m_Singnal_Strength_Value;
	public Text m_Battery_Level_Value;

    protected override void DoUpdateReadings()
    {
        m_X_Acceleration_Value.text = m_Robot.read(CheeseStick.ACCELERATION, CheeseStick.ACCEL_X).ToString();
        m_Y_Acceleration_Value.text = m_Robot.read(CheeseStick.ACCELERATION, CheeseStick.ACCEL_Y).ToString();
        m_Z_Acceleration_Value.text = m_Robot.read(CheeseStick.ACCELERATION, CheeseStick.ACCEL_Z).ToString();
        m_Input_A_Value.text = m_Robot.read(CheeseStick.INPUT_A).ToString();
        m_Input_B_Value.text = m_Robot.read(CheeseStick.INPUT_B).ToString();
        m_Input_C_Value.text = m_Robot.read(CheeseStick.INPUT_C).ToString();
        m_Input_La_Value.text = m_Robot.read(CheeseStick.INPUT_LA).ToString();
        m_Input_Lb_Value.text = m_Robot.read(CheeseStick.INPUT_LB).ToString();
        m_Input_Lc_Value.text = m_Robot.read(CheeseStick.INPUT_LC).ToString();
        m_Temperature_Value.text = m_Robot.readFloat(CheeseStick.TEMPERATURE).ToString();
        m_Singnal_Strength_Value.text = m_Robot.read(CheeseStick.SIGNAL_STRENGTH).ToString();
        m_Battery_Level_Value.text = m_Robot.readFloat(CheeseStick.BATTERY_LEVEL).ToString();
    }
}
