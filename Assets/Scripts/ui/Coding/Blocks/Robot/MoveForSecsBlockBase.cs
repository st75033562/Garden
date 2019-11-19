using Robomation;
using System.Collections;
using UnityEngine;

public abstract class MoveForSecsBlockBase : RobotBlockBase
{
    protected override IEnumerator DoAction(BlockState state)
    {
        var speedMultiplier = this.speedMultiplier;
        state.robot.write(Hamster.LEFT_WHEEL, (int)(state.runtimeState.leftWheelSpeed * speedMultiplier));
        state.robot.write(Hamster.RIGHT_WHEEL, (int)(state.runtimeState.rightWheelSpeed * speedMultiplier));

        yield return new WaitForSeconds(GetMoveTime(state));

        state.robot.write(Hamster.LEFT_WHEEL, 0);
        state.robot.write(Hamster.RIGHT_WHEEL, 0);
    }

    protected virtual float speedMultiplier
    {
        get { return 1.0f; }
    }

    protected abstract float GetMoveTime(BlockState state);
}
