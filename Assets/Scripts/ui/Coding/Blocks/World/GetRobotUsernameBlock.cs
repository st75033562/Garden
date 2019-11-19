using System.Collections;
using System.Collections.Generic;

public class GetRobotUsernameBlock : InsertBlock
{
    public override IEnumerator GetNodeReturnValue(ThreadContext context, ValueWrapper<string> retValue)
    {
        var slotValues = new List<string>();
        yield return Node.GetSlotValues(context, slotValues);
        int robotIndex;
        int.TryParse(slotValues[0], out robotIndex);
        retValue.value = CodeContext.gameboardService.GetRobotNickname(robotIndex);
    }
}
