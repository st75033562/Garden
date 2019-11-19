using Robomation;
using UnityEngine.UI;

public class UIMonitorHamster : UIMonitorRobot
{
	public Text m_X_Acceleration_Value;
	public Text m_Y_Acceleration_Value;
	public Text m_Z_Acceleration_Value;
	public Text m_Light_Value;
	public Text m_Left_Proximity_Value;
	public Text m_Right_Proximity_Value;
	public Text m_Left_Floor_Value;
	public Text m_Right_Floor_Value;
	public Text m_Temperature_Value;
	public Text m_Singnale_Strength_Value;
	public Text m_Input_A_Value;
	public Text m_Input_B_Value;

    protected override void DoUpdateReadings()
    {
        m_X_Acceleration_Value.text = m_Robot.read(Hamster.ACCELERATION, Hamster.ACCEL_X).ToString();
        m_Y_Acceleration_Value.text = m_Robot.read(Hamster.ACCELERATION, Hamster.ACCEL_Y).ToString();
        m_Z_Acceleration_Value.text = m_Robot.read(Hamster.ACCELERATION, Hamster.ACCEL_Z).ToString();
        m_Light_Value.text = m_Robot.read(Hamster.LIGHT).ToString();
        m_Left_Proximity_Value.text = m_Robot.read(Hamster.LEFT_PROXIMITY).ToString();
        m_Right_Proximity_Value.text = m_Robot.read(Hamster.RIGHT_PROXIMITY).ToString();
        m_Left_Floor_Value.text = m_Robot.read(Hamster.LEFT_FLOOR).ToString();
        m_Right_Floor_Value.text = m_Robot.read(Hamster.RIGHT_FLOOR).ToString();
        m_Temperature_Value.text = m_Robot.read(Hamster.TEMPERATURE).ToString();
        m_Singnale_Strength_Value.text = m_Robot.read(Hamster.SIGNAL_STRENGTH).ToString();
        m_Input_A_Value.text = m_Robot.read(Hamster.INPUT_A).ToString();
        m_Input_B_Value.text = m_Robot.read(Hamster.INPUT_B).ToString();
    }
}
