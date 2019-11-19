using Robomation;
using System.Collections;

public class ChangeBuzzerByBlock : RobotBlockBase
{
    protected override IEnumerator DoAction(BlockState state)
    {
        float deltaPitch;
        float.TryParse(state.slotValues[1], out deltaPitch);

        var runtimeState = state.runtimeState;
        runtimeState.buzzerPitch += deltaPitch;
        state.robot.writeFloat(Hamster.BUZZER, runtimeState.buzzerPitch);

        yield break;
    }
}
