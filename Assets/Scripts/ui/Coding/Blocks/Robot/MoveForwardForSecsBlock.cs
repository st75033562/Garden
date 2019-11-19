public class MoveForwardForSecsBlock : MoveForSecsBlockBase
{
    protected override float GetMoveTime(BlockState state)
    {
        float moveTime;
        float.TryParse(state.slotValues[1], out moveTime);
        return moveTime;
	}
}
