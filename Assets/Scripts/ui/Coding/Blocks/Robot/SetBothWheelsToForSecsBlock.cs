
public class SetBothWheelsToForSecsBlock : SetBothWheelsToBlock
{
    protected override float GetSeconds(BlockState state)
    {
        float seconds;
        float.TryParse(state.slotValues[3], out seconds);
        return seconds;
    }
}
