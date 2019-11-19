using Robomation;
using System.Collections;
using UnityEngine;

public abstract class PlayNoteForBeatsBlockBase : RobotBlockBase
{
    protected override IEnumerator DoAction(BlockState state)
    {
        //    state.robot.write(Hamster.NOTE, Hamster.NOTE_OFF);

        state.robot.write(Hamster.BUZZER, 0);
        state.robot.write(Hamster.NOTE, GetNote(state));
        yield return null;
        // convert beat to seconds
        //if (state.runtimeState == null)
        //{
        //    state.SetStateCount(1);
        //}
        //yield return new WaitForSeconds(GetBeats(state) / state.runtimeState.tempo * 60);

  //      state.robot.write(Hamster.NOTE, Hamster.NOTE_OFF);

    }

    protected abstract int GetNote(BlockState state);

    protected abstract float GetBeats(BlockState state);
}
