using System.Collections;

public class StartRobotCodeBlock : BlockBehaviour
{
    public override IEnumerator ActionBlock(ThreadContext context)
    {
        yield return CodeContext.gameboardService.StartRobotCode();
    }
}
