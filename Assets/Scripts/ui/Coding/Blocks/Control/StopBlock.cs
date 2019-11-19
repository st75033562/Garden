using Robomation;
using System.Collections;

public class StopBlock : RobotBlockBase
{
    protected override IEnumerator DoAction(BlockState state)
    {
        state.robot.write(Hamster.LEFT_WHEEL, 0);
        state.robot.write(Hamster.RIGHT_WHEEL, 0);

        yield break;
    }
}
