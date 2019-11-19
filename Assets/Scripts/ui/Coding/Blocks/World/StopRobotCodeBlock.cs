using System.Collections;

public class StopRobotCodeBlock : BlockBehaviour
{
    public override IEnumerator ActionBlock(ThreadContext context)
    {
        CodeContext.gameboardService.StopRobotCode();
        yield break;
    }
}
