using Robomation;
using System.Collections;
using UnityEngine;

public abstract class TurnForSecBlockBase : RobotBlockBase
{
	TurnroundPlugins m_Turn;

	protected override void Start()
	{
		base.Start();
		m_Turn = GetComponentInChildren<TurnroundPlugins>();
    }

    protected override IEnumerator DoAction(BlockState state)
    {
        int left = 0;
        int right = 0;
        string dir = m_Turn.GetMenuValue();
        if ("down_menu_turn_right" == dir)
        {
            left = 30;
            right = -30;
        }
        else if ("down_menu_turn_left" == dir)
        {
            left = -30;
            right = 30;
        }
        else if ("down_menu_move_forward" == dir)
        {
            left = state.runtimeState.leftWheelSpeed;
            right = state.runtimeState.rightWheelSpeed;
        }
        else if ("down_menu_move_backward" == dir)
        {
            left = -state.runtimeState.leftWheelSpeed;
            right = -state.runtimeState.rightWheelSpeed;
        }
        state.robot.write(Hamster.LEFT_WHEEL, left);
        state.robot.write(Hamster.RIGHT_WHEEL, right);

        yield return new WaitForSeconds(GetTurnTime(state));

        state.robot.write(Hamster.LEFT_WHEEL, 0);
        state.robot.write(Hamster.RIGHT_WHEEL, 0);
    }

    protected abstract float GetTurnTime(BlockState state);
}

