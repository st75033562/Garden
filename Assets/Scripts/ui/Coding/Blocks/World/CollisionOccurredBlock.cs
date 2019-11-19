using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class CollisionOccurredBlock : InsertBlock
{
    public override IEnumerator GetNodeReturnValue(ThreadContext context, ValueWrapper<string> retValue)
    {
        var slotValues = new List<string>();
        yield return Node.GetSlotValues(context, slotValues);
        int robotIndex;
        if (int.TryParse(slotValues[0], out robotIndex))
        {
            var objectIds = CodeContext.worldApi.GetRobotCollidedObjects(robotIndex, false);
            retValue.value = objectIds.Any().ToString();
        }
        else
        {
            retValue.value = false.ToString();
        }
    }
}
