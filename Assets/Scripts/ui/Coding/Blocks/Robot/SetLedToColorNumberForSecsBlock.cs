public class SetLedToColorNumberForSecsBlock : SetLedToColorNumberBlock
{
    protected override float GetSeconds(BlockState state)
    {
        float duration;
        float.TryParse(state.slotValues[2], out duration);
        return duration;
    }
}
