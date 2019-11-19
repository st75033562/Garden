public class MoveForwardBlock : MoveForSecsBlockBase
{
    protected override float GetMoveTime(BlockState state)
    {
        return 1.0f;
    }
}
