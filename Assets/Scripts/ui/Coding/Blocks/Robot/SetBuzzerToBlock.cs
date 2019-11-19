using Robomation;
using System.Collections;
using UnityEngine;

public class SetBuzzerToBlock : RobotBlockBase
{
    protected override IEnumerator DoAction(BlockState state)
    {
        float pitch;
        float.TryParse(state.slotValues[1], out pitch);

        state.runtimeState.buzzerPitch = pitch;
        state.robot.writeFloat(Hamster.BUZZER, pitch);

        float seconds = GetSeconds(state);
        if (seconds > 0)
        {
            yield return new WaitForSeconds(seconds);
        }
    }

    protected virtual float GetSeconds(BlockState state)
    {
        return 0;
    }
}
