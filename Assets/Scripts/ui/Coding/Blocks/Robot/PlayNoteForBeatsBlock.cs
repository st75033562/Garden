using Robomation;

public class PlayNoteForBeatsBlock : PlayNoteForBeatsBlockBase
{
    SelectNodePlugins m_BeatNode;
    SelectOctavePlugins m_BeatOctave;

    protected override void Start()
    {
        base.Start();

        m_BeatNode = GetComponentInChildren<SelectNodePlugins>();
        m_BeatOctave = GetComponentInChildren<SelectOctavePlugins>();
    }

    protected override int GetNote(BlockState state)
    {
        int note = m_BeatNode.GetNoteID();
        int octave = 0;
        int.TryParse(m_BeatOctave.GetMenuValue(), out octave);

        note = note + 4 + (octave - 1) * 12;
        return note;
    }

    protected override float GetBeats(BlockState state)
    {
        return BlockUtils.ParseBeat(state.slotValues[1]);
    }
}
