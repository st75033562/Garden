using Robomation;
using System.Collections;

public class ClearBuzzerBlock : RobotBlockBase
{
    protected override IEnumerator DoAction(BlockState state)
    {
        state.runtimeState.buzzerPitch = 0;
        state.robot.writeFloat(Hamster.BUZZER, 0);

        yield break;
    }
}
