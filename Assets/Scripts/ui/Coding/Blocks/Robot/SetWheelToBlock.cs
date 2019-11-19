using Robomation;
using System.Collections;

public class SetWheelToBlock : RobotBlockBase
{
    private WheelMenuPlugins m_wheelMenu;

	protected override void Start()
	{
		base.Start();
        m_wheelMenu = GetComponentInChildren<WheelMenuPlugins>();
	}

    protected override IEnumerator DoAction(BlockState state)
    {
        int speed;
        int.TryParse(state.slotValues[1], out speed);

        switch (m_wheelMenu.GetMenuValue())
        {
        case "down_menu_left_wheel":
            SetLeftWheelTo(state, speed);
            break;

        case "down_menu_right_wheel":
            SetRightWheelTo(state, speed);
            break;

        case "down_menu_both_wheel":
            SetLeftWheelTo(state, speed);
            SetRightWheelTo(state, speed);
            break;
        }

        yield break;
    }

    private void SetLeftWheelTo(BlockState state, int speed)
    {
        state.runtimeState.leftWheelSpeed = speed;
        state.robot.write(Hamster.LEFT_WHEEL, speed);
    }

    private void SetRightWheelTo(BlockState state, int speed)
    {
        state.runtimeState.rightWheelSpeed = speed;
        state.robot.write(Hamster.RIGHT_WHEEL, speed);
    }
}