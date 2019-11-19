public class MoveForwardOnceBlock : MoveForSecsBlockBase
{
    protected override float GetMoveTime(BlockState state)
    {
        return 0.5f;
    }
}
