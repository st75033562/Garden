using Robomation;

public class RestForBeatsBlock : PlayNoteForBeatsBlockBase
{
    protected override int GetNote(BlockState state)
    {
        return Hamster.NOTE_OFF;
    }

    protected override float GetBeats(BlockState state)
    {
        return BlockUtils.ParseBeat(state.slotValues[1]);
    }
}