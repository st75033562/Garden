public class SetLedToColorForSecsBlock : SetLedToColorBlock
{
    protected override float GetSeconds(BlockState state)
    {
        float duration;
        float.TryParse(state.slotValues[1], out duration);
        return duration;
    }
}
