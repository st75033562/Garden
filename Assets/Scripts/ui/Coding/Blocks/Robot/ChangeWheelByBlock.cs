using Robomation;
using System.Collections;

public class ChangeWheelByBlock : RobotBlockBase
{
    private WheelMenuPlugins m_wheelMenu;

	protected override void Start()
	{
		base.Start();
        m_wheelMenu = GetComponentInChildren<WheelMenuPlugins>();
	}

    protected override IEnumerator DoAction(BlockState state)
    {
        int deltaSpeed;
        int.TryParse(state.slotValues[1], out deltaSpeed);

        switch (m_wheelMenu.GetMenuValue())
        {
        case "down_menu_left_wheel":
            ChangeLeftWheelSpeed(state, deltaSpeed);
            break;

        case "down_menu_right_wheel":
            ChangeRightWheelSpeed(state, deltaSpeed);
            break;

        case "down_menu_both_wheel":
            ChangeLeftWheelSpeed(state, deltaSpeed);
            ChangeRightWheelSpeed(state, deltaSpeed);
            break;
        }

        yield break;
    }

    private void ChangeLeftWheelSpeed(BlockState state, int delta)
    {
        state.runtimeState.leftWheelSpeed += delta;
        state.robot.write(Hamster.LEFT_WHEEL, state.runtimeState.leftWheelSpeed);
    }

    private void ChangeRightWheelSpeed(BlockState state, int delta)
    {
        state.runtimeState.rightWheelSpeed += delta;
        state.robot.write(Hamster.RIGHT_WHEEL, state.runtimeState.rightWheelSpeed);
    }
}
