using Robomation;
using System.Collections;

public class ClearLedBlock : RobotBlockBase
{
	SelectLedPlugins m_LedPos;

	protected override void Start()
	{
		base.Start();
		m_LedPos = GetComponentInChildren<SelectLedPlugins>();
	}

	protected override IEnumerator DoAction(BlockState state)
	{
        string led = m_LedPos.GetMenuValue();
        if ("down_menu_left_led" == led)
        {
            state.robot.write(Hamster.LEFT_LED, Hamster.LED_OFF);
        }
        else if ("down_menu_right_led" == led)
        {
            state.robot.write(Hamster.RIGHT_LED, Hamster.LED_OFF);
        }
        else if ("down_menu_both_led" == led)
        {
            state.robot.write(Hamster.LEFT_LED, Hamster.LED_OFF);
            state.robot.write(Hamster.RIGHT_LED, Hamster.LED_OFF);
        }

        yield break;
	}
}
