using System.Collections;
using System.Collections.Generic;

public class SetRobotScoreBlock : BlockBehaviour
{
    public override IEnumerator ActionBlock(ThreadContext context)
    {
        var slotValues = new List<string>();
        yield return Node.GetSlotValues(context, slotValues);
        int robotIndex, score;
        int.TryParse(slotValues[0], out robotIndex);
        int.TryParse(slotValues[1], out score);

        CodeContext.gameboardService.SetRobotScore(robotIndex, score);
    }
}
