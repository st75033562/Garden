public class TurnBlock : TurnForSecBlockBase
{
    protected override float GetTurnTime(BlockState state)
    {
        return 1.0f;
    }
}