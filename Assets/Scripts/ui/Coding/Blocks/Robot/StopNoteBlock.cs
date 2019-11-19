using Robomation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StopNoteBlock : BlockBehaviour
{
    public override IEnumerator ActionBlock(ThreadContext context)
    {
        var slotValues = new List<string>();
        var state = new RobotBlockBase.BlockState(CodeContext, 0, slotValues);
        if (state.robot == null)
        {
            yield break;
        }

        state.robot.write(Hamster.NOTE, Hamster.NOTE_OFF);
    }
}
