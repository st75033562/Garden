using Robomation;
using System.Collections;

public class ChangeBothWheelsByBlock : RobotBlockBase
{
    protected override IEnumerator DoAction(BlockState state)
    {
        int left, right;
        int.TryParse(state.slotValues[1], out left);
        int.TryParse(state.slotValues[2], out right);

        var robot = CodeContext.robotManager.get(state.robotIndex);
        var runtimeState = CodeContext.robotRuntime.GetState(state.robotIndex);

        robot.write(Hamster.LINE_TRACER_MODE, Hamster.LINE_TRACER_MODE_OFF);

        runtimeState.leftWheelSpeed += left;
        robot.write(Hamster.LEFT_WHEEL, runtimeState.leftWheelSpeed);

        runtimeState.rightWheelSpeed += right;
        robot.write(Hamster.RIGHT_WHEEL, runtimeState.rightWheelSpeed);

        yield break;
    }
}
