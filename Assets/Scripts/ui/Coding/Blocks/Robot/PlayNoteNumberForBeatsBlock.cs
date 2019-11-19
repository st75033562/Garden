using Robomation;
using UnityEngine;

public class PlayNoteNumberForBeatsBlock : PlayNoteForBeatsBlockBase
{
    protected override int GetNote(BlockState state)
    {
        int note;
        int.TryParse(state.slotValues[1], out note);
        return Mathf.Clamp(note, Hamster.NOTE_OFF, Hamster.NOTE_C_8);
    }

    protected override float GetBeats(BlockState state)
    {
        return BlockUtils.ParseBeat(state.slotValues[2]);
    }
}
