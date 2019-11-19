public class MoveBackwardForSecsBlock : MoveForwardForSecsBlock
{
    protected override float speedMultiplier
    {
        get { return -1.0f; }
    }
}
