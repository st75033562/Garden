public class TurnOnceBlock : TurnForSecBlockBase
{
    protected override float GetTurnTime(BlockState state)
    {
        return 0.5f;
    }
}
