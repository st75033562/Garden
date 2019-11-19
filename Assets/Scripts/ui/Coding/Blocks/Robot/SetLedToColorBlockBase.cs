using Robomation;
using System.Collections;
using UnityEngine;

public abstract class SetLedToColorBlockBase : RobotBlockBase
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
        int colorId = GetColorId(state);
        if ("down_menu_left_led" == led)
        {
            state.robot.write(Hamster.LEFT_LED, colorId);
        }
        else if ("down_menu_right_led" == led)
        {
            state.robot.write(Hamster.RIGHT_LED, colorId);
        }
        else if ("down_menu_both_led" == led)
        {
            state.robot.write(Hamster.LEFT_LED, colorId);
            state.robot.write(Hamster.RIGHT_LED, colorId);
        }

        var seconds = GetSeconds(state);
        if (seconds > 0)
        {
            yield return new WaitForSeconds(seconds);
        }
	}

    protected abstract int GetColorId(BlockState state);

    protected virtual float GetSeconds(BlockState state)
    {
        return 0;
    }
}
