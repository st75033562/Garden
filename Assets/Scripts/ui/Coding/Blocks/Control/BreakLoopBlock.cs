using System.Collections;

public class BreakLoopBlock : BlockBehaviour
{
    public override IEnumerator ActionBlock(ThreadContext context)
    {
        context.BreakLoop();
        yield break;
    }
}
