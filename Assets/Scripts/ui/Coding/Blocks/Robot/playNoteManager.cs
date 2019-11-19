using Robomation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playNoteManager : Singleton<playNoteManager>
{

    public void Play(RobotBlockBase.BlockState state, int note, float time)
    {
        StopAllCoroutines();
        StartCoroutine(CoroutinePlay(state, note, time));
    }

    IEnumerator CoroutinePlay(RobotBlockBase.BlockState state, int note, float time)
    {
        state.robot.write(Hamster.NOTE, Hamster.NOTE_OFF);

        state.robot.write(Hamster.BUZZER, 0);
        state.robot.write(Hamster.NOTE, note);

        // convert beat to seconds
        if (state.runtimeState == null)
        {
            state.SetStateCount(1);
        }
        yield return new WaitForSeconds(time);

        state.robot.write(Hamster.NOTE, Hamster.NOTE_OFF);
    }
}
