using Robomation;
using System.Collections;
using UnityEngine;

public class SetBothWheelsToBlock : RobotBlockBase
{
	protected override IEnumerator DoAction(BlockState state)
	{
        int left, right;
        int.TryParse(state.slotValues[1], out left);
        int.TryParse(state.slotValues[2], out right);

        state.runtimeState.leftWheelSpeed = left;
        state.robot.write(Hamster.LEFT_WHEEL, left);

        state.runtimeState.rightWheelSpeed = right;
        state.robot.write(Hamster.RIGHT_WHEEL, right);

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
