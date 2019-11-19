public class TurnSecBlock : TurnForSecBlockBase
{
    protected override float GetTurnTime(BlockState state )
    {
        float time;
        float.TryParse(state.slotValues[1], out time);
        return time;
    }
}
