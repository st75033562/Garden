using Robomation;
using System.Collections;
using UnityEngine;

public class BeepBlock : RobotBlockBase
{
    protected override IEnumerator DoAction(BlockState state)
    {
        state.robot.write(Hamster.BUZZER, 440);
        state.robot.write(Hamster.NOTE, 0);
        yield return new WaitForSeconds(1.0f);
    }
}
