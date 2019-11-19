class SetBuzzerToForSecsBlock : SetBuzzerToBlock
{
    protected override float GetSeconds(BlockState state)
    {
        float seconds;
        float.TryParse(state.slotValues[2], out seconds);
        return seconds;
    }
}
